using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS.Core.Base
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class WSRouteAttribute : Attribute
    {
        public WSRouteAttribute(UInt32 id, bool singleInstance)
        {
            this.Id = id;
            this.SingleInstance = singleInstance;
        }
        public UInt32 Id { get; set; }
        public bool SingleInstance { get; set; } = true;
    }
}
