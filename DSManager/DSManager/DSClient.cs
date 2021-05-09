//#define RTT 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSManager.LuaBase;
using NLua;

namespace DSManager
{
    class DSClient : luabase, Entity
    {
        Timerhandler th;
        KChannel channel;
        string wan;
        string ts="";
        ~DSClient()
        { 
        }
        public DSClient() : base("DSClient")
        {
            ts = GetValueFromLua<string>("testmessage");
            wan = GetValueFromLua<string>("wan");

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
                switch ((CMD)buffer[0])
                {
                    case CMD.WANIP:
                        send((byte)CMD.WANIP,Encoding.getbyte(wan));
                        break;
                    case CMD.NEW_DS:
                        int matchserverid = BitConverter.ToInt32(buffer, 1);
                        int roomid = BitConverter.ToInt32(buffer, 5);
                        dsinfor infor = DSManager.GetSingleton().LaunchADS(roomid);
                        byte[] sumbuffer = new byte[12];
                        sumbuffer.WriteTo(0, matchserverid);
                        sumbuffer.WriteTo(4, roomid);
                        sumbuffer.WriteTo(8, infor.port);
                        send((byte)CMD.NEW_DS, sumbuffer);
                        break;
                    case CMD.KILL_DS:
                        matchserverid = BitConverter.ToInt32(buffer, 1); 
                        roomid = BitConverter.ToInt32(buffer, 5);
                        DSManager.GetSingleton().killds(roomid);
                        break;
                    default:
                        break;
                }
#endif

            };
        }

        public void Begin()
        {
            th= new Timerhandler((string s) => {
#if RTT
                string str = TimeHelper.Now().ToString();
                ////////////////////////////////////////////////////////////
                //ASCIIEncoding asen = new ASCIIEncoding();
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] buffer = utf8.GetBytes(str);
                channel.Send(ref buffer);
                //Console.WriteLine(s);
                //window_file_log.Log(ts);
#else

#endif
            }, ts, 100, true);
            //Global.GetComponent<Timer>().Add(th);
        }
        public void send(byte command,byte[] buffer)
        {
            byte[] t = new byte[buffer.Length + 1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel.Send(t);
        }
        public void End()
        {
        }
        public void Update(uint delta)
        {
        }
    }
}
