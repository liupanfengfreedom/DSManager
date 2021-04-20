//#define RTT 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    enum CMD { 
       WANIP,
       NEW_DS,
       KILL_DS,
    }
  public  class DSMproxy
    {
        KChannel channel;
        ServertoDS servertods;
        public DSMproxy(KChannel channel, ServertoDS servertods)
        {
            this.channel = channel;
            this.servertods = servertods;
            this.channel.ondisconnect += () => { 
                this.servertods.DSMchannel.Remove(this);
               // Console.WriteLine("ondisconnect");
                window_file_log.Log("ondisconnect");

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
                        break;
                    case CMD.KILL_DS:
                        break;
                    default:
                        break;
                }
#endif
                };
        }
        public void send(byte command ,ref byte[] buffer)
        {
            byte[] t = new byte[buffer.Length+1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel.Send(ref t);
        }

    }
}
