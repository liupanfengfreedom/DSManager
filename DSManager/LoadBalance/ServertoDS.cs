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
        public ServertoDS() : base("ServertoDS")
        {
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.get(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                DSMchannel.Add(new DSMproxy(channel,this));
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
