using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS.Core.IO
{
    public class WSConfig
    {
        static BufferManager bufferManager;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxBufferSize">最大缓存(M)</param>
        /// <param name="bufferSize">每个缓存切片大小(byte)</param>
        public static void Init(int maxBufferSize, int bufferSize, int perBlockBufferCount)
        {
            bufferManager = new BufferManager(maxBufferSize, bufferSize, perBlockBufferCount);
        }
        public static BufferManager BufferManager
        {
            get
            {
                if (bufferManager == null)
                {
                    throw new Exception("Web socket未完成配置初始化");
                }
                return bufferManager;
            }
        }
    }
}
