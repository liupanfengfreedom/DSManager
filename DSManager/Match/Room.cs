using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class Room
    {
        private readonly object RoomsLock = new object();

        bool b_created = false;
        public bool b_wholeteam { get; private set; }//waitingfor wholeteam 
        public bool b_anyplayerisok { get; private set; }//this room has been waiting too long 
        public int id { get; private set; }
        public bool isfull { get; private set; }
        public string conditions { get; private set; }
        public int halfroomnumber { get; private set; }
        public static uint expiredtime = 2000;
        public ConcurrentDictionary<int, playerinfor> players { get; private set; }
        public Room(int id,int halfroomnumber, string conditions)
        {
            isfull = false;
            players = new ConcurrentDictionary<int, playerinfor>();
            this.id = id;
            this.halfroomnumber = halfroomnumber;
            this.conditions = conditions;
        }
        public Room(int id, int halfroomnumber)
        {
            isfull = false;
            players = new ConcurrentDictionary<int, playerinfor>();
            this.id = id;
            this.halfroomnumber = halfroomnumber;
        }
        public void addplayerv1(playerinfor pi)
        {
            foreach (var v in players)
            {
                LoginServerProxy lsp;
                MatchServer.getsingleton().LoginServers.TryGetValue(pi.loginserverproxyid, out lsp);
                MemoryStream ms = new MemoryStream();
                Serializer.Serialize(ms, v.Value);
                byte[] btms = ms.ToArray();
                byte[] t = new byte[btms.Length + 4];
                byte[] ownerid = BitConverter.GetBytes(pi.playerid);
                Array.Copy(ownerid, 0, t, 0, 4);
                Array.Copy(btms, 0, t, 4, btms.Length);
                lsp.sendtologinserver((Byte)CMDMatchServer.OTHERPLAYERINFOR, t);
            }
            foreach (var v in players)
            {
                LoginServerProxy lsp;
                MatchServer.getsingleton().LoginServers.TryGetValue(v.Value.loginserverproxyid, out lsp);
                MemoryStream ms = new MemoryStream();
                Serializer.Serialize(ms, pi);
                byte[] btms = ms.ToArray();
                byte[] t = new byte[btms.Length + 4];
                byte[] ownerid = BitConverter.GetBytes(v.Value.playerid);
                Array.Copy(ownerid, 0, t, 0, 4);
                Array.Copy(btms, 0, t, 4, btms.Length);
                lsp.sendtologinserver((Byte)CMDMatchServer.OTHERPLAYERINFOR, t);
            }
            players.TryAdd(pi.playerid, pi);
            Logger.log("roomid : "+ id+" number of players :" + players.Count);
            //if (players.Count == halfroomnumber)
            //{ 
            //}         
        }
        public void startgame()
        { 
            int newid = RoomManager.getsingleton().creatingtocreated(id);
            MatchServer.getsingleton().sendtoloadbalanceserver((byte)CMDLoadBalanceServer.CREATEDSV1, BitConverter.GetBytes(newid));
            Logger.log("startgame() create room =================================================== newid : " + newid + " id :"+ id);

        }
        public void changeid(int id)
        {
            this.id = id;
        }
        public void addplayer(playerinfor pi)
        {
            foreach (var v in players)
            {
                LoginServerProxy lsp;
                MatchServer.getsingleton().LoginServers.TryGetValue(pi.loginserverproxyid, out lsp);
                MemoryStream ms = new MemoryStream();
                Serializer.Serialize(ms, v.Value);
                byte[] btms = ms.ToArray();
                byte[] t = new byte[btms.Length + 4];
                byte[] ownerid = BitConverter.GetBytes(pi.playerid);
                Array.Copy(ownerid, 0, t, 0, 4);
                Array.Copy(btms, 0, t, 4, btms.Length);
                lsp.sendtologinserver((Byte)CMDMatchServer.OTHERPLAYERINFOR, t);
            }
            foreach (var v in players)
            {
                LoginServerProxy lsp;
                MatchServer.getsingleton().LoginServers.TryGetValue(v.Value.loginserverproxyid, out lsp);
                MemoryStream ms = new MemoryStream();
                Serializer.Serialize(ms, pi);
                byte[] btms = ms.ToArray();
                byte[] t = new byte[btms.Length + 4];
                byte[] ownerid = BitConverter.GetBytes(v.Value.playerid);
                Array.Copy(ownerid, 0, t, 0, 4);
                Array.Copy(btms, 0, t, 4, btms.Length);
                lsp.sendtologinserver((Byte)CMDMatchServer.OTHERPLAYERINFOR, t);
            }
////////////////////
            if (players.Count >= halfroomnumber)
            {
                pi.side = 1;
            }
            else
            { 
                pi.side = 0;
            }
            players.TryAdd(pi.playerid, pi);
            if (players.Count == halfroomnumber)
            {
                b_wholeteam = true;
            }
            else
            { 
                b_wholeteam = false;
            }
            if (players.Count == halfroomnumber * 2)
            {
                isfull = true;
                lock (RoomsLock)
                {
                    if (!b_created)
                    {
                        b_created = true;
                        Logger.log("room is full so create ds request=================================================== : " + id);
                        RoomManager.getsingleton().waitingtofighting(id);
                        MatchServer.getsingleton().sendtoloadbalanceserver((byte)CMDLoadBalanceServer.CREATEDS, BitConverter.GetBytes(id));
                    }
                }
            }
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(System.TimeSpan.FromMilliseconds(Room.expiredtime));
                    Logger.log("room time expired 000");
                    b_anyplayerisok = true;
                    /////////////////////////////////////////////////////////////////////////
                    ///
                    await Task.Delay(System.TimeSpan.FromMilliseconds(Room.expiredtime));
                    lock (RoomsLock)
                    {
                        if (!b_created)
                        {
                            b_created = true;
                            Logger.log("room time expired 111===============================================================: " + id);
                            RoomManager.getsingleton().waitingtofighting(id);
                            MatchServer.getsingleton().sendtoloadbalanceserver((byte)CMDLoadBalanceServer.CREATEDS, BitConverter.GetBytes(id));
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
        }
        public int NumberOfOnlinePlayers()
        {
            foreach (var v in players)
            {
                if (v.Value.offline)
                {
                    playerinfor pif;
                    players.TryRemove(v.Key,out pif);
                }
            }
            return players.Count;
        }
        public string[] getconditions()
        {
           String[] sperater = { "???" };
           return conditions.Split(sperater, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
