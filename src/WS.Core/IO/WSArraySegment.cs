using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS.Core.IO
{
    public class WSArraySegment:IDisposable
    {
        public WSArraySegment(WSBuffer wsBuffer, ArraySegment<byte> buffer)
        {
            this.WSBuffer = wsBuffer;
            this.Buffer = buffer;
        }
        public WSBuffer WSBuffer { get; set; }
        public ArraySegment<byte> Buffer { get; set; }

        public void Dispose()
        {
            this.WSBuffer.Push(this);
        }
    }
}
