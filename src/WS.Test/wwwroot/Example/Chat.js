$(function () {
    protobuf.load("proto/proto_market.json", function (err, root) {
        StartWS(root);
    });
});
var Controllers = {
    ChatController: 1
}
var ReceiveCommand = {
    Error: 0,
    ChatContent: 1//聊天内容
}
var SendCommand = {
    ClientUserChat: 1//发送聊天内容
}

function StartWS(root) {
    var Chat = root.lookup("Chat");

    var ws = new WebSocket("ws://192.168.199.236:8987/ws/market");
    ws.onopen = function (e) {
        console.log("Connection open...");
    };
    ws.binaryType = "arraybuffer";
    ws.onmessage = function (e) {
        if (e.data instanceof ArrayBuffer) {
            var cmdArray = new Uint8Array(e.data, 0, 2);
            var receiveBuffer = new Uint8Array(e.data, 2);
            var cmd = ByteToUnShort(cmdArray);
            if (cmd == ReceiveCommand.ChatContent) {
                //解析聊天内容
                var data = Chat.decode(receiveBuffer);
                console.log(data);
                $("#container").append("<p>" + data.Name + "说:" + data.Content + "</p>");
            } else if (cmd == ReceiveCommand.Error) {
                //处理异常消息
            }
        }
    };
    ws.onerror = function (e) {
        console.log('websocked error');
    }
    ws.onclose = function (e) {
        console.log("Connection closed", e);
        setTimeout(function () { StartWS(root); }, 2000);
    };

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
}
//构造发送bytes数据
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
function ByteToUnShort(b) {
    return (b[0] & 0xff) | ((b[1] & 0xff) << 8);
}