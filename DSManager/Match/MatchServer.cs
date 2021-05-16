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
        CREATEROOM,
        JOINROOM,
        JOINROOMFAILED,
        STARTGAME,
        PLAYEREXITQUEST,
        OTHERPLAYERINFOR,
    }
    class MatchServer : luabase, Entity
    {
        static MatchServer matchserver;
        KChannel channel_loadbalance;
        public ConcurrentDictionary<int, LoginServerProxy> LoginServers { get; private set; }
        ConcurrentDictionary<int, playerinfor> matchpool = new ConcurrentDictionary<int, playerinfor>();
        Filter filter = new Filter();
        public MatchServer() : base("MatchServer")
        {
            LoginServers = new ConcurrentDictionary<int, LoginServerProxy>();
            matchserver = this;
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.createorget(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                int id;
                do
                {
                    id = RandomHelper.RandomNumber(int.MinValue, int.MaxValue);
                } while (LoginServers.ContainsKey(id));
                LoginServers.TryAdd(id, new LoginServerProxy(id, channel, this));
                Logger.log("loinginserver in");
            };
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        await Task.Delay(50000);
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
                switch ((CMDLoadBalanceServer)buffer[0])
                {
                    case CMDLoadBalanceServer.CREATEDS:
                        int roomid = BitConverter.ToInt32(buffer, 1);
                        int dsport = BitConverter.ToInt32(buffer, 5);
                        string dswan = Encoding.getstring(buffer, 9, buffer.Length - 9);
                        Room room;
                        RoomManager.getsingleton().fightingRooms.TryGetValue(roomid,out room);
                        foreach (var v in room.players.Values)
                        {
                            if (v.offline)
                            {
                                Logger.log("this player is offline ,player id is  : "+ v.playerid);
                                continue;
                            }
                            LoginServerProxy lsp;
                            bool b = LoginServers.TryGetValue(v.loginserverproxyid,out lsp);
                            if (b)
                            {
                                byte[] wanbytes = Encoding.getbyte(dswan);
                                byte[] sumbuffer = new byte[12 + wanbytes.Length];
                                sumbuffer.WriteTo(0, v.playerid);
                                sumbuffer.WriteTo(4, v.side);
                                sumbuffer.WriteTo(8, dsport);
                                Array.Copy(wanbytes, 0, sumbuffer, 12, wanbytes.Length);
                                lsp.sendtologinserver((byte)CMDMatchServer.MATCHREQUEST, sumbuffer);
                            }
                            else
                            {

                            }
                        }
                        Logger.log("--roomid-- : " + roomid + "--dsport--" + dsport + "--dswan-- " + dswan);
                        break;
                    case CMDLoadBalanceServer.CREATEDSV1:
                        roomid = BitConverter.ToInt32(buffer, 1);
                        dsport = BitConverter.ToInt32(buffer, 5);
                        dswan = Encoding.getstring(buffer, 9, buffer.Length - 9);
                        RoomManager.getsingleton().CreatedRooms.TryGetValue(roomid, out room);
                        foreach (var v in room.players.Values)
                        {
                            if (v.offline)
                            {
                                Logger.log("this player is offline ,player id is  : " + v.playerid);
                                continue;
                            }
                            LoginServerProxy lsp;
                            bool b = LoginServers.TryGetValue(v.loginserverproxyid, out lsp);
                            if (b)
                            {
                                byte[] wanbytes = Encoding.getbyte(dswan);
                                byte[] sumbuffer = new byte[12 + wanbytes.Length];
                                sumbuffer.WriteTo(0, v.playerid);
                                sumbuffer.WriteTo(4, v.side);
                                sumbuffer.WriteTo(8, dsport);
                                Array.Copy(wanbytes, 0, sumbuffer, 12, wanbytes.Length);
                                lsp.sendtologinserver((byte)CMDMatchServer.STARTGAME, sumbuffer);
                            }
                            else
                            {

                            }
                        }
                        Logger.log("--roomid-- : " + roomid + "--dsport--" + dsport + "--dswan-- " + dswan);
                        break;
                    case CMDLoadBalanceServer.DESTROY:
                        Logger.log("CMDLoadBalanceServer.DESTROY");
                        break;
                    default:
                        break;
                }
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
        public void addtomatchpoolV1(playerinfor player)
        {       
            bool b = matchpool.TryAdd(player.playerid, player);
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
            /////////////////////////////////////////////////////////////////
            
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
