﻿using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.UserControls
{
    [RtmpMessageType(RtmpMessageType.UserControlMessage)]
    internal class RtmpUserControlMessageHandler : IRtmpMessageHandler
    {
        private readonly ILogger _logger;

        public RtmpUserControlMessageHandler(ILogger<RtmpUserControlMessageHandler> logger)
        {
            _logger = logger;
        }

        public Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
