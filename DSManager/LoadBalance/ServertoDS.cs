using DSManager.LuaBase;
using NLua;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{

    public class ServertoDS : luabase, Entity
    {
        public List<DSMproxy> DSMchannels = new List<DSMproxy>();
        public ConcurrentDictionary<int, MatchSeverProxy> matchserverproxys { get; private set; }
        public ServertoDS() : base("ServertoDS")
        {
            matchserverproxys = new ConcurrentDictionary<int, MatchSeverProxy>();
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.createorget(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                Logger.log("DSMchannel.Add");
                DSMchannels.Add(new DSMproxy(channel,this));
            };
//////////////////////////////////////////////////////////////////////
            server = GetValueFromLua<LuaTable>("serverv1");
            nettype = (string)server["nettype"];
            serveraddr = (LuaTable)server[nettype];
            port = (int)(Int64)serveraddr["port"];
            service = Session.createorget(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                Logger.log("matchserverproxys.Add");
                int id;
                do
                {
                    id = RandomHelper.RandomNumber(int.MinValue, int.MaxValue);
                } while (matchserverproxys.ContainsKey(id));
                matchserverproxys.TryAdd(id, new MatchSeverProxy(id, channel, this));
            };
        }
        public DSMproxy GetABestDSM()
        {
            DSMchannels.Sort((DSMproxy d1, DSMproxy d2)=>{ return d1.numberofds - d2.numberofds;});
            if (DSMchannels.Count > 0)
            {
                return DSMchannels[0];
            }
            else
            { 
                Logger.log("DSMchannels.Count == 0 ,this should not happen ");
            }
            return null;
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
