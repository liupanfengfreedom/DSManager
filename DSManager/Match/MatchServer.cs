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
        PLAYEREXITQUEST,
    }
    class MatchServer : luabase, Entity
    {
        public List<LoginServerProxy> LoginServers = new List<LoginServerProxy>();
        ConcurrentDictionary<int, playerinfor> matchpool = new ConcurrentDictionary<int, playerinfor>();

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
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        await Task.Delay(1000);
                        Logger.log(matchpool.Count+ " :matchpool size");
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });

        }
        public void addtomatchpool(playerinfor player)
        {
            bool b = matchpool.TryAdd(player.playerid,player);
            if (b)
            { }
            else
            { 
                Logger.log("the same key already in matchpool");
            }
        }
        public void removefrompool(int id)
        {
            playerinfor player;
            bool b = matchpool.TryRemove(id,out player);
            if (b)
            { }
            else
            {
                Logger.log("remove key failed from matchpool");
            }
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
