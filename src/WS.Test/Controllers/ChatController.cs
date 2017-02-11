using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WS.Coin.Common;
using WS.Coin.Model;
using WS.Core.Base;
using WS.Core.IO;

namespace WS.Coin.Controllers
{
    [WSRoute(1, true)]
    public class ChatController : WSController
    {
        public ChatController()
        {
            RegisterWSHandler<Chat>(SendChat);
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
}
