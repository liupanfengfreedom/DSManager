using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DSManager.LuaBase;
using NLua;
namespace DSManager
{
    class PlayerSimulator : luabase, Entity
    {
        Timerhandler th;
        KChannel channel;
        string ts = "";
        public PlayerSimulator() : base("PlayerSimulator")
        {
            ts = GetValueFromLua<string>("testmessage");
            LuaTable remoteserver = GetValueFromLua<LuaTable>("remoteserver");
            string nettype = (string)remoteserver["nettype"];
            LuaTable serveraddr = (LuaTable)remoteserver[nettype];
            string serverip = (string)serveraddr["serverip"];
            int port = (int)(Int64)serveraddr["port"];
            IPAddress ipAd = IPAddress.Parse(serverip);//local ip address  "172.16.5.188"
            channel = Session.getnew().GetChannel(new IPEndPoint(ipAd, port));
            channel.onUserLevelReceivedCompleted += (ref byte[] buffer) => {
#if RTT
                var str = System.Text.Encoding.UTF8.GetString(buffer);
                str += TimeHelper.Now();
                string[] sd = new string[1];
                sd[0] = "server";
                string[] sa = str.Split(sd, StringSplitOptions.RemoveEmptyEntries);
                string l1 = sa[0].Substring(9);
                string l2 = sa[1].Substring(9);
                int r = Int32.Parse(l1);
                int r1 = Int32.Parse(l2);
                int rr = r1 - r;
                window_file_log.Log(rr.ToString());
                //Console.WriteLine(str);
#else
                switch ((CMDPlayer)buffer[0])
                {
                    case CMDPlayer.SINGUP:
                        Logger.log("Sing up ok");
                        break;
                    case CMDPlayer.LOGIN:
                        Logger.log("log in ok");
                        break;
                    case CMDPlayer.MATCHREQUEST:
                        int side = BitConverter.ToInt32(buffer, 1);
                        int dsport = BitConverter.ToInt32(buffer, 5);
                        string dswan = Encoding.getstring(buffer, 9, buffer.Length - 9);
                        Logger.log("player : --side-- : " + side + "--dsport--" + dsport + "--dswan-- " + dswan);
                        break;
                    default:
                        break;
                }
#endif

            };
        }
        public void Begin()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (!channel.isConnected)
                    {
                        await Task.Delay(100);
                    }
                    ts += RandomHelper.RandomNumber(0,int.MaxValue);
                    send((byte)CMDPlayer.LOGIN, Encoding.getbyte(ts));
                    await Task.Delay(2000);
                    send((byte)CMDPlayer.MATCHREQUEST, Encoding.getbyte(""));

                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
        }

        public void End()
        {
        }

        public void Update(uint delta)
        {
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
