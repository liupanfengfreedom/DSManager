using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class Session
    {
        KService kService;
        static Session session;
        public Session()//client
        {
            kService = new KService();
        }
        public Session(int port)//server
        {
            kService = new KService(port);
        }
        public static Session get()//client
        {
            if (session == null)
            {
                session = new Session();
            }
            return session;
        }
        public static KService get(int port)//server
        {
            if (session == null)
            {
                session = new Session(port);
            }
            return session.kService;
        }
        public  KChannel GetChannel(IPEndPoint remotserveripEndPoint)
        {
            return kService.CreateAConnectChannel(remotserveripEndPoint);
        }

    }
}
