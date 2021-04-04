using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSManager.LuaBase;
using NLua;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
namespace DSManager
{
    class Launchserver : luabase
    {
        private static int preport = 0;
        int startingport = 8000;
        string wanip = "120.55.126.186";//WAN
        string dspath = "";
        public Launchserver() : base("config")
        {
            LuaTable dsinfor = GetValueFromLua<LuaTable>("dsinfor");
            startingport = (int)(Int64)dsinfor["startingport"];
            wanip = (string)dsinfor["wanip"];
            dspath = (string)dsinfor["dspath"];
        }
        public Process CreateADSInstance()
        {
            int port = GetOneAvailablePort();
            string Arguments = string.Format(" -log=ue.log -port={0}", port);
            try
            {
                Process myProcess = new Process();
                //using (myProcess = new Process())
                {
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.FileName = dspath; //apppath;
                    myProcess.StartInfo.Arguments = Arguments;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.Start();
                }
                window_file_log.Log("launch server at port: " + port);
                return myProcess;
            }
            catch (Exception e)
            {
                return null;
                window_file_log.Log(e.Message);
            }
        }
         bool PortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints;
            //ipEndPoints = ipProperties.GetActiveTcpListeners();
            ipEndPoints = ipProperties.GetActiveUdpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
         int GetOneAvailablePort()
        {
            int counter = 0;
            bool b = PortInUse(startingport);
            while (b)
            {
                b = PortInUse(startingport + counter);
                if (b)
                {
                    counter++;
                }
            }
            if (preport == (startingport + counter))
            {
                counter++;
            }
            preport = startingport + counter;
            return preport;
        }
    }
}
