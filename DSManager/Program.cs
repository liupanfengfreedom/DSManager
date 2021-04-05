using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSManager.LuaBase;
using NLua;

namespace DSManager
{
    class ct:Entity
    {
        int counter = 0;
        Timerhandler th;
        public virtual void Begin()
        {
            int x = 9;
            th = new Timerhandler((string s) => {
                Console.WriteLine("k1 ct" + s);
                Console.WriteLine("x " + x);
                if (counter++ == 5)
                {
                    th.kill = true;
                }
            }, "ct timer event", 1000, true);
            MessageManager.GetSingleton().AddMessagelistener(this, in KeyMap.k1, (ref string msg) => {
                Console.WriteLine("k1 ct" + msg);
                msg += "00000000";
            });
            Global.GetComponent<Timer>().Add(th);

        }
        public virtual void Update(uint delta)
        {
            
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
        }, "ct1 timer event", 2000, true);
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
    class Program
    {
        static void Main(string[] args)
        {
            Global.AddComponent<ct>();
            Global.AddComponent<ct1>();
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
            DSManager.GetSingleton().cleardss();
        }
    }
}
