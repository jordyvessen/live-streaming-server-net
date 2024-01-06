﻿using LiveStreamingServerNet.Newtorking.Contracts;
using LiveStreamingServerNet.Rtmp.Contracts;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Attributes;
using LiveStreamingServerNet.Rtmp.RtmpEventHandlers.MessageDispatcher.Contracts;
using LiveStreamingServerNet.Rtmp.Services.Contracts;
using LiveStreamingServerNet.Rtmp.Utilities;
using LiveStreamingServerNet.Utilities;
using Microsoft.Extensions.Logging;

namespace LiveStreamingServerNet.Rtmp.RtmpEventHandlers.Media
{
    [RtmpMessageType(RtmpMessageType.VideoMessage)]
    internal class RtmpVideoMessageHandler : IRtmpMessageHandler
    {
        private readonly IRtmpStreamManagerService _streamManager;
        private readonly IRtmpMediaMessageManagerService _mediaMessageManager;
        private readonly ILogger _logger;

        public RtmpVideoMessageHandler(
            IRtmpStreamManagerService streamManager,
            IRtmpMediaMessageManagerService mediaMessageManager,
            ILogger<RtmpVideoMessageHandler> logger)
        {
            _streamManager = streamManager;
            _mediaMessageManager = mediaMessageManager;
            _logger = logger;
        }

        public Task<bool> HandleAsync(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpClientPeerContext peerContext,
            INetBuffer payloadBuffer,
            CancellationToken cancellationToken)
        {
            var publishStreamContext = peerContext.PublishStreamContext ??
                throw new InvalidOperationException("Stream is not yet published.");

            var hasHeader = CacheVideoSequence(chunkStreamContext, publishStreamContext, payloadBuffer);
            BroacastVideoMessageToSubscribers(chunkStreamContext, publishStreamContext, payloadBuffer, hasHeader);
            return Task.FromResult(true);
        }

        private void BroacastVideoMessageToSubscribers(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer,
            bool hasSequenceHeader)
        {
            if (hasSequenceHeader)
            {
                using var subscribers = _streamManager.GetSubscribersLocked(publishStreamContext.StreamPath);
                BroacastVideoMessageToSubscribers(chunkStreamContext, false, payloadBuffer, subscribers.Value);
            }
            else
            {
                var subscribers = _streamManager.GetSubscribers(publishStreamContext.StreamPath);
                BroacastVideoMessageToSubscribers(chunkStreamContext, true, payloadBuffer, subscribers);
            }
        }

        private void BroacastVideoMessageToSubscribers(
            IRtmpChunkStreamContext chunkStreamContext,
            bool isSkippable,
            INetBuffer payloadBuffer,
            IList<IRtmpClientPeerContext> subscribers)
        {
            _mediaMessageManager.EnqueueVideoMessage(
                subscribers,
                chunkStreamContext.MessageHeader.Timestamp,
                chunkStreamContext.MessageHeader.MessageStreamId,
                isSkippable,
                payloadBuffer.CopyAllTo);
        }

        private static bool CacheVideoSequence(
            IRtmpChunkStreamContext chunkStreamContext,
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer)
        {
            var firstByte = payloadBuffer.ReadByte();
            var frameType = (VideoFrameType)(firstByte >> 4);
            var codecId = (VideoCodecId)(firstByte & 0x0f);

            if (codecId == VideoCodecId.AVC)
            {
                var avcPackageType = (AVCPacketType)payloadBuffer.ReadByte();

                if (frameType == VideoFrameType.KeyFrame)
                {
                    ClearGroupOfPicturesCache(publishStreamContext);
                }

                if (frameType == VideoFrameType.KeyFrame && avcPackageType == AVCPacketType.SequenceHeader)
                {
                    publishStreamContext.VideoSequenceHeader = payloadBuffer.MoveTo(0).ReadBytes(payloadBuffer.Size);
                    payloadBuffer.MoveTo(0);
                    return true;
                }

                if (avcPackageType == AVCPacketType.NALU)
                {
                    CacheGroupOfPictures(publishStreamContext, payloadBuffer, chunkStreamContext.MessageHeader.Timestamp);
                }
            }

            payloadBuffer.MoveTo(0);

            return false;
        }

        private static void CacheGroupOfPictures(
            IRtmpPublishStreamContext publishStreamContext,
            INetBuffer payloadBuffer,
            uint timestamp)
        {
            var rentedBuffer = new RentedBuffer(payloadBuffer.Size);
            payloadBuffer.MoveTo(0).ReadBytes(rentedBuffer.Bytes, 0, payloadBuffer.Size);
            publishStreamContext.AddPictureCache(new PicturesCache(MediaType.Video, timestamp, rentedBuffer, payloadBuffer.Size));
        }

        private static void ClearGroupOfPicturesCache(IRtmpPublishStreamContext publishStreamContext)
        {
            publishStreamContext.ClearGroupOfPicturesCache();
        }
    }
}
