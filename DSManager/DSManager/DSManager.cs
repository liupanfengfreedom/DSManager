using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
namespace DSManager
{

    class DSManager
    {
        Dictionary<int,dsinfor> dss;
        Dictionary<int,dsinfor> dssV1;
        Launchserver LS;
        static DSManager dsm=null;
        public DSManager() {
            dss = new Dictionary<int, dsinfor>();
            dssV1 = new Dictionary<int, dsinfor>();
            LS = new Launchserver();
        }
         ~DSManager() {
            cleardss();
        }
        public static DSManager GetSingleton()
        {
            if (dsm == null)
            {
                dsm = new DSManager();
            }
            return dsm;
        }
        public dsinfor LaunchADS(int id)
        {
            dsinfor ds = LS.CreateADSInstance();
            if (ds == null)
            {

            }
            else {
                dss.Add(id,ds);
                //Console.WriteLine("d.ProcessName: " + ds.ProcessName);
                //Console.WriteLine("d.Id: " + ds.Id);
                ds.process.Disposed += (object sender, EventArgs e) =>
                {
                    //Console.Write("Disposed");
                    Logger.log("Disposed" + ds.process.Id);
                };
                ds.process.Exited += (object sender, EventArgs e) =>
                {
                    //Console.Write("exit");
                    Logger.log("exit" + ds.process.Id);
                };
            }
            Logger.log("create" + ds.process.Id);
            return ds;
        }
        public dsinfor LaunchADSV1(int id)
        {
            dsinfor ds = LS.CreateADSInstance();
            if (ds == null)
            {

            }
            else
            {
                dssV1.Add(id, ds);
                //Console.WriteLine("d.ProcessName: " + ds.ProcessName);
                //Console.WriteLine("d.Id: " + ds.Id);
                ds.process.Disposed += (object sender, EventArgs e) =>
                {
                    //Console.Write("Disposed");
                    Logger.log("Disposed" + ds.process.Id);
                };
                ds.process.Exited += (object sender, EventArgs e) =>
                {
                    //Console.Write("exit");
                    Logger.log("exit" + ds.process.Id);
                };
            }
            Logger.log("create" + ds.process.Id);
            return ds;
        }
        public void killds(int id)
        {
            if (dss.ContainsKey(id))
            {
                if (dss[id].process != null && !dss[id].process.HasExited)
                { 
                    KillProcessAndChildren(dss[id].process.Id);
                }
                dss.Remove(id);
            }
        }
        public void killdsV1(int id)
        {
            if (dssV1.ContainsKey(id))
            {
                if (dssV1[id].process != null && !dssV1[id].process.HasExited)
                {
                    KillProcessAndChildren(dssV1[id].process.Id);
                }
                dssV1.Remove(id);
            }
        }
        public void cleardss()
        {
            foreach (var d in dss)
            {
                if (d.Value != null && !d.Value.process.HasExited)
                {
                    KillProcessAndChildren(d.Value.process.Id);
                }
            }
            dss.Clear();

            foreach (var d in dssV1)
            {
                if (d.Value != null && !d.Value.process.HasExited)
                {
                    KillProcessAndChildren(d.Value.process.Id);
                }
            }
            dssV1.Clear();
        }
        private  void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }
    }

}
