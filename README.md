# WebSocket
Based on .net core websoket framework

websocket url:/

test url:/example/index.html

javascript:/example/chat.js

调试：可以直接实用.net core命令 dotnet run运行，也可以实用vs2017，但注意修改配置文件里的启动url地址和websocket的连接地址


备注：消息采用protobuffer进行序列化，js端使用需要先申明proto数据格式（参考例子里的proto_market.json）

内存调优配置：appsettings.json=>WebSocket

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
`@javascript端调用`
```javascript
//发送聊天内容
$("#btn_Send").unbind("click").click(function () {
    var content = $("#txt_Content").val();
    var name = $("#txt_Name").val();
    var newchat = Chat.create({ Name: name, Content: content, SourceId: "聊天室1" });
    var dataBuffer = Chat.encode(newchat).finish();
    var buffer = GenerateCmdBuffer(Controllers.ChatController, SendCommand.ClientUserChat, dataBuffer);

    if (ws.readyState === WebSocket.OPEN) {
        ws.send(buffer);
    } else {

    }
});
//构造发送数据(pare1:控制器id,pare2:消息command，pare3:对象的byte数组)
function GenerateCmdBuffer(controller, command, dataBuffer) {
    var controllerLittleEndian = new dcodeIO.ByteBuffer(4).writeUint32(controller, 0).flip();
    var controllerBigEndian = new Uint8Array(4);
    controllerBigEndian[0] = controllerLittleEndian.view[3];
    controllerBigEndian[1] = controllerLittleEndian.view[2];
    controllerBigEndian[2] = controllerLittleEndian.view[1];
    controllerBigEndian[3] = controllerLittleEndian.view[0];
    var commandLittleEndian = new dcodeIO.ByteBuffer(2).writeUint16(command, 0).flip();
    var commandBigEndian = new Uint8Array(2);
    commandBigEndian[0] = commandLittleEndian.view[1];
    commandBigEndian[1] = commandLittleEndian.view[0];
    var allBuffer = dcodeIO.ByteBuffer.concat([controllerBigEndian, commandBigEndian, dataBuffer], "binary");
    return allBuffer.view;
}
```
