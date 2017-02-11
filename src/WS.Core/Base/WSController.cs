using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;
using ProtoBuf;
using System.Threading;
using WS.Core.IO;

namespace WS.Core.Base
{
    public abstract class WSController : IWSController
    {
        protected ConcurrentDictionary<Guid, WSSession> SessionPool { get; set; }
        public WSController()
        {
            SessionPool = new ConcurrentDictionary<Guid, WSSession>();
        }
        public ILogger Logger { get; set; }
        public ICollection<WSSession> SessionList { get { return SessionPool.Values; } }
        public void TryAddSession(WSSession session)
        {
            SessionPool.TryAdd(session.SessionId, session);
        }
        public Task Broadcast<T>(ushort command, T data)
        {
            return WSExtensions.Broadcast(SessionPool.Values, command, data);
        }
        public Task Broadcast(ushort command)
        {
            return WSExtensions.Broadcast(SessionPool.Values, command);
        }
        public Task Broadcast<T>(T data) where T : IMsg
        {
            if (MsgMapping.TryGetCommand(typeof(T), out var cmd))
                return Broadcast(cmd, data);
            else
                throw new Exception("发送的数据的class未添加MsgAttribute标记");
        }
        public Task Broadcast(ArraySegment<byte> buffer)
        {
            return WSExtensions.Broadcast(SessionPool.Values, buffer);
        }

        Dictionary<ushort, Func<WSSession, IMsg, Task>> WsActionDict = new Dictionary<ushort, Func<WSSession, IMsg, Task>>();
        protected void RegisterWSHandler<T>(Func<WSSession, IMsg, Task> func) where T : IMsg
        {
            if (MsgMapping.TryGetCommand(typeof(T), out var cmd))
                WsActionDict.Add(cmd, func);
            else
                throw new Exception("接收的数据的class未添加MsgAttribute标记");
        }
        Dictionary<ushort, Func<Task>> NoParameterWsActionDict = new Dictionary<ushort, Func<Task>>();
        protected void RegisterWSHandler(ushort command, Func<Task> func)
        {
            NoParameterWsActionDict.Add(command, func);
        }
        public abstract Task Receive(WSSession session, WSArraySegment buffer, int count);

        public async Task Receive(WSSession session, ushort command, IMsg data)
        {
            if (WsActionDict.TryGetValue(command, out var func))
            {
                await func(session, data);
            }
        }
        public async Task Receive(WSSession session, ushort command)
        {
            if (NoParameterWsActionDict.TryGetValue(command, out var func))
            {
                await func();
            }
        }
        public virtual void SessionDispose(WSSession session)
        {
            SessionPool.TryRemove(session.SessionId, out session);
        }
    }
}
