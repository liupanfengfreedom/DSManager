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
                        break;
                    case CMD.NEW_DS:
                        int id = BitConverter.ToInt32(buffer, 1);
                        int port = BitConverter.ToInt32(buffer, 5);
                        Logger.log("id :"+id+ " -- port : "+ port);
                        break;
                    case CMD.KILL_DS:
                        break;
                    default:
                        break;
                }
#endif
                };
        }
        public void DS_request(int id, CMD cmd)
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
            send((byte)cmd, BitConverter.GetBytes(id));
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
