using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;
using DSManager.LuaBase;

namespace DSManager
{

    class LoginServer : luabase, Entity
    {
        public List<Player> Players = new List<Player>();
        public LoginServer() : base("Loginserver")
        {
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.get(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                Players.Add(new Player(channel, this));
                Logger.log("onaccept");
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
