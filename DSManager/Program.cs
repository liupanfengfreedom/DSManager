using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSManager.LuaBase;
using NLua;
using ProtoBuf;

namespace DSManager
{
    class ctest : Entity
    {
        ConcurrentDictionary<int, string> d1 = new ConcurrentDictionary<int, string>();
        public virtual void Begin()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        int r = RandomHelper.RandomNumber(0, 100);
                        if (d1.ContainsKey(r))
                        {
                            continue;
                        }
                        d1.TryAdd(r,r.ToString());
                        Thread.Sleep(1);
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        int r = RandomHelper.RandomNumber(0, 100);
                        string v;
                        bool b = d1.TryRemove(r,out v);
                        if (b)
                        { 
                            Logger.log("remove ok------------------");
                        }
                        Thread.Sleep(1);
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
;                       foreach (var v in d1)
                        {
                            Logger.log(v.Value);
                            Thread.Sleep(10);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
        }
        public virtual void Update(uint delta)
        {

        }
        public virtual void End()
        {

        }
    }
    class ct:Entity
    {
        int counter = 0;
        Timerhandler th;
        public virtual void Begin()
        {
            Global.GetComponent<Timer>().Add(new Timerhandler((string s) => {
                Console.WriteLine("" + s);
            }, "-----------------", 1000, false));

            //MessageManager.GetSingleton().AddMessagelistener(this, in KeyMap.k1, (ref string msg) => {
            //    Console.WriteLine("k1 ct" + msg);
            //    msg += "00000000";
            //});
        }
        public virtual void Update(uint delta)
        {
            int x = 9;
            if (th == null)
            {
                th = new Timerhandler((string s) => {
                    Console.WriteLine("k1 ct" + s);
                    Console.WriteLine("x " + x);
                    if (counter++ == 5)
                    {
                        th.kill = true;
                    }
                }, "ct timer event", 1000, true);
                Global.GetComponent<Timer>().Add(th);
            }
        }
        public virtual void End()
        {
            MessageManager.GetSingleton().RemoveMessagelistener(this, ref KeyMap.k1);
        }
    }
    class ct1 : Entity
    {
        Timerhandler th = new Timerhandler((string s) => {
            Console.WriteLine("k1 ct" + TimeHelper.Now());
        }, "ct1 timer event", 1000, true);
        public virtual void Begin()
        {
            MessageManager.GetSingleton().AddMessagelistener(this, in KeyMap.k1, (ref string msg) => {
                Console.WriteLine("k1 ct" + msg);
                msg += "111111";
            });
            Global.GetComponent<Timer>().Add(th);

        }
        public virtual void Update(uint delta)
        {

        }
        public virtual void End()
        {
            MessageManager.GetSingleton().RemoveMessagelistener(this);
        }
    }
    [ProtoContract]
    class Person
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public Address Address { get; set; }
    }
    [ProtoContract]
    class Address
    {
        [ProtoMember(1)]
        public string Line1 { get; set; }
        [ProtoMember(2)]
        public string Line2 { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            String[] sperater = { "???" };
            string str = "allss1???a2???a3???a4???";
            string[] arraystr = str.Split(sperater, StringSplitOptions.RemoveEmptyEntries);
            //int index = str.IndexOf("???", StringComparison.OrdinalIgnoreCase);
            //string str1 = str.Substring(0, index);
            //string strremain = str.Substring(index + 3);
            //List<int> DSMchannels = new List<int>();
            //DSMchannels.Add(9);
            //DSMchannels.Add(1);
            //DSMchannels.Add(5);
            //DSMchannels.Sort((int d1, int d2) => { return d1 - d2; });

            Global.AddComponent<ServertoDS>();
            Global.AddComponent<DSClient>();
            Global.AddComponent<MatchServer>();
            Global.AddComponent<LoginServer>();
            //Global.AddComponent<PlayerSimulator>();
            //Global.AddComponent<ct1>();
            //Global.AddComponent<ct>();
            //Global.AddComponent<ctest>();
            while (true)
            {
                try
                {                   
                    Global.Update(10);
                }
                catch (Exception e)
                {
                throw new Exception($"main update: {e.Message}");

                }
            }
            int i = 0;
            while (true)
            {
                Thread.Sleep(2000);
                string msg = "sss";
                MessageManager.GetSingleton().SendMessage(in KeyMap.k1, ref msg);
                Console.WriteLine("after send  " + msg);

                // MessageManager.GetSingleton().SendMessage(in KeyMap.k2, ref msg);
                // DSManager.GetSingleton().LaunchADS();
                if (i++ == 5)
                {
                    // break;
                    Global.RemoveComponent<ct1>();
                }
            }
            //DSManager.GetSingleton().cleardss();
        }
    }
}
