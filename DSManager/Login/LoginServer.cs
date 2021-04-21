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
    class LoginServer : luabase, Entity
    {
        public ConcurrentDictionary<int, Player> Players = new ConcurrentDictionary<int, Player>();
        public LoginServer() : base("Loginserver")
        {
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.get(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                int id = 0;
                do
                {
                    id = RandomHelper.RandomNumber(int.MinValue, int.MaxValue);
                } while (Players.ContainsKey(id));
                Players.TryAdd(id,new Player(id,channel, this));
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
