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
        public RoomManager()
        {
            waitingRooms = new ConcurrentDictionary<int, Room>();
            fightingRooms = new ConcurrentDictionary<int, Room>();
        }
        public static RoomManager getsingleton()
        {
            if (roommanager == null)
            {
                roommanager = new RoomManager();
            }
            return roommanager;
        }
        public Room createroom(string rejectcondition)
        {
            int id;
            do {
                id = RandomHelper.RandomNumber(0, int.MaxValue);
            } while (waitingRooms.ContainsKey(id) || fightingRooms.ContainsKey(id));
            Room room = new Room(id, rejectcondition);
            waitingRooms.TryAdd(id, room);
            Logger.log("waitingRooms"+ waitingRooms.Count);
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
    }
}
