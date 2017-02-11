using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace WS.Core.IO
{
    public class WSMemoryStream : MemoryStream
    {
        WSArraySegment buffer;
        public WSMemoryStream(WSArraySegment buffer) : base(buffer.Buffer.Array, buffer.Buffer.Offset, buffer.Buffer.Count)
        {
            this.buffer = buffer;
        }
        public WSMemoryStream(WSArraySegment buffer,int offset, int count) : base(buffer.Buffer.Array, buffer.Buffer.Offset+offset, count)
        {
            this.buffer = buffer;
        }
    }
}
