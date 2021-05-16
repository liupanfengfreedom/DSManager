using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class RoomManager
    {
        static RoomManager roommanager=null;
        public ConcurrentDictionary<int, Room> waitingRooms { get; private set; }
        public ConcurrentDictionary<int, Room> fightingRooms { get; private set; }
        ///////////////////////////////////////////////////////////////////////
        public ConcurrentDictionary<int, Room> CreatingRooms { get; private set; }
        public ConcurrentDictionary<int, Room> CreatedRooms { get; private set; }
        public RoomManager()
        {
            waitingRooms = new ConcurrentDictionary<int, Room>();
            fightingRooms = new ConcurrentDictionary<int, Room>();

            CreatingRooms = new ConcurrentDictionary<int, Room>();
            CreatedRooms = new ConcurrentDictionary<int, Room>();
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        await Task.Delay(1000);
                        foreach (var v in fightingRooms)
                        {
                           int number = v.Value.NumberOfOnlinePlayers();
                            if (number == 0)
                            { 
                                MatchServer.getsingleton().sendtoloadbalanceserver((byte)CMDLoadBalanceServer.DESTROY, BitConverter.GetBytes(v.Key));
                                Room room;
                                fightingRooms.TryRemove(v.Key,out room);
                            }
                        }
                        //////////////////////////////////////////////////////////////////////////////
                        foreach (var v in CreatedRooms)
                        {
                            int number = v.Value.NumberOfOnlinePlayers();
                            if (number == 0)
                            {
                                MatchServer.getsingleton().sendtoloadbalanceserver((byte)CMDLoadBalanceServer.DESTROYV1, BitConverter.GetBytes(v.Key));
                                Room room;
                                CreatedRooms.TryRemove(v.Key, out room);
                            }
                        }
                        foreach (var v in CreatingRooms)
                        {
                            int number = v.Value.NumberOfOnlinePlayers();
                            if (number == 0)
                            {
                                Room room;
                                CreatingRooms.TryRemove(v.Key, out room);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
        }
        public static RoomManager getsingleton()
        {
            if (roommanager == null)
            {
                roommanager = new RoomManager();
            }
            return roommanager;
        }
        public Room createroom(int halfroomnumber, string rejectcondition)
        {
            int id;
            do {
                id = RandomHelper.RandomNumber(int.MinValue, int.MaxValue);
            } while (waitingRooms.ContainsKey(id) || fightingRooms.ContainsKey(id));
            Room room = new Room(id, halfroomnumber, rejectcondition);
            Logger.log("new room----------- : "+ id);
            waitingRooms.TryAdd(id, room);
            Logger.log("waitingRooms"+ waitingRooms.Count);
            return room;
        }
        public Room createroom(int halfroomnumber)
        {
            int id;
            do
            {
                id = RandomHelper.RandomNumber(0,999);//for create room 
            } while (CreatingRooms.ContainsKey(id));
            Room room = new Room(id, halfroomnumber);
            CreatingRooms.TryAdd(id, room);
            Logger.log("CreatingRooms " + CreatingRooms.Count);
            return room;
        }

        public ICollection<Room> getcurrentwaitingroom()
        {
           return waitingRooms.Values;
        }
        public void waitingtofighting(int id)
        {
            Room room;
            if (waitingRooms.TryGetValue(id, out room))
            {
                fightingRooms.TryAdd(id, room);
                waitingRooms.TryRemove(id,out room);
            }
            else
            { 
                Logger.log("this should not happen  error here: waitingtofighting(int id)");
            }
        }
        public int creatingtocreated(int id)
        {
            Room room;
            if (CreatingRooms.TryGetValue(id, out room))
            {
                CreatingRooms.TryRemove(id, out room);
                //int id;
                do
                {
                    id = RandomHelper.RandomNumber(int.MinValue, int.MaxValue);//for CreatedRooms 
                } while (CreatedRooms.ContainsKey(id));
                room.changeid(id);
                CreatedRooms.TryAdd(id, room);
                return id;
            }
            else
            {
                Logger.log("this should not happen  error here: waitingtofighting(int id)");
            }
            return 0;
        }
    }
}
