using DSManager.LuaBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLua;

namespace DSManager
{
    class BenchMark : luabase, Entity
    {
        public BenchMark() : base("BenchMark")
        {

        }
        void Entity.Begin()
        {
            GetValueFromLua<LuaFunction>("Begin").Call();
        }

        void Entity.End()
        {
            GetValueFromLua<LuaFunction>("End").Call();
        }

        void Entity.Update(uint delta)
        {
            GetValueFromLua<LuaFunction>("Update").Call(delta);

        }

    }
}
