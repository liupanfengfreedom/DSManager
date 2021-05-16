using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NLua;
using System.Runtime.InteropServices;
using System.Reflection;

namespace DSManager.LuaBase
{
 public class luabase
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();
        [DllImport("Kernel32.dll")]
        public static extern void AllocConsole();
       Lua state = new Lua();
       public luabase()
        {
        }
       public luabase(String luafile)
        {
            state["this"] = this;
            state.LoadCLRPackage();
            state.DoString(File.ReadAllText("./Lua/"+luafile+".lua"));
        }
        public void csharpprint(string str)
        {
            Console.WriteLine(this.ToString()+" : "+str);
        }
        public T GetValueFromLua<T>(string str)
        {
           return (T)state[str];
        }
    }
}
