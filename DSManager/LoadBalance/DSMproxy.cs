//#define RTT 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
   public enum CMD { 
       WANIP,
       NEW_DS,
       KILL_DS,
    }
  public  class DSMproxy
    {
        KChannel channel;
        ServertoDS servertods;
        byte[] wan;
        public int numberofds { get; private set; }
        public DSMproxy(KChannel channel, ServertoDS servertods)
        {
            this.channel = channel;
            this.servertods = servertods;
            this.channel.ondisconnect += () => { 
                this.servertods.DSMchannels.Remove(this);
                // Console.WriteLine("ondisconnect");
                Logger.log("ondisconnect");

            };
            this.channel.onUserLevelReceivedCompleted += (ref byte[] buffer) =>
            {
#if RTT
                var str = System.Text.Encoding.UTF8.GetString(buffer);
                //Console.WriteLine(str);
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] buffer1 = asen.GetBytes(str + "server");
                send(ref buffer1);
#else
                switch ((CMD)buffer[0])
                {
                    case CMD.WANIP:
                        wan = new byte[buffer.Length-1];
                        Array.Copy(buffer,1,wan,0, wan.Length);
                        break;
                    case CMD.NEW_DS:
                        int matchserverid = BitConverter.ToInt32(buffer, 1);
                        int roomid = BitConverter.ToInt32(buffer, 5);
                        int port = BitConverter.ToInt32(buffer, 9);
                        MatchSeverProxy matchserverproxy;
                        servertods.matchserverproxys.TryGetValue(matchserverid,out matchserverproxy);
                        byte[] sumbuffer = new byte[8+wan.Length];
                        sumbuffer.WriteTo(0, roomid);
                        sumbuffer.WriteTo(4, port);
                        Array.Copy(wan,0, sumbuffer,8,wan.Length);
                        matchserverproxy.send((byte)CMDLoadBalanceServer.CREATEDS, sumbuffer);
                        Logger.log("matchserverid :" + matchserverid + " -- roomid :" + roomid + " -- port : "+ port);
                        break;
                    case CMD.KILL_DS:
                        break;
                    default:
                        break;
                }
#endif
            };
            send((byte)CMD.WANIP, BitConverter.GetBytes(0));
        }
        public void DS_request(int matchserverid, int roomid, CMD cmd)
        {
            switch (cmd)
            {
                case CMD.NEW_DS:
                    numberofds++;
                    break;
                case CMD.KILL_DS:
                    numberofds--;
                    break;
                default:
                    break;
            }
            byte[] sumbuffer = new byte[8];
            sumbuffer.WriteTo(0, matchserverid);
            sumbuffer.WriteTo(4, roomid);
            send((byte)cmd, sumbuffer);
        }
        void send(byte command , byte[] buffer)
        {
            byte[] t = new byte[buffer.Length+1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel.Send(t);
        }

    }
}
