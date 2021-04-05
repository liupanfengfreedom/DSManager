using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSManager
{
    class Global
    {
        static Dictionary<Type, Entity> Entitys = new Dictionary<Type, Entity>();
        static Global()
        { 
            Global.AddComponent<Timer>();
        }
        public static T AddComponent<T>() where T : Entity, new()
        {
            T t;
            Type type = typeof (T);
            bool b = Entitys.ContainsKey(type);
            if (b)
            {
                t = (T)Entitys[type];
                // throw new Exception($"AddComponent, component already exist, component: {typeof(T).Name}");
            }
            else
            {
                t = new T();
                t.Begin();
                Entitys.Add(type, t);
            }
            return t;
        }
        public static T GetComponent<T>() where T : Entity, new()
        {
            T t;
            Type type = typeof(T);
            bool b = Entitys.ContainsKey(type);
            if (b)
            {
                t = (T)Entitys[type];
            }
            else
            {
                t = new T();
                t.Begin();
                Entitys.Add(type, t);
            }
            return t;
        }
        public static void RemoveComponent<T>() where T : Entity, new()
        {
            Type type = typeof(T);
            bool b = Entitys.ContainsKey(type);
            if (b)
            {
                Entitys[type].End();
                Entitys.Remove(type);
            }
        }
        public static void Update(uint delta)
        {
            Thread.Sleep((int)delta);
            foreach (var v in Entitys.Values)
            {
                v.Update(delta);
            }
        }
    }
}
