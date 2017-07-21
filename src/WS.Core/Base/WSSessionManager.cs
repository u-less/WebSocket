using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WS.Core.DefaultMsg;
using WS.Core.IO;
using WS.Core.IOC;

namespace WS.Core.Base
{
    public class WSSessionManager
    {
        static Dictionary<UInt32, Type> controllerDict = new Dictionary<UInt32, Type>();
        static Func<Type, ILogger> LoggerGenerator;
        public void Init(IocManager ioc, Func<Type, ILogger> loggerGenerator)
        {
            var assemblyNameList = DependencyContext.Default.GetDefaultAssemblyNames();
            var wsRouterType = typeof(WSRouteAttribute);
            var controllerType = typeof(IWSController);
            foreach (var assemblyName in assemblyNameList)
            {
                var assembly = Assembly.Load(assemblyName);
                var allType = assembly.GetExportedTypes().Where(t => controllerType.IsAssignableFrom(t)).ToList();
                foreach (var type in allType)
                {
                    var typeInfo = type.GetTypeInfo();
                    var routerAttr = typeInfo.GetCustomAttribute(wsRouterType);
                    if (routerAttr != null)
                    {
                        var routerInfo = routerAttr as WSRouteAttribute;
                        if (routerInfo.SingleInstance)
                            ioc.Builder.RegisterType(type).Named(routerInfo.Id.ToString(), controllerType).SingleInstance();
                        else
                            ioc.Builder.RegisterType(type).Named(routerInfo.Id.ToString(), controllerType).InstancePerDependency();
                        controllerDict.Add(routerInfo.Id, type);
                    }
                }
            }
            iocManager = ioc;
            LoggerGenerator = loggerGenerator;
            SessionPool = new ConcurrentDictionary<string, WSSession>();
        }
        static ConcurrentDictionary<Type, ILogger> loggerIdct = new ConcurrentDictionary<Type, ILogger>();
        protected ILogger GetLogger(Type type)
        {
            if (!loggerIdct.TryGetValue(type, out var logger))
            {
                logger = LoggerGenerator(type);
                loggerIdct.TryAdd(type, logger);
            }
            return logger;
        }
        protected static Type sessionType = typeof(WSSession);
        public virtual async Task Accept(HttpContext context, WebSocket webSocket)
        {
            var session = new WSSession(context, webSocket, GetLogger(sessionType));
            session.OnDispose = SessionDispose;
            session.OnReceive = Receive;
            SessionPool.TryAdd(session.SessionId, session);
            await session.Receive();
        }
        public async Task Accept(WSSession session)
        {
            session.OnDispose = SessionDispose;
            session.OnReceive = Receive;
            SessionPool.TryAdd(session.SessionId, session);
            await session.Receive();
        }
        static int uint16Length = sizeof(UInt16);
        static int uint32Length = sizeof(UInt32);
        static int uint48Length = uint16Length + uint32Length;
        public async Task Receive(WSSession session, WSArraySegment buffer, int count)
        {
            var controllerId = BitConverter.ToUInt32(buffer.Buffer.Array, buffer.Buffer.Offset); //获取命令
            var controller = GetController(controllerId);
            if (controller != null)
            {
                controller.TryAddSession(session);
                var command = BitConverter.ToUInt16(buffer.Buffer.Array, buffer.Buffer.Offset + uint32Length);
                var surplus = count - uint48Length;
                if (surplus > 0)
                {
                    if (MsgMapping.TryGetMsgType(command, out var msgType))
                    {
                        IMsg data = null;
                        using (var wm = new WSMemoryStream(buffer, uint48Length, surplus))
                        {
                            try
                            {
                                data = Serializer.Deserialize(msgType, wm) as IMsg;
                            }
                            catch (Exception e)
                            {
                                await session.Send(new ErrorMsg() { MsgCommandId = command, ErrorCode = (int)ErrorCode.DeSerializerError, Message = e.Message });
                                GetLogger(typeof(WSSessionManager)).LogError(string.Format("Message:{0},StackTrace:{1}", e.Message, e.StackTrace));
                            }
                        }
                        if (data != null)
                        {
                            try
                            {
                                session.ControllerDict.TryAdd(controllerId, controller);
                                await controller.Receive(session, command, data);
                            }
                            catch (AggregateException e)
                            {
                                await session.Send(new ErrorMsg() { MsgCommandId = command, ErrorCode = (int)ErrorCode.DeSerializerError, Message = e.InnerException.Message });
                                controller.Logger.LogError(string.Format("Message:{0},StackTrace:{1}", e.InnerException.Message, e.InnerException.StackTrace));
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            session.ControllerDict.TryAdd(controllerId, controller);
                            await controller.Receive(session, buffer, count);
                        }
                        catch (AggregateException e)
                        {
                            await session.Send(new ErrorMsg() { MsgCommandId = command, ErrorCode = (int)ErrorCode.DeSerializerError, Message = e.InnerException.Message });
                            controller.Logger.LogError(string.Format("Message:{0},StackTrace:{1}", e.InnerException.Message, e.InnerException.StackTrace));
                        }
                    }
                }
                else
                    await controller.Receive(session, command);
            }
        }
        public virtual void SessionDispose(WSSession session)
        {
            foreach (var controller in session.ControllerDict.Values)
            {
                controller.SessionDispose(session);
            }
            SessionPool.TryRemove(session.SessionId, out session);
        }
        public static IWSController GetController(uint id)
        {
            if (controllerDict.TryGetValue(id, out var type))
            {
                var controller = Container.ResolveNamed<IWSController>(id.ToString());
                if (controller.Logger == null)
                {
                    controller.Logger = LoggerGenerator(type);
                }
                return controller;
            }
            else
                return null;
        }
        public static bool TryGetSession(string sessionId, out WSSession session)
        {
            return SessionPool.TryGetValue(sessionId, out session);
        }
        static IocManager iocManager;
        public static IContainer Container { get { return iocManager.Container; } }
        public static ConcurrentDictionary<string, WSSession> SessionPool { get; set; }
    }
}
