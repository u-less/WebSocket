using WS.Core.IOC;

namespace WS.Coin
{
    public class GlobalConfig
    {
        /// <summary>
        /// 默认Ioc管理器
        /// </summary>
        public static IocManager DefaultIocManager { get; set; } = new IocManager();
    }
}
