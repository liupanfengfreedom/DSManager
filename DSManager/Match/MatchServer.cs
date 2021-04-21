using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;
using DSManager.LuaBase;

namespace DSManager
{
    class MatchServer : luabase, Entity
    {
        public List<LoginServerProxy> LoginServers = new List<LoginServerProxy>();
        public MatchServer() : base("Loginserver")
        {
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.get(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
             
                Logger.log("");
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
