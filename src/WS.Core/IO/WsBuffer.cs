using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace WS.Core.IO
{
    public class WSBuffer
    {
        byte[] buffer;
        ConcurrentQueue<WSArraySegment> ArraySegmentQueue;
        public WSBuffer(int bufferSize, int blockSize)
        {
            buffer = new byte[bufferSize * blockSize];
            ArraySegmentQueue = new ConcurrentQueue<WSArraySegment>();
            for (int i = 0; i < blockSize; i++)
            {
                int offset = i * bufferSize;
                ArraySegmentQueue.Enqueue(new WSArraySegment(this, new ArraySegment<byte>(buffer, offset, bufferSize)));
            }
        }
        public void Push(WSArraySegment segment)
        {
            if (!ArraySegmentQueue.Contains(segment))
                ArraySegmentQueue.Enqueue(segment);
        }
        public WSArraySegment Pull()
        {
            WSArraySegment segment;
            if (!ArraySegmentQueue.TryDequeue(out segment))
            {
                segment = null;
            }
            return segment;
        }
    }
}
