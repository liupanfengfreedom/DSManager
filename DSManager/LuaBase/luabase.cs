using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NLua;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

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
        public void startprocess(string path,string argument)
        {
            Process myProcess = new Process();
            {
                myProcess.StartInfo.UseShellExecute = false;
                myProcess.StartInfo.FileName = path; //apppath;
                myProcess.StartInfo.Arguments = argument;
                myProcess.StartInfo.CreateNoWindow = false;
                myProcess.Start();
            }
        }
        public void startthread(Func<string, bool> c, int delaytime, string parastr)
        {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        bool b = c.Invoke(parastr);
                        if (b)
                        {
                            break;
                        }
                        await Task.Delay(delaytime);
                    }
                }
                catch (Exception e)
                {
                    Logger.log(e.ToString());
                }
            });
        }
        public T GetValueFromLua<T>(string str)
        {
           return (T)state[str];
        }
    }
}
