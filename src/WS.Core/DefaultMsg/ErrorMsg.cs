using System;
using ProtoBuf;
using WS.Core.Base;

namespace WS.Core.DefaultMsg
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [Msg(0)]
    public class ErrorMsg : IMsg
    {
        public UInt16 MsgCommandId { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
