using ProtoBuf;
using WS.Coin.Common;
using WS.Core.Base;

namespace WS.Coin.Model
{
    [Msg(ReceiveCommand.ClientUserChat)]
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
}
