using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;
using DSManager.LuaBase;
using System.Collections.Concurrent;
using System.Net;

namespace DSManager
{
    class LoginServer : luabase, Entity
    {
        KChannel channel_matchserver;
        public ConcurrentDictionary<int, Player> Players = new ConcurrentDictionary<int, Player>();
        public LoginServer() : base("Loginserver")
        {
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.createorget(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                int id = 0;
                do
                {
                    id = RandomHelper.RandomNumber(int.MinValue, int.MaxValue);
                } while (Players.ContainsKey(id));
                Players.TryAdd(id,new Player(id,channel, this));
                Logger.log("onaccept");
            };
////////////////////////////////////////////////////////////////////////////////////////
            LuaTable remoteserver = GetValueFromLua<LuaTable>("remoteserver");
            nettype = (string)remoteserver["nettype"];
            serveraddr = (LuaTable)remoteserver[nettype];
            string serverip = (string)serveraddr["serverip"];
            port = (int)(Int64)serveraddr["port"];
            IPAddress ipAd = IPAddress.Parse(serverip);//local ip address  "172.16.5.188"
            channel_matchserver = Session.getnew().GetChannel(new IPEndPoint(ipAd, port));
            channel_matchserver.onUserLevelReceivedCompleted += (ref byte[] buffer) => {


            };
        }
        public void sendtomatchserver(byte command, byte[] buffer)
        {
            byte[] t = new byte[buffer.Length + 1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel_matchserver.Send(t);
        }
        void Entity.Begin()
        {

        }

        void Entity.End()
        {

        }

        void Entity.Update(uint delta)
        {
        }
    }
}
