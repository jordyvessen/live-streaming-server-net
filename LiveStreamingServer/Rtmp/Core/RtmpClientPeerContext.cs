﻿using LiveStreamingServer.Rtmp.Core.Contracts;
using LiveStreamingServer.Rtmp.Core.Utilities;
using System.Collections.Concurrent;

namespace LiveStreamingServer.Rtmp.Core
{
    public class RtmpClientPeerContext : IRtmpClientPeerContext
    {
        public RtmpClientPeerState State { get; set; } = RtmpClientPeerState.HandshakeC0;
        public HandshakeType HandshakeType { get; set; } = HandshakeType.SimpleHandshake;
        public int InChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;
        public int OutChunkSize { get; set; } = RtmpConstants.DefaultChunkSize;

        private ConcurrentDictionary<uint, IRtmpChunkStreamContext> _chunkStreamContexts = new();

        public IRtmpChunkStreamContext GetChunkStreamContext(uint chunkStreamId)
        {
            return _chunkStreamContexts.GetOrAdd(chunkStreamId, new RtmpChunkStreamContext(chunkStreamId));
        }
    }
}
