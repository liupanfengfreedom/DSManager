using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class Room
    {
        Timerhandler th;
        Timerhandler th1;
        public bool b_wholeteam { get; private set; }//waitingfor wholeteam 
        public bool b_anyplayerisok { get; private set; }//this room has been waiting too long 
        public int id { get; private set; }
        public string conditions { get; private set; }
        public static int halfroomnumber = 1 ;
        public static uint expiredtime = 2000;
        public ConcurrentDictionary<int, playerinfor> players { get; private set; }
        public Room(int id, string conditions)
        {
            players = new ConcurrentDictionary<int, playerinfor>();
            this.id = id;
            this.conditions = conditions;
        }
        public void addplayer(playerinfor pi)
        {
            if (players.Count > halfroomnumber)
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
                th.kill = true;
                Logger.log("room is full so create ds request");
                RoomManager.getsingleton().waitingtofighting(id);
                MatchServer.getsingleton().sendtoloadbalanceserver((byte)CMDLoadBalanceServer.CREATEDS, BitConverter.GetBytes(id));
            }
            if (th == null)
            {
                th = new Timerhandler((string s) => {
                    Logger.log("room time expired 000");
                    b_anyplayerisok = true;
/////////////////////////////////////////////////////////////////////////
                    Global.GetComponent<Timer>().Add(new Timerhandler((string s1) => {
                        Logger.log("room time expired 111");
                        RoomManager.getsingleton().waitingtofighting(id);
                        MatchServer.getsingleton().sendtoloadbalanceserver((byte)CMDLoadBalanceServer.CREATEDS, BitConverter.GetBytes(id));
                    }, "", Room.expiredtime, false));

                }, "", Room.expiredtime, false);//
                Global.GetComponent<Timer>().Add(th);
            }
        }
        public string[] getconditions()
        {
           String[] sperater = { "???" };
           return conditions.Split(sperater, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
