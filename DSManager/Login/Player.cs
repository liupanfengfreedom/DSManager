using ProtoBuf;
using System;
using System.IO;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    enum CMDPlayer
    {
        SINGUP,
        LOGIN,
        MATCHREQUEST,
        CREATEROOM,
        JOINROOM,
        JOINROOMFAILED,
        STARTGAME,
        OTHERPLAYERINFOR,
        EXITREQUEST,
        RECONNECTLOGIN,
        RECONNECT,
        RECONNECTV1,
        CHECKCONNECTION,
    }
    class Player
    {
        KChannel channel;
        LoginServer loginserver;
        int id;
        public string playerinfor { get;private set;}
        public Player(int id,KChannel channel, LoginServer loginserver)
        {
            this.id = id;
            this.channel = channel;
            this.loginserver = loginserver;
            this.channel.ondisconnect += () => {
                Player v;
                this.loginserver.Players.TryRemove(id, out v);
                loginserver.sendtomatchserver((byte)CMDMatchServer.PLAYEREXITQUEST, BitConverter.GetBytes(id));
                Logger.log(id + " :ondisconnect");
            };
            this.channel.onUserLevelReceivedCompleted += (ref byte[] buffer) =>
            {
#if RTT
                var str = System.Text.Encoding.UTF8.GetString(buffer);
                //Console.WriteLine(str);
                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] buffer1 = asen.GetBytes(str + "server");
                send(ref buffer1);
#else
                switch ((CMDPlayer)buffer[0])
                {
                    case CMDPlayer.SINGUP:
                        Logger.log("Sing up ");
                        //write data base
                        break;
                    case CMDPlayer.LOGIN:
                        Logger.log("log in ");
                        var str = Encoding.getstring(buffer, 1, buffer.Length - 1);
                        Logger.log(str);//username password ,these infor maybe used to query database
                        ////////////////////////////////////////////////////////////////////
                        //read data base
                        //simulate playerinfor
                        int simulateddata;
                        for (int i = 0; i < 2; i++)
                        {
                            simulateddata = RandomHelper.RandomNumber(1, 3);
                            playerinfor += simulateddata.ToString()+"???";
                        }
                        ////////////////////////////////////////////////////////////////////////////////////
                        //ack
                        byte[] infor = Encoding.getbyte(playerinfor);
                        byte[] t = new byte[playerinfor.Length + 4];
                        t.WriteTo(0, id);    
                        Array.Copy(infor, 0, t, 4, infor.Length);
                        send((byte)CMDPlayer.LOGIN, t);
                        break;
                    case CMDPlayer.MATCHREQUEST:
                        int halfroomnumber = BitConverter.ToInt32(buffer, 1);
                        Logger.log("match ");
                        Logger.log("halfroomnumber :"+ halfroomnumber);
                        Logger.log(playerinfor);
                        var playerinfor_ = new playerinfor {
                            playerid = id,
                            SimulateInforStr = playerinfor + halfroomnumber+"???",
                            SimulateInforInt = 1234
                        };
                        MemoryStream ms = new MemoryStream();
                        Serializer.Serialize(ms, playerinfor_);
                        loginserver.sendtomatchserver((byte)CMDMatchServer.MATCHREQUEST, ms.ToArray());
                        break;
                    case CMDPlayer.CREATEROOM:
                        Logger.log("createroom ");
                        Logger.log(playerinfor);
                        playerinfor_ = new playerinfor
                        {
                            playerid = id,
                            homeowner = true,
                            SimulateInforStr = playerinfor,
                            halfroomnumber = BitConverter.ToInt32(buffer, 1),//here the halfroomnumber seem to be useless
                        };
                        ms = new MemoryStream();
                        Serializer.Serialize(ms, playerinfor_);
                        loginserver.sendtomatchserver((byte)CMDMatchServer.CREATEROOM, ms.ToArray());
                        break;
                    case CMDPlayer.JOINROOM:
                        Logger.log("joinroom ");
                        Logger.log(playerinfor);
                        playerinfor_ = new playerinfor
                        {
                            playerid = id,
                            homeowner = false,
                            SimulateInforStr = playerinfor,
                            roomnumber = BitConverter.ToInt32(buffer, 1),
                        };
                        ms = new MemoryStream();
                        Serializer.Serialize(ms, playerinfor_);
                        loginserver.sendtomatchserver((byte)CMDMatchServer.JOINROOM, ms.ToArray());
                        break;
                    case CMDPlayer.STARTGAME:
                        Logger.log("startgame ");
                        Logger.log(playerinfor);
                        playerinfor_ = new playerinfor
                        {
                            playerid = id,
                            homeowner = true,
                            roomnumber = BitConverter.ToInt32(buffer, 1),
                        };
                        ms = new MemoryStream();
                        Serializer.Serialize(ms, playerinfor_);
                        loginserver.sendtomatchserver((byte)CMDMatchServer.STARTGAME, ms.ToArray());
                        break;
                    case CMDPlayer.EXITREQUEST:
                        Logger.log("CMDPlayer.EXITREQUEST");
                        loginserver.sendtomatchserver((byte)CMDMatchServer.PLAYEREXITQUEST, BitConverter.GetBytes(id));

                        break;
                    case CMDPlayer.RECONNECTLOGIN://
                        Logger.log("CMDPlayer.RECONNECTLOGIN");
                        str = Encoding.getstring(buffer, 1, buffer.Length - 1);
                        Logger.log(str);//username password ,these infor maybe used to query database
                        ////////////////////////////////////////////////////////////////////
                        //read data base
                        //simulate playerinfor
                        for (int i = 0; i < 2; i++)
                        {
                            simulateddata = RandomHelper.RandomNumber(1, 3);
                            playerinfor += simulateddata.ToString() + "???";
                        }
                        send((byte)CMDPlayer.RECONNECTLOGIN, BitConverter.GetBytes(id));
                        break;
                    /*
                     server side : here may just for ios device when front_back switch this player will be die so replace it 
                     client side : when front_back switch first check the connection to DS ,if it is connecting then do nothing ,if it is disconnect then reconnect to ds directly
                     */
                    case CMDPlayer.RECONNECT://
                        Logger.log("CMDPlayer.RECONNECT");
                        int id_ = BitConverter.ToInt32(buffer, 1);
                        int roomid = BitConverter.ToInt32(buffer, 5);
                        halfroomnumber = BitConverter.ToInt32(buffer, 9);
                        str = Encoding.getstring(buffer,13, buffer.Length - 13);
                        Logger.log(str);//username password ,these infor maybe used to query database
                        ////////////////////////////////////////////////////////////////////
                        //read data base
                        //simulate playerinfor
                        for (int i = 0; i < 2; i++)
                        {
                            simulateddata = RandomHelper.RandomNumber(1, 3);
                            playerinfor += simulateddata.ToString() + "???";
                        }
                        playerinfor_ = new playerinfor
                        {
                            playerid = id,
                            oldplayerid = id_,
                            roomid = roomid,
                            SimulateInforStr = playerinfor + halfroomnumber + "???",
                            SimulateInforInt = 1234
                        };
                        ms = new MemoryStream();
                        Serializer.Serialize(ms, playerinfor_);  
                        loginserver.sendtomatchserver((byte)CMDMatchServer.RECONNECT, ms.ToArray());
                        break;
                    case CMDPlayer.RECONNECTV1://
                        Logger.log("CMDPlayer.RECONNECTv1");
                        id_ = BitConverter.ToInt32(buffer, 1);
                        roomid = BitConverter.ToInt32(buffer, 5);
                        int owner = BitConverter.ToInt32(buffer, 9);
                        str = Encoding.getstring(buffer, 13, buffer.Length - 13);
                        Logger.log(str);//username password ,these infor maybe used to query database
                        ////////////////////////////////////////////////////////////////////
                        //read data base
                        //simulate playerinfor
                        for (int i = 0; i < 2; i++)
                        {
                            simulateddata = RandomHelper.RandomNumber(1, 3);
                            playerinfor += simulateddata.ToString() + "???";
                        }
                        playerinfor_ = new playerinfor
                        {
                            playerid = id,
                            oldplayerid = id_,
                            roomid = roomid,
                            homeowner = owner==1? true : false,
                            SimulateInforStr = playerinfor,
                            SimulateInforInt = 1234
                        };
                        ms = new MemoryStream();
                        Serializer.Serialize(ms, playerinfor_);
                        loginserver.sendtomatchserver((byte)CMDMatchServer.RECONNECTV1, ms.ToArray());
                        break;
                    case CMDPlayer.CHECKCONNECTION://
                        Logger.log("CMDPlayer.CHECKCONNECTION");
                        send((byte)CMDPlayer.CHECKCONNECTION, BitConverter.GetBytes(0));
                        break;
                    default:
                        break;
                }
#endif
            };
        }
        public void send(byte command, byte[] buffer)
        {
            byte[] t = new byte[buffer.Length + 1];
            t[0] = command;
            Array.Copy(buffer, 0, t, 1, buffer.Length);
            channel.Send(t);
        }
    }
}
