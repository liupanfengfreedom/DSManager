using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DSManager.LuaBase;
using NLua;
namespace DSManager
{
    class PlayerSimulator : luabase, Entity
    {
        Timerhandler th;
        KChannel channel;
        string ts = "";
        bool bcreat;
        int roomnumber;
        int id;
        int roomid;
        IPAddress ipAd;
        int port;
        public PlayerSimulator() : base("PlayerSimulator")
        {
            ts = GetValueFromLua<string>("testmessage");      
            LuaTable roominfor = GetValueFromLua<LuaTable>("roominfor");
            bcreat = (bool)roominfor["bcreat"];
            roomnumber = (int)(Int64)roominfor["roomnumber"];
            LuaTable remoteserver = GetValueFromLua<LuaTable>("remoteserver");
            string nettype = (string)remoteserver["nettype"];
            LuaTable serveraddr = (LuaTable)remoteserver[nettype];
            string serverip = (string)serveraddr["serverip"];
            port = (int)(Int64)serveraddr["port"];
            ipAd = IPAddress.Parse(serverip);//local ip address  "172.16.5.188"
            createchannel();
        }
        public void createchannel()
        {
            channel = Session.getnew().GetChannel(new IPEndPoint(ipAd, port));
            channel.onUserLevelReceivedCompleted += (ref byte[] buffer) => {
#if RTT
                var str = System.Text.Encoding.UTF8.GetString(buffer);
                str += TimeHelper.Now();
                string[] sd = new string[1];
                sd[0] = "server";
                string[] sa = str.Split(sd, StringSplitOptions.RemoveEmptyEntries);
                string l1 = sa[0].Substring(9);
                string l2 = sa[1].Substring(9);
                int r = Int32.Parse(l1);
                int r1 = Int32.Parse(l2);
                int rr = r1 - r;
                Logger.log(rr.ToString());
                //Console.WriteLine(str);
#else
                switch ((CMDPlayer)buffer[0])
                {
                    case CMDPlayer.SINGUP:
                        Logger.log("Sing up ok");
                        break;
                    case CMDPlayer.LOGIN:
                        id = BitConverter.ToInt32(buffer, 1);
                        string infor = Encoding.getstring(buffer, 5, buffer.Length - 5);
                        Logger.log("log in ok ,id : " + id + "infor : " + infor);
                        break;
                    case CMDPlayer.CREATEROOM:
                        roomid = roomnumber = BitConverter.ToInt32(buffer, 1);
                        Logger.log("creatroom : " + roomnumber);
                        break;
                    case CMDPlayer.JOINROOM:
                        roomid = roomnumber = BitConverter.ToInt32(buffer, 1);
                        Logger.log("joinroom : " + roomnumber);
                        break;
                    case CMDPlayer.JOINROOMFAILED:
                        roomnumber = BitConverter.ToInt32(buffer, 1);
                        Logger.log("joinroom failed: " + roomnumber);
                        break;
                    case CMDPlayer.STARTGAME:
                        int side = BitConverter.ToInt32(buffer, 1);
                        int dsport = BitConverter.ToInt32(buffer, 5);
                        roomid = BitConverter.ToInt32(buffer, 9);
                        string dswan = Encoding.getstring(buffer, 13, buffer.Length - 13);
                        Logger.log("STARTGAME player : --side-- : " + side + "--dsport--" + dsport + "--dswan-- " + dswan + "--roomid-- " + roomid);
                        break;
                    case CMDPlayer.MATCHREQUEST:
                        side = BitConverter.ToInt32(buffer, 1);
                        dsport = BitConverter.ToInt32(buffer, 5);
                        roomid = BitConverter.ToInt32(buffer, 9);
                        dswan = Encoding.getstring(buffer, 13, buffer.Length - 13);
                        Logger.log("MATCHREQUEST player : --side-- : " + side + "--dsport--" + dsport + "--dswan-- " + dswan + "--roomid-- " + roomid);
                        break;
                    case CMDPlayer.OTHERPLAYERINFOR:
                        int playerid = BitConverter.ToInt32(buffer, 1);
                        int playerside = BitConverter.ToInt32(buffer, 5);
                        int oldplayerid = BitConverter.ToInt32(buffer, 9);
                        string playerinfor = Encoding.getstring(buffer, 13, buffer.Length - 13);
                        Logger.log("OTHERPLAYERINFOR  otherplayerid : " + playerid + " otheroldplayerid : " + oldplayerid + " playerside : " + (int)playerside + " playerinfor : " + playerinfor);
                        break;
                    case CMDPlayer.RECONNECTLOGIN:
                        id = BitConverter.ToInt32(buffer, 1);
                        Logger.log("RECONNECTLOGIN : ");
                        break;
                    case CMDPlayer.RECONNECT:
                        id = BitConverter.ToInt32(buffer, 1);
                        int result = BitConverter.ToInt32(buffer, 5);
                        Logger.log("RECONNECT : " + result );//failed when disconnected happen  when the roomid created but has not received by the client,so the client should launch a rematch
                        break;
                    case CMDPlayer.RECONNECTV1:
                        id = BitConverter.ToInt32(buffer, 1);
                        result = BitConverter.ToInt32(buffer, 5);
                        Logger.log("RECONNECTV1 : " + result);
                        break;
                    default:
                        break;
                }
#endif
            };

        }
        public void Begin()
        {
            Task.Run(async () =>
            {
                try
                {
                    int halfroomnumber = 2;
                    byte[] t;
                    while (!channel.isConnected)
                    {
                        await Task.Delay(100);
                    }
                    ts += RandomHelper.RandomNumber(0, int.MaxValue);
                    send((byte)CMDPlayer.LOGIN, Encoding.getbyte(ts));
                    //await Task.Delay(1000);
                    //createchannel();
                    //while (!channel.isConnected)
                    //{
                    //    await Task.Delay(10);
                    //}
                    //send((byte)CMDPlayer.RECONNECTLOGIN, Encoding.getbyte(ts));
                    //await Task.Delay(1000);
                    //createchannel();
                    //while (!channel.isConnected)
                    //{
                    //    await Task.Delay(10);
                    //}
                    //send((byte)CMDPlayer.RECONNECTLOGIN, Encoding.getbyte(ts));
                    //await Task.Delay(1000);


                    //send((byte)CMDPlayer.MATCHREQUEST, BitConverter.GetBytes(halfroomnumber));
                    //await Task.Delay(1000);
                    //createchannel();
                    //while (!channel.isConnected)
                    //{
                    //    await Task.Delay(10);
                    //}
                    //byte[] strbytes = Encoding.getbyte(ts);
                    //t = new byte[12+strbytes.Length];
                    //t.WriteTo(0, id);
                    //t.WriteTo(4, roomid);
                    //t.WriteTo(8, halfroomnumber);
                    //Array.Copy(strbytes, 0, t, 12, strbytes.Length);
                    //send((byte)CMDPlayer.RECONNECT, t);
                    //await Task.Delay(6000);
                    //createchannel();
                    //while (!channel.isConnected)
                    //{
                    //    await Task.Delay(10);
                    //}
                    //t.WriteTo(0, id);
                    //t.WriteTo(4, roomid);
                    //t.WriteTo(8, halfroomnumber);
                    //send((byte)CMDPlayer.RECONNECT, t);
                    //await Task.Delay(1000);

                    //createchannel();
                    //while (!channel.isConnected)
                    //{
                    //    await Task.Delay(10);
                    //}
                    //t = new byte[12];
                    //t.WriteTo(0, id);
                    //t.WriteTo(4, roomid);
                    //t.WriteTo(8, halfroomnumber);

                    //send((byte)CMDPlayer.RECONNECT, t);
                    //await Task.Delay(2000);

                    //createchannel();
                    //while (!channel.isConnected)
                    //{
                    //    await Task.Delay(10);
                    //}
                    //t = new byte[12];
                    //t.WriteTo(0, id);
                    //t.WriteTo(4, roomid);
                    //t.WriteTo(8, halfroomnumber);

                    //send((byte)CMDPlayer.RECONNECT, t);


                    //await Task.Delay(2000);

                    //createchannel();
                    //while (!channel.isConnected)
                    //{
                    //    await Task.Delay(10);
                    //}
                    //t = new byte[12];
                    //t.WriteTo(0, id);
                    //t.WriteTo(4, roomid);
                    //t.WriteTo(8, halfroomnumber);

                    //send((byte)CMDPlayer.RECONNECT, t);
                    //await Task.Delay(2000);

                    //createchannel();
                    //while (!channel.isConnected)
                    //{
                    //    await Task.Delay(10);
                    //}
                    //t = new byte[12];
                    //t.WriteTo(0, id);
                    //t.WriteTo(4, roomid);
                    //t.WriteTo(8, halfroomnumber);

                    //send((byte)CMDPlayer.RECONNECT, t);


                    if (bcreat)
                    {
                        Logger.log("owner");
                        send((byte)CMDPlayer.CREATEROOM, BitConverter.GetBytes(halfroomnumber));//here the halfroomnumber seem to be useless                      
                        await Task.Delay(2000);
                        createchannel();
                        while (!channel.isConnected)
                        {
                            await Task.Delay(100);
                        }
                        t = new byte[12];
                        t.WriteTo(0, id);
                        t.WriteTo(4, roomid);
                        t.WriteTo(8, bcreat ? 1 : 0);
                        Logger.log("roomid" + roomid);
                        send((byte)CMDPlayer.RECONNECTV1, t);
                        await Task.Delay(3000);

                        send((byte)CMDPlayer.STARTGAME, BitConverter.GetBytes(roomnumber));
                        await Task.Delay(1000);
                        createchannel();
                        while (!channel.isConnected)
                        {
                            await Task.Delay(100);
                        }
                        t = new byte[12];
                        t.WriteTo(0, id);
                        t.WriteTo(4, roomid);
                        t.WriteTo(8, bcreat ? 1 : 0);
                        Logger.log("roomid" + roomid);
                        send((byte)CMDPlayer.RECONNECTV1, t);
                        await Task.Delay(1000);
                        createchannel();
                        while (!channel.isConnected)
                        {
                            await Task.Delay(100);
                        }
                        t = new byte[12];
                        t.WriteTo(0, id);
                        t.WriteTo(4, roomid);
                        t.WriteTo(8, bcreat ? 1 : 0);
                        Logger.log("roomid" + roomid);
                        send((byte)CMDPlayer.RECONNECTV1, t);
                    }
                    else
                    {
                        Logger.log("client");
                        roomnumber = RandomHelper.RandomNumber(0, roomnumber);
                        send((byte)CMDPlayer.JOINROOM, BitConverter.GetBytes(roomnumber));
                        await Task.Delay(5000);
                        createchannel();
                        while (!channel.isConnected)
                        {
                            await Task.Delay(100);
                        }
                        t = new byte[12];
                        t.WriteTo(0, id);
                        t.WriteTo(4, roomid);
                        t.WriteTo(8, bcreat ? 1 : 0);
                        Logger.log("roomid" + roomid);
                        send((byte)CMDPlayer.RECONNECTV1, t);
                        await Task.Delay(30000);
                        createchannel();
                        while (!channel.isConnected)
                        {
                            await Task.Delay(100);
                        }
                        t = new byte[12];
                        t.WriteTo(0, id);
                        t.WriteTo(4, roomid);
                        t.WriteTo(8, bcreat ? 1 : 0);
                        Logger.log("roomid" + roomid);
                        send((byte)CMDPlayer.RECONNECTV1, t);
                        await Task.Delay(5000);
                        createchannel();
                        while (!channel.isConnected)
                        {
                            await Task.Delay(100);
                        }
                        t = new byte[12];
                        t.WriteTo(0, id);
                        t.WriteTo(4, roomid);
                        t.WriteTo(8, bcreat ? 1 : 0);
                        Logger.log("roomid" + roomid);
                        send((byte)CMDPlayer.RECONNECTV1, t);
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
        }

        public void End()
        {
        }

        public void Update(uint delta)
        {
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
