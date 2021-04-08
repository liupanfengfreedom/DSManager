using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    public delegate void MessageHander(string str);
    class Timerhandler
    {
        public bool loop = false;
        public bool kill = false;
        public int time = 0;
        public uint period = 0;
        public string para;
        public MessageHander timeHander;
        public Timerhandler(in MessageHander act,string para,uint time,bool loop=false)
        {
            timeHander = act;
            period = time;
            this.time = (int)time;
            this.loop = loop;
            this.para = para;
        }
    }
    class Timer : Entity
    {
        List<Timerhandler> timerhandles = new List<Timerhandler>();
        public void Begin()
        {
            
        }

        public void End()
        {
            
        }
        public void Add(in Timerhandler th)
        {
            timerhandles.Add(th);
        }
        public void Update(uint delta)
        {
            for (int i = 0; i < timerhandles.Count; i++)
            {
                if (timerhandles[i].kill)
                { 
                    timerhandles.Remove(timerhandles[i]);
                    continue;
                }
                timerhandles[i].time -= (int)delta;
                if (timerhandles[i].time <= 0)
                {
                    timerhandles[i].timeHander.Invoke(timerhandles[i].para);
                    if (timerhandles[i].loop)
                    {
                        timerhandles[i].time = (int)timerhandles[i].period;
                    }
                    else
                    { 
                        timerhandles.Remove(timerhandles[i]);
                    }
                }
            }
        }
    }
}
