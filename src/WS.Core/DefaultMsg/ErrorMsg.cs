using System;
using ProtoBuf;
using WS.Core.Base;

namespace WS.Core.DefaultMsg
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [Msg(0)]
    public class ErrorMsg : IMsg
    {
        [ProtoMember(1)]
        public UInt16 MsgCommandId { get; set; }
        [ProtoMember(2)]
        public int ErrorCode { get; set; }
        [ProtoMember(3)]
        public string Message { get; set; }
    }
}
