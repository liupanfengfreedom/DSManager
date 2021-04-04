using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class MessageManager
    {
        public delegate void MessageHander(ref string str);
        static MessageManager mm =null;
        protected static object _lock = new object();
        ConcurrentDictionary<string, ConcurrentDictionary<Object,MessageHander>> eventmap=null;
        public MessageManager()
        {
            eventmap = new ConcurrentDictionary<string, ConcurrentDictionary<object, MessageHander>>();
        }
        ~MessageManager()
        {
        }
        public static MessageManager GetSingleton()
        {
            if (mm == null)
            {
                mm = new MessageManager();
            }
            return mm;
        }
        public void SendMessage(in string key, ref string msg)
        {
            bool b = eventmap.ContainsKey(key);
            if (b)
            {
                ConcurrentDictionary<Object, MessageHander> acts = eventmap[key];
                foreach (var v in acts.Values)
                {
                    v.Invoke(ref msg);
                }
            }
        }
        public void AddMessagelistener(Object instance,in string key,in MessageHander act)
        {
            bool b = eventmap.ContainsKey(key);
            if (b)
            {
            }
            else
            {
                eventmap.TryAdd(key, new ConcurrentDictionary<Object, MessageHander>());
            }
            b = eventmap[key].ContainsKey(instance);
            if (b)
            { }
            else
            {
                eventmap[key].TryAdd(instance, act);
            }
        }
        /*
         remove listeners for key from this instance
         */
        public void RemoveMessagelistener(Object instance, ref string key)
        {
            bool b = eventmap.ContainsKey(key);
            if (b)
            {
                b = eventmap[key].ContainsKey(instance);
                if (b)
                {
                    MessageHander act;
                    eventmap[key].TryRemove(instance, out act);
                }
            }
        }
        /*
           remove all listeners for any key from this instance
        */
        public void RemoveMessagelistener(Object instance)
        {
            foreach (var v in eventmap.Values)
            {
                if (v.ContainsKey(instance))
                {
                    MessageHander act;
                    v.TryRemove(instance,out act);
                }
            }
        }
    }
}
