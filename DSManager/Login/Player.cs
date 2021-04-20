using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    enum CMDPlayer
    {
        SINGUP,
        LOGIN,
        MATCHREQUEST,
    }
    class Player
    {
        KChannel channel;
        LoginServer loginserver;
        public Player(KChannel channel, LoginServer loginserver)
        {
            this.channel = channel;
            this.loginserver = loginserver;
            this.channel.ondisconnect += () => {
                this.loginserver.Players.Remove(this);
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
                var str = Encoding.getstring(buffer, 1, buffer.Length - 1);
                switch ((CMDPlayer)buffer[0])
                {
                    case CMDPlayer.SINGUP:
                        Logger.log("Sing up ");
                        //write data base
                        break;
                    case CMDPlayer.LOGIN:
                        Logger.log("log in ");
                        Logger.log(str);
                        //read data base
                        send((byte)CMDPlayer.LOGIN, Encoding.getbyte("hi"));
                        break;
                    case CMDPlayer.MATCHREQUEST:
                        Logger.log("match ");
                        break;
                    default:
                        break;
                }
#endif
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
