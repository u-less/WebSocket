using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WS.Core.IO;

namespace WS.Core.Base
{
    public class WSExtensions
    {
        static int shortByteLength = sizeof(ushort);
        public static async Task Broadcast<T>(IEnumerable<WSSession> sessions, ushort command, T data)
        {
            List<Task> allTask = new List<Task>();
            using (var wsBuffer = WSConfig.BufferManager.Pull())
            {
                System.Buffer.BlockCopy(BitConverter.GetBytes(command), 0, wsBuffer.Buffer.Array, wsBuffer.Buffer.Offset, shortByteLength);
                using (var wm = new WSMemoryStream(wsBuffer, shortByteLength, wsBuffer.Buffer.Count - shortByteLength))
                {
                    Serializer.Serialize(wm, data);
                    ArraySegment<byte> buffer = new ArraySegment<byte>(wsBuffer.Buffer.Array, wsBuffer.Buffer.Offset, (int)wm.Position + shortByteLength);
                    foreach (var session in sessions)
                    {
                        var task = session.Send(buffer);
                        allTask.Add(task);
                    }
                    await Task.WhenAll(allTask.ToArray());
                }
            }
        }
        public static async Task Broadcast<T>(IEnumerable<WSSession> sessions, T data)
        {
            if (MsgMapping.TryGetCommand(typeof(T), out var command))
            {
                await Broadcast(sessions, command, data);
            }
            else
            {
                throw new Exception("发送的数据的class未添加MsgAttribute标记");
            }
        }
        public static async Task Broadcast(IEnumerable<WSSession> sessions, ushort command)
        {
            List<Task> allTask = new List<Task>();
            using (var wsBuffer = WSConfig.BufferManager.Pull())
            {
                System.Buffer.BlockCopy(BitConverter.GetBytes(command), 0, wsBuffer.Buffer.Array, wsBuffer.Buffer.Offset, shortByteLength);
                ArraySegment<byte> buffer = new ArraySegment<byte>(wsBuffer.Buffer.Array, wsBuffer.Buffer.Offset, shortByteLength);
                foreach (var session in sessions)
                {
                    var task = session.Send(buffer);
                    allTask.Add(task);
                }
                await Task.WhenAll(allTask.ToArray());
            }
        }
        public static async Task Broadcast(IEnumerable<WSSession> sessions, ArraySegment<byte> buffer)
        {
            List<Task> allTask = new List<Task>();
            foreach (var session in sessions)
            {
                var task = session.Send(buffer);
                allTask.Add(task);
            }
            await Task.WhenAll(allTask.ToArray());
        }
    }
}
