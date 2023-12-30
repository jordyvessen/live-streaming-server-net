﻿using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher;
using LiveStreamingServer.Rtmp.Core.RtmpEventHandler.CommandDispatcher.Attributes;
using LiveStreamingServer.Rtmp.Core.RtmpEvents;

namespace LiveStreamingServer.Rtmp.Core.RtmpEventHandler.Commands
{
    public record RtmpCallCommand(double TransactionId, IDictionary<string, object> CommandObject, IDictionary<string, object> Arguments);

    [RtmpCommand("call")]
    public class RtmpCallCommandHandler : RtmpCommandHandler<RtmpCallCommand>
    {
        public override Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            RtmpCallCommand command,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
