using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    [ProtoContract]
    class playerinfor
    {
        [ProtoMember(1)]
        public  int playerid { get; set; }
        [ProtoMember(2)]
        public int halfroomnumber { get; set; }

////////////////////////////////////////////////////////
        [ProtoMember(3)]
        public int roomnumber { get; set; }
        [ProtoMember(4)]
        public bool homeowner { get; set; }
////////////////////////////////////////////////////////

        [ProtoMember(5)]
        public string SimulateInforStr { get; set; }
        [ProtoMember(6)]
        public int SimulateInforInt { get; set; }
        [ProtoMember(7)]
        public bool offline { get; set; }
        [ProtoMember(8)]
        public int loginserverproxyid { get; set; }
        /// //////////////////////////////////////////////////////////////
        /////////return infor
        [ProtoMember(9)]
        public byte side { get; set; }
        [ProtoMember(10)]
        public int oldplayerid { get; set; }
        [ProtoMember(11)]
        public int roomid { get; set; }
    }
    class LoginServerProxy
    {
        int id;
        KChannel channel_loginserver;
        MatchServer matchserver;
        public LoginServerProxy(int id,KChannel channel, MatchServer matchserver)
        {
            this.id = id;
            this.channel_loginserver = channel;
            this.matchserver = matchserver;
            this.channel_loginserver.ondisconnect += () => {
                LoginServerProxy lsp;
                bool b = this.matchserver.LoginServers.TryRemove(id,out lsp);
                if (b)
                {
                   Logger.log("channel_loginserver.ondisconnect successfully");
                }
                else
                { 
                   Logger.log("this should not happen for channel_loginserver.ondisconnect");
                }
            };
            channel_loginserver.onUserLevelReceivedCompleted += (ref byte[] buffer) => {
                //var str = Encoding.getstring(buffer, 1, buffer.Length - 1);
                switch ((CMDMatchServer)buffer[0])
                {
                    case CMDMatchServer.MATCHREQUEST:
                        playerinfor pi = new playerinfor();
                        MemoryStream ms = new MemoryStream();
                        ms.Write(buffer, 1, buffer.Length - 1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);
                        pi.loginserverproxyid = this.id;
                        Logger.log(pi.SimulateInforStr);
                        matchserver.addtomatchpool(pi);
                        Logger.log(pi.playerid + " :matchrequest");

                        break;
                    case CMDMatchServer.CREATEROOM:
                        pi = new playerinfor();
                        ms = new MemoryStream();
                        ms.Write(buffer, 1, buffer.Length - 1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);
                        pi.loginserverproxyid = this.id;
                        Logger.log(pi.SimulateInforStr);
                        matchserver.addtomatchpoolV1(pi);
                        Room room = RoomManager.getsingleton().createroom(pi.halfroomnumber);
                        room.addplayerv1(pi);
                        /////////////////////////////////////////
                        pi.roomnumber = room.id;
                        ms = new MemoryStream();
                        Serializer.Serialize(ms, pi);
                        sendtologinserver((byte)CMDMatchServer.CREATEROOM, ms.ToArray());
                        Logger.log(pi.roomnumber + " :pi.roomnumber CREATEROOM ,pi.halfroomnumber : " + pi.halfroomnumber);
                        break;
                    case CMDMatchServer.JOINROOM:
                        pi = new playerinfor();
                        ms = new MemoryStream();
                        ms.Write(buffer, 1, buffer.Length - 1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);
                        pi.loginserverproxyid = this.id;
                        Logger.log(pi.SimulateInforStr);
                        matchserver.addtomatchpoolV1(pi);
                        if (RoomManager.getsingleton().CreatingRooms.TryGetValue(pi.roomnumber, out room))
                        {
                            room.addplayerv1(pi);
                            /////////////////////////////////////////
                            pi.roomnumber = room.id;
                            ms = new MemoryStream();
                            Serializer.Serialize(ms, pi);
                            sendtologinserver((byte)CMDMatchServer.JOINROOM, ms.ToArray());
                            Logger.log(pi.roomnumber + " :pi.roomnumber JOINROOM");
                        }
                        else
                        {
                            ms = new MemoryStream();
                            Serializer.Serialize(ms, pi);
                            sendtologinserver((byte)CMDMatchServer.JOINROOMFAILED, ms.ToArray());
                            Logger.log("join room but the room with specific number is not found ,this roomnumber is : " + pi.roomnumber);
                        }


                        break;
                    case CMDMatchServer.STARTGAME:
                        pi = new playerinfor();
                        ms = new MemoryStream();
                        ms.Write(buffer, 1, buffer.Length - 1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);
                        pi.loginserverproxyid = this.id;
                        Logger.log(id + " :startgame");
                        if (RoomManager.getsingleton().CreatingRooms.TryGetValue(pi.roomnumber, out room))
                        {
                            Logger.log(pi.roomnumber + " :pi.roomnumber STARTGAME");
                            room.startgame();
                        }
                        else
                        {
                            Logger.log("join room but the room with specific number is not found");
                        }


                        break;
                    case CMDMatchServer.PLAYEREXITQUEST:
                        int playerid = BitConverter.ToInt32(buffer, 1);
                        matchserver.removefrommatchpool(playerid);
                        Logger.log("loginserverproxyid: " + id + " :playerexitquest playerid " + playerid);

                        break;
                    case CMDMatchServer.RECONNECT:

                        pi = new playerinfor();
                        ms = new MemoryStream();
                        ms.Write(buffer, 1, buffer.Length - 1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);
                        pi.loginserverproxyid = this.id;
                        Logger.log("loginserverproxyid: " + id + " playerid RECONNECT:  " + pi.playerid);
                        byte[] feedbackbuffer = new byte[8];
                        feedbackbuffer.WriteTo(0, pi.playerid);
                        Filter filter;
                        if (Filter.Filtertracker.TryGetValue(pi.oldplayerid, out filter))
                        {
                            playerinfor p1;
                            filter.players.TryRemove(pi.oldplayerid, out p1);
                            if (p1 == null)
                            { }
                            else
                            {
                                matchserver.removefrommatchpool(pi.oldplayerid);
                                matchserver.onlyaddtomatchpool(pi);
                                filter.players.TryAdd(pi.playerid, pi);
                                feedbackbuffer.WriteTo(4, 1);
                                sendtologinserver((byte)CMDMatchServer.RECONNECT, feedbackbuffer);
                                Logger.log("=========Filter.Filtertracker:   playerid :" + pi.playerid + "oldplayerid : " + pi.oldplayerid);
                            }

                            break;
                        }
                        if (RoomManager.getsingleton().waitingRooms.TryGetValue(pi.roomid, out room))
                        {
                            playerinfor p1 = room.removeplayer(pi.oldplayerid);//waitingrooms and fightingrooms may contain the same roomid but playid is Globally unique
                            if (p1 == null)
                            { }
                            else
                            {
                                matchserver.removefrommatchpool(pi.oldplayerid);
                                matchserver.onlyaddtomatchpool(pi);
                                room.rejoin(pi);
                                feedbackbuffer.WriteTo(4, 1);
                                sendtologinserver((byte)CMDMatchServer.RECONNECT, feedbackbuffer);
                                Logger.log("=========waitingRooms:   playerid :" + pi.playerid+"oldplayerid : " + pi.oldplayerid);
                            }
                            break;
                        }
                        if (RoomManager.getsingleton().fightingRooms.TryGetValue(pi.roomid, out room))
                        {
                            playerinfor p1 = room.getplayer(pi.oldplayerid);
                            if (p1 == null)
                            { }
                            else
                            {
                                matchserver.removefrommatchpool(pi.oldplayerid);
                                matchserver.onlyaddtomatchpool(pi);
                                room.rejoin(pi);
                                feedbackbuffer.WriteTo(4, 1);
                                sendtologinserver((byte)CMDMatchServer.RECONNECT, feedbackbuffer);
                                Logger.log("=========fightingRooms:   playerid :" + pi.playerid + "oldplayerid : " + pi.oldplayerid);
                            }

                            break;
                        }

                        feedbackbuffer.WriteTo(4, 0);
                        sendtologinserver((byte)CMDMatchServer.RECONNECT, feedbackbuffer);
                        Logger.log("=========failed:   playerid :" + pi.playerid+"oldplayerid : " + pi.oldplayerid);
                        break;
                    case CMDMatchServer.RECONNECTV1:

                        pi = new playerinfor();
                        ms = new MemoryStream();
                        ms.Write(buffer, 1, buffer.Length - 1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);
                        pi.loginserverproxyid = this.id;
                        Logger.log("loginserverproxyid: " + id + " playerid RECONNECT:  " + pi.playerid);
                        feedbackbuffer = new byte[8];
                        feedbackbuffer.WriteTo(0, pi.playerid);
                        if (RoomManager.getsingleton().CreatingRooms.TryGetValue(pi.roomid, out room))
                        {
                            playerinfor p1 = room.getplayer(pi.oldplayerid);
                            if (p1 == null)
                            { }
                            else
                            {
                                matchserver.removefrommatchpool(pi.oldplayerid);
                            }
                            matchserver.onlyaddtomatchpool(pi);
                            room.rejoin(pi);
                            feedbackbuffer.WriteTo(4, 1);
                            sendtologinserver((byte)CMDMatchServer.RECONNECTV1, feedbackbuffer);
                            break;
                        }
   
                        if (RoomManager.getsingleton().CreatedRooms.TryGetValue(pi.roomid, out room))
                        {
                            playerinfor p1 = room.getplayer(pi.oldplayerid);
                            if (p1 == null)
                            { }
                            else
                            {
                                matchserver.removefrommatchpool(pi.oldplayerid);
                            }
                            matchserver.onlyaddtomatchpool(pi);
                            room.rejoin(pi);
                            feedbackbuffer.WriteTo(4, 1);
                            sendtologinserver((byte)CMDMatchServer.RECONNECTV1, feedbackbuffer);
                            break;
                        }

                        feedbackbuffer.WriteTo(4, 0);
                        sendtologinserver((byte)CMDMatchServer.RECONNECT, feedbackbuffer);
                        break;
                    default:
                        break;
                }

            };
        }
        public void sendtologinserver(byte command, byte[] buffer)
        {
            byte[] t = new byte[buffer.Length + 1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel_loginserver.Send(t);
        }
    }
}
