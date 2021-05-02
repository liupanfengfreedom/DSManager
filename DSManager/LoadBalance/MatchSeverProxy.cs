using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    enum CMDLoadBalanceServer
    {
        CREATEDS,
        DESTROY,
    }
    public class MatchSeverProxy
    {
        KChannel channel;
        ServertoDS servertods;
        public MatchSeverProxy(KChannel channel, ServertoDS servertods)
        {
            this.channel = channel;
            this.servertods = servertods;
            this.channel.ondisconnect += () => {
                // Console.WriteLine("ondisconnect");

            };
            this.channel.onUserLevelReceivedCompleted += (ref byte[] buffer) =>
            {
                switch ((CMDLoadBalanceServer)buffer[0])
                {
                    case CMDLoadBalanceServer.CREATEDS:
                        Logger.log("CMDLoadBalanceServer.CREATEDS");
                        break;
                    case CMDLoadBalanceServer.DESTROY:
                        Logger.log("CMDLoadBalanceServer.DESTROY");
                        break;
                    default:
                        break;
                }
            };
        }
        public void send(byte command, ref byte[] buffer)
        {
            byte[] t = new byte[buffer.Length + 1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel.Send(t);
        }

    }
}
