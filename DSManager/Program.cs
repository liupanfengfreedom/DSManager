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
        public virtual void Begin()
        {
            MessageManager.GetSingleton().AddMessagelistener(this, in KeyMap.k1, (ref string msg) => {
                Console.WriteLine("k1 ct" + msg);
                msg += "00000000";
            });
        }
        public virtual void Update(float delta)
        {
            
        }
        public virtual void End()
        {
            MessageManager.GetSingleton().RemoveMessagelistener(this, ref KeyMap.k1);
        }
    }
    class ct1 : Entity
    {
        public virtual void Begin()
        {
            MessageManager.GetSingleton().AddMessagelistener(this, in KeyMap.k1, (ref string msg) => {
                Console.WriteLine("k1 ct" + msg);
                msg += "111111";
            });
        }
        public virtual void Update(float delta)
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
            while (true)
            {
                try
                {
                    Thread.Sleep(10);
                    Global.Update(10);
                }
                catch (Exception e)
                {
                throw new Exception($"main update: {e.Message}");

                }
            }
        }
    }
}
