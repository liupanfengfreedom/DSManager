using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;
using DSManager.LuaBase;
using System.Collections.Concurrent;
using System.Net;
using System.IO;
using ProtoBuf;

namespace DSManager
{
    class LoginServer : luabase, Entity
    {
        KChannel channel_matchserver;
        public ConcurrentDictionary<int, Player> Players = new ConcurrentDictionary<int, Player>();
        public LoginServer() : base("Loginserver")
        {
            LuaTable server = GetValueFromLua<LuaTable>("server");
            string nettype = (string)server["nettype"];
            LuaTable serveraddr = (LuaTable)server[nettype];
            int port = (int)(Int64)serveraddr["port"];
            KService service = Session.createorget(port);
            service.onAcceptAKchannel += (ref KChannel channel) => {
                int id = 0;
                do
                {
                    id = RandomHelper.RandomNumber(int.MinValue, int.MaxValue);
                } while (Players.ContainsKey(id));
                Players.TryAdd(id,new Player(id,channel, this));
                Logger.log("onaccept");
            };
////////////////////////////////////////////////////////////////////////////////////////
            LuaTable remoteserver = GetValueFromLua<LuaTable>("remoteserver");
            nettype = (string)remoteserver["nettype"];
            serveraddr = (LuaTable)remoteserver[nettype];
            string serverip = (string)serveraddr["serverip"];
            port = (int)(Int64)serveraddr["port"];
            IPAddress ipAd = IPAddress.Parse(serverip);//local ip address  "172.16.5.188"
            channel_matchserver = Session.getnew().GetChannel(new IPEndPoint(ipAd, port));
            channel_matchserver.onUserLevelReceivedCompleted += (ref byte[] buffer) => {
                switch ((CMDMatchServer)buffer[0])
                {
                    case CMDMatchServer.MATCHREQUEST:
                        int playerid = BitConverter.ToInt32(buffer, 1);
                        Player player;
                        bool b =  Players.TryGetValue(playerid,out player);
                        if (b)
                        {
                            int side = BitConverter.ToInt32(buffer, 5);
                            int dsport = BitConverter.ToInt32(buffer, 9);
                            string dswan = Encoding.getstring(buffer, 13, buffer.Length - 13);
                            byte[] tempbuffer = new byte[buffer.Length - 5];
                            Array.Copy(buffer, 5, tempbuffer, 0, tempbuffer.Length);
                            player.send((byte)CMDPlayer.MATCHREQUEST, tempbuffer);
                            Logger.log("--player.playerinfor-- : " + player.playerinfor + "--side-- : " + side + "--dsport--" + dsport + "--dswan-- " + dswan);
                        }
                        else
                        {
    
                        }

                        break;
                    case CMDMatchServer.CREATEROOM:
                        playerinfor pi = new playerinfor();
                        MemoryStream ms = new MemoryStream();
                        ms.Write(buffer, 1, buffer.Length - 1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);        
                        b = Players.TryGetValue(pi.playerid, out player);
                        if (b)
                        {
                            player.send((byte)CMDPlayer.CREATEROOM, BitConverter.GetBytes(pi.roomnumber));
                            Logger.log(pi.roomnumber+ " :pi.roomnumber :createroom");
                        }
                        else
                        {

                        }





                        break;
                    case CMDMatchServer.JOINROOM:

                        Logger.log(" :joinroom");

                        break;
                    case CMDMatchServer.PLAYEREXITQUEST:
      
                        Logger.log(" :playerexitquest");

                        break;
                    default:
                        break;
                }
            };
        }
        public void sendtomatchserver(byte command, byte[] buffer)
        {
            byte[] t = new byte[buffer.Length + 1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel_matchserver.Send(t);
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
