using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class LoginServerProxy
    {
        KChannel channel;
        MatchServer matchserver;
        public LoginServerProxy(KChannel channel, MatchServer matchserver)
        {
            this.channel = channel;
            this.matchserver = matchserver;
            this.channel.ondisconnect += () => {
                this.matchserver.LoginServers.Remove(this);
                Logger.log("ondisconnect");
            };
        }
        public void send(byte command, byte[] buffer)
        {
            byte[] t = new byte[buffer.Length + 1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel.Send(ref t);
        }
    }
}
