using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;
using DSManager.LuaBase;
using System.Collections.Concurrent;

namespace DSManager
{
    enum CMDMatchServer
    {
        MATCHREQUEST,
    }
    class MatchServer : luabase, Entity
    {
        public List<LoginServerProxy> LoginServers = new List<LoginServerProxy>();
        public ConcurrentDictionary<int, playerinfor> Players = new ConcurrentDictionary<int, playerinfor>();

        public MatchServer() : base("MatchServer")
        {
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.get(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                LoginServers.Add(new LoginServerProxy(channel, this));
                Logger.log("loinginserver in");
            };
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
