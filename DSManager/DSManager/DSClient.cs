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
        string ts="";
        public DSClient() : base("DSClient")
        {
            ts = GetValueFromLua<string>("testmessage");
            LuaTable remoteserver = GetValueFromLua<LuaTable>("remoteserver");
            string nettype = (string)remoteserver["nettype"];
            LuaTable serveraddr = (LuaTable)remoteserver[nettype];
            string serverip = (string)serveraddr["serverip"];
            int port = (int)(Int64)serveraddr["port"];
            IPAddress ipAd = IPAddress.Parse(serverip);//local ip address  "172.16.5.188"
            channel = Session.get().GetChannel(new IPEndPoint(ipAd, port));
            channel.onUserLevelReceivedCompleted += (ref byte[] buffer) => {
                var str = System.Text.Encoding.UTF8.GetString(buffer);
                Console.WriteLine(str);
            };
        }

        public void Begin()
        {
            th= new Timerhandler((string s) => {
                //ASCIIEncoding asen = new ASCIIEncoding();
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] buffer = utf8.GetBytes(s);

                channel.Send(ref buffer);

                Console.WriteLine(s);

            }, ts, 100, true);
            Global.GetComponent<Timer>().Add(th);
        }

        public void End()
        {
            
        }
        public void Update(uint delta)
        {
        }
    }
}
