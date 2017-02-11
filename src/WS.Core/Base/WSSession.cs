using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using WS.Core.IO;

namespace WS.Core.Base
{
    public class WSSession : IDisposable
    {
        public Guid SessionId
        {
            get;
            set;
        }
        HttpContext context;
        WebSocket webSocket;
        ILogger logger;
        WSArraySegment receiveBuffer;
        public WSSession(HttpContext context, WebSocket webSocket, ILogger logger)
        {
            this.context = context;
            this.webSocket = webSocket;
            this.logger = logger;
            SessionId = Guid.NewGuid();
            receiveBuffer = WSConfig.BufferManager.Pull();
        }
        public ConcurrentDictionary<uint, IWSController> ControllerDict { get; set; } = new ConcurrentDictionary<uint, IWSController>();
        ConcurrentDictionary<string, object> sessionValues = new ConcurrentDictionary<string, object>();
        public void SetData<T>(string key, T data) where T : class
        {
            sessionValues.AddOrUpdate(key, data, (k, old) => { return data; });
        }
        public T GetData<T>(string key) where T : class
        {
            if (sessionValues.TryGetValue(key, out var value))
            {
                return value as T;
            }
            else
                return null;
        }
        public void RemoveData(string key)
        {
            sessionValues.TryRemove(key, out var value);
        }
        public HttpContext HttpContext { get { return context; } }
        public WebSocket Socket { get { return webSocket; } }
        public async Task Receive()
        {
            await ReceiveProcess(receiveBuffer, receiveBuffer.Buffer, 0);
        }
        private async Task ReceiveProcess(WSArraySegment wsBuffer, ArraySegment<byte> buffer, int count)
        {
            if (count >= wsBuffer.Buffer.Count)
            {
                logger.LogError("消息长度大于缓存大小");
            }
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Binary)
            {
                count += result.Count;
                if (result.EndOfMessage)
                {
                    await OnReceive(this, wsBuffer, count);
                    await Receive();
                }
                else
                {
                    var newBuffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + count, buffer.Count - count);
                    await ReceiveProcess(wsBuffer, newBuffer, count);
                }
            }
            if (result.CloseStatus.HasValue)
            {
                Dispose();
            }
        }
        static int uint16Length = sizeof(UInt16);
        public async Task Send<T>(UInt16 command, T data)
        {
            using (var wsBuffer = WSConfig.BufferManager.Pull())
            {
                System.Buffer.BlockCopy(BitConverter.GetBytes(command), 0, wsBuffer.Buffer.Array, wsBuffer.Buffer.Offset, uint16Length);
                using (var wm = new WSMemoryStream(wsBuffer, uint16Length, wsBuffer.Buffer.Count - uint16Length))
                {
                    Serializer.Serialize(wm, data);
                    ArraySegment<byte> buffer = new ArraySegment<byte>(wsBuffer.Buffer.Array, wsBuffer.Buffer.Offset, (int)wm.Position + uint16Length);
                    await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
        }
        public async Task Send<T>(T data) where T : IMsg
        {
            if (MsgMapping.TryGetCommand(typeof(T), out var command))
            {
                await Send(command, data);
            }
            else
            {
                throw new Exception("发送的数据的class未添加MsgAttribute标记");
            }
        }
        public async Task Send(UInt16 command)
        {
            using (var wsBuffer = WSConfig.BufferManager.Pull())
            {
                System.Buffer.BlockCopy(BitConverter.GetBytes(command), 0, wsBuffer.Buffer.Array, wsBuffer.Buffer.Offset, uint16Length);
                ArraySegment<byte> buffer = new ArraySegment<byte>(wsBuffer.Buffer.Array, wsBuffer.Buffer.Offset, uint16Length);
                await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
        public async Task Send(ArraySegment<byte> buffer)
        {
            await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
        }
        public delegate Task ReceiveHandle(WSSession session, WSArraySegment buffer, int count);
        public ReceiveHandle OnReceive { get; set; }
        public delegate void DisposeHandle(WSSession session);
        public DisposeHandle OnDispose { get; set; }
        public void Dispose()
        {
            receiveBuffer.Dispose();
            OnDispose(this);
            context.Abort();
            webSocket.Abort();
        }
    }
}
