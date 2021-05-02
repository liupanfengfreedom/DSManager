using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class Filter
    {
        Timerhandler th;
        ConcurrentDictionary<int, playerinfor> players = new ConcurrentDictionary<int, playerinfor>();
        ConcurrentDictionary<string, Filter> layers = new ConcurrentDictionary<string, Filter>();
        public void sort(string conditions,playerinfor pi)
        {
            int index = conditions.IndexOf("???", StringComparison.OrdinalIgnoreCase);
            string condition = conditions.Substring(0, index);
            string remaincondition = conditions.Substring(index + 3);
            Filter filter;
            if (layers.TryGetValue(condition, out filter))
            {
            }
            else
            {
                filter = new Filter();
                layers.TryAdd(condition, filter);
            }
///////////////////////////////////////////////////////////
            if (remaincondition == "")
            {
                if (th == null)
                {
                    th = new Timerhandler((string s) => {
                        Logger.log("filter time expired");
                        ICollection<Room> waitingrooms = RoomManager.getsingleton().getcurrentwaitingroom();
                        Room room = null;
                        foreach (var v in waitingrooms)
                        {
                            bool b =v.b_anyplayerisok;//this room has been waiting too long 
                            if (b)//search for a whole team with different national flag
                            {
                                room = v;
                            }
                        }
                        if (room == null)
                        {
                            room = RoomManager.getsingleton().createroom("");//this room does not reject any national flag 
                        }     
                        foreach (var v in players.Values)
                        {
                            room.addplayer(v);
                        }
                        players.Clear();
                    }, "", Room.expiredtime, false);//
                    Global.GetComponent<Timer>().Add(th);
                }
                addplayer(pi);
            }
            else
            {
                filter.sort(remaincondition,pi);
            }
        }
        void addplayer(playerinfor pi)
        {
            players.TryAdd(pi.playerid, pi);
            if (players.Count == Room.halfroomnumber)//when number of player in this filter is equal to Room.halfroomnumber then move these players to room
            {
                Logger.log("players.Count == Room.halfroomnumber");
                th.kill = true;
                String[] sperater = { "???" };
                String[] conditions = pi.SimulateInforStr.Split(sperater, StringSplitOptions.RemoveEmptyEntries);

                ICollection<Room> waitingrooms =  RoomManager.getsingleton().getcurrentwaitingroom();
                Room room=null;
                foreach (var v in waitingrooms)
                {
                    bool b = v.b_wholeteam;//waiting for another whole team 
                    Logger.log("v.b_wholeteam    :"+ b);
                    String[] roomconditions = v.getconditions();
                    /*
                     match the team which meet certain criteria to the same room
                     */
                    bool b1 = roomconditions[0] != conditions[0];// && roomconditions[1] != conditions[1];    //ensure these two team with different national flag
                    Logger.log("meet certain criteria    :" + b1);

                    if (b && b1)//search for a whole team with different national flag
                    {
                        room = v;
                        Logger.log("the certain criteria is meet");
                    }
                }
                if (room == null)
                {
                    room = RoomManager.getsingleton().createroom(pi.SimulateInforStr);
                    Logger.log("room == null so create a room");
                }
                foreach (var v in players.Values)
                { 
                    room.addplayer(v);
                }
                players.Clear();
            }
        }
    }
}
