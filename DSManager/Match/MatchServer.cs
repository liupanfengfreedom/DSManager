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
    enum CMDMatchServer
    {
        MATCHREQUEST,
        PLAYEREXITQUEST,
    }
    class MatchServer : luabase, Entity
    {
        static MatchServer matchserver;
        KChannel channel_loadbalance;
        public List<LoginServerProxy> LoginServers = new List<LoginServerProxy>();
        ConcurrentDictionary<int, playerinfor> matchpool = new ConcurrentDictionary<int, playerinfor>();
        Filter filter = new Filter();
        public MatchServer() : base("MatchServer")
        {
            matchserver = this;
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.createorget(port);
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
                        await Task.Delay(5000);
                        Logger.log(matchpool.Count+ " :matchpool size");
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
 ////////////////////////////////////////////////////////////////////////////////////////
            LuaTable remoteserver = GetValueFromLua<LuaTable>("remoteserver");
            nettype = (string)remoteserver["nettype"];
            serveraddr = (LuaTable)remoteserver[nettype];
            string serverip = (string)serveraddr["serverip"];
            port = (int)(Int64)serveraddr["port"];
            IPAddress ipAd = IPAddress.Parse(serverip);//local ip address  "172.16.5.188"
            channel_loadbalance = Session.getnew().GetChannel(new IPEndPoint(ipAd, port));
            channel_loadbalance.onUserLevelReceivedCompleted += (ref byte[] buffer) => {


            };
        } 

        public void addtomatchpool(playerinfor player)
        {
            filter.sort(player.SimulateInforStr, player);
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
            {
                player.offline = true;
            }
            else
            {
                Logger.log("remove key failed from matchpool");
            }
        }
        public void sendtoloadbalanceserver(byte command, byte[] buffer)
        {
            byte[] t = new byte[buffer.Length + 1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel_loadbalance.Send(t);
        }
        public static  MatchServer  getsingleton()
        {
            return matchserver;
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
