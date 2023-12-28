﻿using LiveStreamingServer.Newtorking;
using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Extensions;

namespace LiveStreamingServer.Rtmp.Core.RtmpHeaders
{
    public interface IRtmpChunkMessageHeader
    {
        int Size { get; }
        void Write(INetBuffer netBuffer);
        void UseExtendedTimestamp();
        bool HasExtendedTimestamp();
        uint GetTimestamp();
        void SetMessageLength(int messageLength);
    }

    public record struct RtmpChunkMessageHeaderType0 : IRtmpChunkMessageHeader
    {
        public const int kSize = 11;
        public int Size => kSize;

        public uint Timestamp { get; private set; }
        public int MessageLength { get; private set; }
        public byte MessageTypeId { get; }
        public uint MessageStreamId { get; }

        public RtmpChunkMessageHeaderType0(uint timestamp, int messageLength, byte messageTypeId, uint messageStreamId)
        {
            Timestamp = timestamp;
            MessageLength = messageLength;
            MessageTypeId = messageTypeId;
            MessageStreamId = messageStreamId;
        }

        public RtmpChunkMessageHeaderType0(uint timestamp, byte messageTypeId, uint messageStreamId)
        {
            Timestamp = timestamp;
            MessageLength = 0;
            MessageTypeId = messageTypeId;
            MessageStreamId = messageStreamId;
        }

        public static async Task<RtmpChunkMessageHeaderType0> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, kSize, cancellationToken);
            var timestampDelta = netBuffer.ReadUInt24BigEndian();
            var messageLength = (int)netBuffer.ReadUInt24BigEndian();
            var messageTypeId = netBuffer.ReadByte();
            var messageStreamId = netBuffer.ReadUInt32();

            return new RtmpChunkMessageHeaderType0(timestampDelta, messageLength, messageTypeId, messageStreamId);
        }

        public void SetMessageLength(int messageLength)
        {
            MessageLength = messageLength;
        }

        public void UseExtendedTimestamp()
        {
            Timestamp = 0xffffff;
        }

        public bool HasExtendedTimestamp()
        {
            return Timestamp >= 0xffffff;
        }

        public uint GetTimestamp()
        {
            return Timestamp;
        }

        public void Write(INetBuffer netBuffer)
        {
            netBuffer.WriteUInt24BigEndian(Timestamp);
            netBuffer.WriteUInt24BigEndian((uint)MessageLength);
            netBuffer.Write(MessageTypeId);
            netBuffer.Write(MessageStreamId);
        }
    }

    public record struct RtmpChunkMessageHeaderType1 : IRtmpChunkMessageHeader
    {
        public const int kSize = 7;
        public int Size => kSize;

        public uint TimestampDelta { get; private set; }
        public int MessageLength { get; private set; }
        public byte MessageTypeId { get; }
        public uint MessageStreamId { get; }

        public RtmpChunkMessageHeaderType1(uint timestampDelta, int messageLength, byte messageTypeId)
        {
            TimestampDelta = timestampDelta;
            MessageLength = messageLength;
            MessageTypeId = messageTypeId;
        }

        public RtmpChunkMessageHeaderType1(uint timestampDelta, byte messageTypeId)
        {
            TimestampDelta = timestampDelta;
            MessageLength = 0;
            MessageTypeId = messageTypeId;
        }

        public static async Task<RtmpChunkMessageHeaderType1> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, kSize, cancellationToken);

            var timestampDelta = netBuffer.ReadUInt24BigEndian();
            var messageLength = (int)netBuffer.ReadUInt24BigEndian();
            var messageTypeId = netBuffer.ReadByte();

            return new RtmpChunkMessageHeaderType1(timestampDelta, messageLength, messageTypeId);
        }

        public void SetMessageLength(int messageLength)
        {
            MessageLength = messageLength;
        }

        public void UseExtendedTimestamp()
        {
            TimestampDelta = 0xffffff;
        }

        public bool HasExtendedTimestamp()
        {
            return TimestampDelta >= 0xffffff;
        }

        public uint GetTimestamp()
        {
            return TimestampDelta;
        }

        public void Write(INetBuffer netBuffer)
        {
            netBuffer.WriteUInt24BigEndian(TimestampDelta);
            netBuffer.WriteUInt24BigEndian((uint)MessageLength);
            netBuffer.Write(MessageTypeId);
        }
    }

    public record struct RtmpChunkMessageHeaderType2 : IRtmpChunkMessageHeader
    {
        public const int kSize = 3;
        public int Size => kSize;

        public uint TimestampDelta { get; private set; }

        public RtmpChunkMessageHeaderType2(uint TimestampDelta)
        {
            this.TimestampDelta = TimestampDelta;
        }

        public static async Task<RtmpChunkMessageHeaderType2> ReadAsync(INetBuffer netBuffer, ReadOnlyNetworkStream networkStream, CancellationToken cancellationToken)
        {
            await netBuffer.CopyStreamData(networkStream, kSize, cancellationToken);

            var timestampDelta = netBuffer.ReadUInt24BigEndian();

            return new RtmpChunkMessageHeaderType2(timestampDelta);
        }

        public void SetMessageLength(int messageLength) { }

        public void UseExtendedTimestamp()
        {
            TimestampDelta = 0xffffff;
        }

        public bool HasExtendedTimestamp()
        {
            return TimestampDelta >= 0xffffff;
        }

        public uint GetTimestamp()
        {
            return TimestampDelta;
        }

        public void Write(INetBuffer netBuffer)
        {
            netBuffer.WriteUInt24BigEndian(TimestampDelta);
        }
    }
}
