﻿using LiveStreamingServerNet.Rtmp.Contracts;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.CommandDispatcher
{
    internal abstract class RtmpCommandHandler
    {
        public abstract Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, object command, CancellationToken cancellationToken);
    }

    internal abstract class RtmpCommandHandler<TCommand> : RtmpCommandHandler
    {
        public sealed override Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, object command, CancellationToken cancellationToken)
        {
            return HandleAsync(chunkStreamContext, peerContext, (TCommand)command, cancellationToken);
        }

        public abstract Task<bool> HandleAsync(IRtmpChunkStreamContext chunkStreamContext, IRtmpClientPeerContext peerContext, TCommand command, CancellationToken cancellationToken);
    }
}
