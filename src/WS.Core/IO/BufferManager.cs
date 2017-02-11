using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace WS.Core.IO
{
    public class BufferManager
    {
        int maxBlockCount;
        int bufferSize;
        int perBlockBufferCount;
        ConcurrentBag<WSBuffer> bufferList = new ConcurrentBag<WSBuffer>();
        public BufferManager(int maxBufferSize, int bufferSize, int perBlockBufferCount)
        {
            this.bufferSize = bufferSize;
            this.perBlockBufferCount = perBlockBufferCount;
            maxBlockCount = maxBufferSize / perBlockBufferCount;
            if (maxBlockCount == 0) maxBlockCount = 1;
            bufferList.Add(new WSBuffer(bufferSize, perBlockBufferCount));
            maxBlockCount--;
        }
        public WSArraySegment Pull()
        {
            WSArraySegment segment = null;
            foreach (var wb in bufferList)
            {
                segment = wb.Pull();
                if (segment != null) break;
            }
            if (segment == null)
            {
                if (Interlocked.Decrement(ref maxBlockCount) > 0)
                {
                    var wb = new WSBuffer(bufferSize, perBlockBufferCount);
                    segment = wb.Pull();
                    bufferList.Add(wb);
                }
            }
            return segment;
        }
    }
}
