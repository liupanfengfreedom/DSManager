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
        public int halfroomnumber { get; private set; }
        private readonly object playersLock = new object();
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
                filter.halfroomnumber = Int32.Parse(condition);
                filter.addplayer(pi);
            }
            else
            {
                filter.sort(remaincondition,pi);
            }
        }
        void addplayer(playerinfor pi)
        {
            lock (playersLock)
            {
                players.TryAdd(pi.playerid, pi);
                if (players.Count == halfroomnumber)//when number of player in this filter is equal to Room.halfroomnumber then move these players to room *************
                {
                    Logger.log("players.Count == Room.halfroomnumber : "+ halfroomnumber);
                    String[] sperater = { "???" };
                    String[] conditions = pi.SimulateInforStr.Split(sperater, StringSplitOptions.RemoveEmptyEntries);

                    ICollection<Room> waitingrooms = RoomManager.getsingleton().getcurrentwaitingroom();
                    Room room = null;
                    foreach (var v in waitingrooms)
                    {
                        bool b = v.b_wholeteam;//waiting for another whole team 
                        Logger.log("v.b_wholeteam    :" + b);
                        String[] roomconditions = v.getconditions();
                        if (roomconditions.Length == 0)
                        {
                            continue;
                        }
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
                        room = RoomManager.getsingleton().createroom(halfroomnumber,pi.SimulateInforStr);
                        Logger.log("room == null so create a room");
                    }
                    foreach (var v in players.Values)
                    {
                        room.addplayer(v);
                    }
                    players.Clear();
                }
            }
            if (players.Count != 1)
            {
                return;
            }
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(System.TimeSpan.FromMilliseconds(Room.expiredtime));
                    lock (playersLock)
                    {
                        Logger.log("filter time expired");
                        if (players.Count == 0)
                        {
                            Logger.log("number of player in this filter is equal to Room.halfroomnumber then move these players to room *************");
                            return;
                        }
                        Func<int, int, Room> localgetroom = (int i1, int i2) => {
                            ICollection<Room> waitingrooms = RoomManager.getsingleton().getcurrentwaitingroom();
                            Room room1 = null;
                            foreach (var v in waitingrooms)
                            {
                                bool b = v.b_anyplayerisok;//this room has been waiting too long 
                                if (b)//search for a whole team with different national flag
                                {
                                    room1 = v;
                                }
                            }
                            if (room1 == null)
                            {
                                room1 = RoomManager.getsingleton().createroom(halfroomnumber, "");//this room does not reject any national flag 
                            }
                            return room1;
                        };

                        Room room = localgetroom(0, 0);
                        foreach (var v in players.Values)
                        {
                            if (!room.isfull)
                            {
                                room.addplayer(v);
                            }
                            else
                            {
                                room = localgetroom(0, 0);
                                room.addplayer(v);
                                Logger.log("room is full to create a new room and add player-----------------------------");
                            }
                        }
                        players.Clear();
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
        }
    }
}
