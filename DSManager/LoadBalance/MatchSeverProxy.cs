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
        int id;
        public MatchSeverProxy(int id,KChannel channel, ServertoDS servertods)
        {
            this.id = id;
            this.channel = channel;
            this.servertods = servertods;
            this.channel.ondisconnect += () => {
                // Console.WriteLine("ondisconnect");
                MatchSeverProxy matchserverproxy;
               bool b = servertods.matchserverproxys.TryRemove(id,out matchserverproxy);
                if (b)
                {
                  Logger.log("remove a MatchSeverProxy sucessfully");
                }
                else
                { 
                  Logger.log("this should not happen when remove a MatchSeverProxy");
                }
            };
            this.channel.onUserLevelReceivedCompleted += (ref byte[] buffer) =>
            {
                switch ((CMDLoadBalanceServer)buffer[0])
                {
                    case CMDLoadBalanceServer.CREATEDS:
                        int roomid = BitConverter.ToInt32(buffer, 1);        
                        servertods.GetABestDSM().DS_request(id,roomid, CMD.NEW_DS);
                        Logger.log("CMDLoadBalanceServer.CREATEDS MatchSeverid : " + id + "--roomid--" + roomid);
                        break;
                    case CMDLoadBalanceServer.DESTROY:
                        Logger.log("CMDLoadBalanceServer.DESTROY");
                        break;
                    default:
                        break;
                }
            };
        }
        public void send(byte command,byte[] buffer)
        {
            byte[] t = new byte[buffer.Length + 1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel.Send(t);
        }

    }
}
