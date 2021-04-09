using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
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
                Console.WriteLine("ondisconnect");
            };
            this.channel.onUserLevelReceivedCompleted += (ref byte[] buffer) =>
            {
                var str = System.Text.Encoding.UTF8.GetString(buffer);
                //Console.WriteLine(str);
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] buffer1 = asen.GetBytes(str + "fromserver");
                send(ref buffer1);
            };
        }
        public void send(ref byte[] buffer)
        {
            channel.Send(ref buffer);
        }
    }
}
