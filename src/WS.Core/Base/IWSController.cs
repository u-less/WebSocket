using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WS.Core.IO;

namespace WS.Core.Base
{
    public interface IWSController
    {
        ILogger Logger { get; set; }
        void TryAddSession(WSSession session);
        ICollection<WSSession> SessionList { get; }
        Task Broadcast(ushort command);
        Task Broadcast<T>(T data) where T : IMsg;
        Task Broadcast<T>(ushort command, T data);
        Task Broadcast(ArraySegment<byte> buffer);
        Task Receive(WSSession session, WSArraySegment buffer, int count);
        Task Receive(WSSession session, UInt16 command, IMsg data);
        Task Receive(WSSession session, ushort command);
        void SessionDispose(WSSession session);
    }
}
