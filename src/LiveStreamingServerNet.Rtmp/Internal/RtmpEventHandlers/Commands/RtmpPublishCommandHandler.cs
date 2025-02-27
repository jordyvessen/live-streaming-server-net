﻿using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher;
using LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands.Dispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.Internal.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Services.Extensions;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;
using LiveStreamingServerNet.Rtmp.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.Internal.RtmpEventHandlers.Commands
{
    internal record RtmpPublishCommand(double TransactionId, IDictionary<string, object> CommandObject, string PublishingName, string PublishingType);

    [RtmpCommand("publish")]
    internal class RtmpPublishCommandHandler : RtmpCommandHandler<RtmpPublishCommand>
    {
        private readonly IServiceProvider _services;
        private readonly IRtmpServerContext _serverContext;
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpCommandMessageSenderService _commandMessageSender;
        private readonly IRtmpServerStreamEventDispatcher _eventDispatcher;
        private readonly ILogger _logger;

        public RtmpPublishCommandHandler(
            IServiceProvider services,
            IRtmpServerContext serverContext,
            IRtmpStreamManagerService streamManager,
            IRtmpCommandMessageSenderService commandMessageSender,
            IRtmpServerStreamEventDispatcher eventDispatcher,
            ILogger<RtmpPublishCommandHandler> logger)
        {
            _services = services;
            _serverContext = serverContext;
            _streamManager = streamManager;
            _commandMessageSender = commandMessageSender;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        public override async ValueTask<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientContext clientContext,
            RtmpPublishCommand command,
            CancellationToken cancellationToken)
        {
            _logger.Publish(clientContext.Client.ClientId, command.PublishingName, command.PublishingType);

            if (clientContext.StreamId == null)
                throw new InvalidOperationException("Stream is not yet created.");

            var (streamPath, streamArguments) = ParsePublishContext(command, clientContext);

            var authorizationResult = await AuthorizeAsync(clientContext, command, chunkStreamContext, streamPath, streamArguments);

            if (authorizationResult.IsAuthorized)
            {
                streamPath = authorizationResult.StreamPathOverride ?? streamPath;
                streamArguments = authorizationResult.StreamArgumentsOverride ?? streamArguments;

                StartPublishing(clientContext, command, chunkStreamContext, streamPath, streamArguments);
            }

            return true;
        }

        private async ValueTask<AuthorizationResult> AuthorizeAsync(
            IRtmpClientContext clientContext,
            string streamPath,
            IDictionary<string, string> streamArguments,
            string publishingType)
        {
            var authorizationHandler = _services.GetService<IRtmpAuthorizationHandler>();

            if (authorizationHandler != null)
                return await authorizationHandler.AuthorizePublishingAsync(clientContext.Client, streamPath, streamArguments, publishingType);

            return AuthorizationResult.Authorized();
        }

        private static (string StreamPath, IDictionary<string, string> StreamArguments)
            ParsePublishContext(RtmpPublishCommand command, IRtmpClientContext clientContext)
        {
            var (streamName, arguments) = StreamUtilities.ParseStreamPath(command.PublishingName);

            var streamPath = $"/{string.Join('/',
                new string[] { clientContext.AppName, streamName }.Where(s => !string.IsNullOrEmpty(s)).ToArray())}";

            return (streamPath, arguments);
        }

        private async ValueTask<AuthorizationResult> AuthorizeAsync(
            IRtmpClientContext clientContext,
            RtmpPublishCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            if (streamArguments.TryGetValue("code", out var authCode) && authCode == _serverContext.AuthCode)
                return AuthorizationResult.Authorized();

            var result = await AuthorizeAsync(clientContext, streamPath, streamArguments, command.PublishingType);

            if (!result.IsAuthorized)
            {
                _logger.AuthorizationFailed(clientContext.Client.ClientId, streamPath, command.PublishingType, result.Reason);
                SendAuthorizationFailedCommandMessage(clientContext, chunkStreamContext, result.Reason);
            }

            return result;
        }

        private bool StartPublishing(
            IRtmpClientContext clientContext,
            RtmpPublishCommand command,
            IRtmpChunkStreamContext chunkStreamContext,
            string streamPath,
            IDictionary<string, string> streamArguments)
        {
            var startPublishingResult = _streamManager.StartPublishingStream(clientContext, streamPath, streamArguments, out _);

            switch (startPublishingResult)
            {
                case PublishingStreamResult.Succeeded:
                    _logger.PublishingStarted(clientContext.Client.ClientId, streamPath, command.PublishingType);
                    SendPublishingStartedMessage(clientContext, chunkStreamContext);
                    _eventDispatcher.RtmpStreamPublishedAsync(clientContext, streamPath, streamArguments.AsReadOnly());
                    return true;

                case PublishingStreamResult.AlreadySubscribing:
                    _logger.AlreadySubscribing(clientContext.Client.ClientId, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already subscribing.");
                    return false;

                case PublishingStreamResult.AlreadyPublishing:
                    _logger.AlreadyPublishing(clientContext.Client.ClientId, streamPath);
                    SendBadConnectionCommandMessage(clientContext, chunkStreamContext, "Already publishing.");
                    return false;

                case PublishingStreamResult.AlreadyExists:
                    _logger.StreamAlreadyExists(clientContext.Client.ClientId, streamPath, command.PublishingType);
                    SendAlreadyExistsCommandMessage(clientContext, chunkStreamContext);
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(startPublishingResult), startPublishingResult, null);
            }
        }

        private void SendAlreadyExistsCommandMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadName,
                "Stream already exists.");
        }

        private void SendBadConnectionCommandMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishBadConnection,
                reason);
        }

        private void SendAuthorizationFailedCommandMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext, string reason)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Error,
                RtmpStatusCodes.PublishUnauthorized,
                reason);
        }

        private void SendPublishingStartedMessage(IRtmpClientContext clientContext, IRtmpChunkStreamContext chunkStreamContext)
        {
            _commandMessageSender.SendOnStatusCommandMessage(
                clientContext,
                chunkStreamContext.ChunkStreamId,
                RtmpArgumentValues.Status,
                RtmpStatusCodes.PublishStart,
                "Publishing started.");
        }
    }
}
