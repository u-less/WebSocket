using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS.Coin.Common
{
    public class Consts
    {
        public const string MarketHandlerRouter = "/ws/market";
        public const string MarketDepthExchange = "MarketDepth";
        public const string MarketDepthRoutingKey = "MarketDepth"; 
        public const string KLineExchange = "KLine";
        public const string KLineRoutingKey = "KLine";
        public const string ChatExchange = "Chat";
        public const string ChatRoutingKey = "Chat";
    }
}
