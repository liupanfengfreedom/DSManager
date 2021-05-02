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
        public string SimulateInforStr { get; set; }
        [ProtoMember(3)]
        public int SimulateInforInt { get; set; }
        [ProtoMember(4)]
        public bool offline { get; set; }
        /// //////////////////////////////////////////////////////////////
        /////////return infor
        [ProtoMember(5)]
        public byte side { get; set; }
    }
    class LoginServerProxy
    {
        KChannel channel_loginserver;
        MatchServer matchserver;
        public LoginServerProxy(KChannel channel, MatchServer matchserver)
        {
            this.channel_loginserver = channel;
            this.matchserver = matchserver;
            this.channel_loginserver.ondisconnect += () => {
                this.matchserver.LoginServers.Remove(this);
                Logger.log("ondisconnect");
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
                        Logger.log(pi.SimulateInforStr);
                        matchserver.addtomatchpool(pi);
                        Logger.log(pi.playerid+" :matchrequest");

                        break;
                    case CMDMatchServer.PLAYEREXITQUEST:
                        int id = BitConverter.ToInt32(buffer, 1);
                        matchserver.removefrompool(id);
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
