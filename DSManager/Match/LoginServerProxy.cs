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
                        ms.Write(buffer,1,buffer.Length-1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);
                        pi.loginserverproxyid = this.id;
                        Logger.log(pi.SimulateInforStr);
                        matchserver.addtomatchpool(pi);
                        Logger.log(pi.playerid+" :matchrequest");

                        break;
                    case CMDMatchServer.CREATEROOM:
                        pi = new playerinfor();
                        ms = new MemoryStream();
                        ms.Write(buffer, 1, buffer.Length - 1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);
                        pi.loginserverproxyid = this.id;
                        Logger.log(pi.SimulateInforStr);
                        Room room = RoomManager.getsingleton().createroom(pi.halfroomnumber);
                        room.addplayerv1(pi);
                        /////////////////////////////////////////
                        pi.roomnumber = room.id;
                        ms = new MemoryStream();
                        Serializer.Serialize(ms, pi);
                        sendtologinserver((byte)CMDMatchServer.CREATEROOM, ms.ToArray());
                        Logger.log(pi.roomnumber + " :pi.roomnumber CREATEROOM");
                        break;
                    case CMDMatchServer.JOINROOM:
                        pi = new playerinfor();
                        ms = new MemoryStream();
                        ms.Write(buffer, 1, buffer.Length - 1);
                        ms.Position = 0;
                        pi = Serializer.Deserialize<playerinfor>(ms);
                        pi.loginserverproxyid = this.id;
                        Logger.log(pi.SimulateInforStr);
                        if (RoomManager.getsingleton().CreatingRooms.TryGetValue(pi.roomnumber, out room))
                        {
                            room.addplayerv1(pi);
                        }
                        else
                        { 
                            Logger.log("join room but the room with specific number is not found");
                        }
                        Logger.log(pi.roomnumber + " :pi.roomnumber JOINROOM");


                        break;
                    case CMDMatchServer.PLAYEREXITQUEST:
                        int playerid = BitConverter.ToInt32(buffer, 1);
                        matchserver.removefrompool(playerid);
                        Logger.log(id+" :playerexitquest");

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
