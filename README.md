# WebSocket
Based on .net core websoket framework

websocket url:/

test url:/example/index.html

javascript:/example/chat.js

调试：可以直接实用.net core命令 dotnet run运行，也可以实用vs2017，但注意修改配置文件里的启动url地址和websocket的连接地址


备注：消息实用protobuffer进行序列化，js端使用需要先申明proto数据格式（参考例子里的proto_market.json）

Example usage:

`@消息定义`
```csharp
    [Msg(ReceiveCommand.ClientUserChat)]//申明消息Command
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class Chat : IMsg
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Content { get; set; }
        [ProtoMember(3)]
        public string SourceId { get; set; }
    }
```
`@申明控制器`
```csharp
    [WSRoute(1, true)]//申明控制器编号
    public class ChatController : WSController
    {
        public ChatController()//支持依赖注入
        {
            RegisterWSHandler<Chat>(SendChat);//注册Action,Action只允许两个参数，第一个为session,第二个为自定义消息
        }

        public async Task SendChat(WSSession session, IMsg messag)
        {
            var data = messag as Chat;
            if (data != null)
            {
                await Broadcast(data);
            }
        }
        public override Task Receive(WSSession session, WSArraySegment buffer, int count)
        {
            throw new NotImplementedException();
        }
    }
```
