using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS.Core.IOC
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class IocExportAttribute : Attribute
    {
        public IocExportAttribute(Type type, bool singleInstance = false)
        {
            ContractType = type;
            SingleInstance = singleInstance;
        }
        public object ContractKey { get; set; }
        /// <summary>
        /// 如果绑定了名称,使用的时候只能通过名称来获取对象
        /// </summary>
        public string ContractName { get; set; }
        public Type ContractType { get; set; }
        /// <summary>
        /// >如果绑定了key,使用的时候只能通过名称来获取对象
        /// </summary>
        public bool SingleInstance { get; set; }
    }
}
