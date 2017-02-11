using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS.Core.Base
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MsgAttribute:Attribute
    {
        public MsgAttribute(UInt16 command)
        {
            this.Command = command;
        }
        public UInt16 Command
        {
            get;
            set;
        }
    }
}
