﻿using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.RtmpHeaders;

namespace LiveStreamingServer.Rtmp.Core.Services.Contracts
{
    public interface IRtmpChunkMessageSenderService
    {
        void Send<TRtmpChunkMessageHeader>(
            IRtmpClientPeerContext peerContext,
            RtmpChunkBasicHeader basicHeader,
            TRtmpChunkMessageHeader messageHeader,
            Action<INetBuffer> payloadWriter,
            Action? callback = null) where TRtmpChunkMessageHeader : struct, IRtmpChunkMessageHeader;
    }
}