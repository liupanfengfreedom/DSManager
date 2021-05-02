using DSManager.LuaBase;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{

    public class ServertoDS : luabase, Entity
    {
        public List<DSMproxy> DSMchannel = new List<DSMproxy>();
        public List<MatchSeverProxy> matchserverproxys =  new List<MatchSeverProxy>();
        public ServertoDS() : base("ServertoDS")
        {
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.createorget(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                Logger.log("DSMchannel.Add");
                DSMchannel.Add(new DSMproxy(channel,this));
            };
//////////////////////////////////////////////////////////////////////
            server = GetValueFromLua<LuaTable>("serverv1");
            nettype = (string)server["nettype"];
            serveraddr = (LuaTable)server[nettype];
            port = (int)(Int64)serveraddr["port"];
            service = Session.createorget(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                Logger.log("matchserverproxys.Add");
                matchserverproxys.Add(new MatchSeverProxy(channel, this));
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
