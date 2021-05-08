using ProtoBuf;
using System;
using System.IO;
using System.Buffers;
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
        EXITREQUEST,
    }
    class Player
    {
        KChannel channel;
        LoginServer loginserver;
        int id;
        public string playerinfor { get;private set;}
        public Player(int id,KChannel channel, LoginServer loginserver)
        {
            this.id = id;
            this.channel = channel;
            this.loginserver = loginserver;
            this.channel.ondisconnect += () => {
                Player v;
                this.loginserver.Players.TryRemove(id,out v);
                loginserver.sendtomatchserver((byte)CMDMatchServer.PLAYEREXITQUEST, BitConverter.GetBytes(id));

                Logger.log(id + " :ondisconnect");
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
                        ////////////////////////////////////////////////////////////////////
                        //read data base
                        //simulate playerinfor
                        int simulateddata;
                        for (int i = 0; i < 2; i++)
                        {
                            simulateddata = RandomHelper.RandomNumber(1, 3);
                            playerinfor += simulateddata.ToString()+"???";
                        }

////////////////////////////////////////////////////////////////////////////////////
                        //ack
                        send((byte)CMDPlayer.LOGIN, Encoding.getbyte("hi"));
                        break;
                    case CMDPlayer.MATCHREQUEST:
                        Logger.log("match ");
                        Logger.log(playerinfor);
                        var playerinfor_ = new playerinfor {
                            playerid = id,
                            SimulateInforStr = playerinfor,
                            SimulateInforInt = 1234
                        };
                        MemoryStream ms = new MemoryStream();
                        Serializer.Serialize(ms, playerinfor_);
                        loginserver.sendtomatchserver((byte)CMDMatchServer.MATCHREQUEST, ms.ToArray());
                        break;
                    case CMDPlayer.EXITREQUEST:
                        Logger.log("CMDPlayer.EXITREQUEST");
                        loginserver.sendtomatchserver((byte)CMDMatchServer.PLAYEREXITQUEST, BitConverter.GetBytes(id));

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
            channel.Send(t);
        }
    }
}
