using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class Global
    {
        static Dictionary<Type, Entity> Entitys = new Dictionary<Type, Entity>();
        public static void AddComponent<T>() where T : Entity, new()
        {
			Type type = typeof (T);
            bool b = Entitys.ContainsKey(type);
            if (b)
            {
                throw new Exception($"AddComponent, component already exist, component: {typeof(T).Name}");
            }
            else
            {
                T t = new T();
                t.Begin();
                Entitys.Add(type, t);
            }
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
        public static void Update(float delta)
        {
            foreach (var v in Entitys.Values)
            {
                v.Update(delta);
            }
        }
    }
}
