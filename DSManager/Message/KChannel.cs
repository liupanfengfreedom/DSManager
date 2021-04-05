using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    public delegate void OnUserLevelReceivedCompleted(ref byte[] buffer);
    class KChannel
    {
        public OnUserLevelReceivedCompleted onUserLevelReceivedCompleted;
        Timerhandler th;
        public uint Id { get; set; }
        public uint requestConn { get; set; }
        private UdpClient socket;//local socket
        private bool isConnected;
        private IPEndPoint remoteEndPoint;
		private Kcp kcp;
		private KService kService;
        private readonly byte[] cacheBytes = new byte[ushort.MaxValue];
		private uint lastpingtime { get; set; }
        public KChannel(uint conn, uint requestConn, UdpClient socket, IPEndPoint remoteEndPoint, KService kService)//server do this
        {
            this.Id = conn;
            this.requestConn = requestConn;
            this.kService = kService;
            this.socket = socket;
            this.remoteEndPoint = remoteEndPoint;
            kcphandle handle = new kcphandle();
            handle.Out = buffer => {
                this.socket.Send(buffer.ToArray(),buffer.Length, this.remoteEndPoint);
            };
            kcp = new Kcp(this.Id, handle);
            kcp.SetMtu(1400);
            kcp.NoDelay(1, 10, 2, 1);  //fast
            this.isConnected = true;
        }
        public KChannel(uint requestConn, UdpClient socket, IPEndPoint remoteEndPoint)//client do this
        {
            this.requestConn = requestConn;
            this.socket = socket;
            this.remoteEndPoint = remoteEndPoint;
            th = new Timerhandler((string s) => {
                cacheBytes.WriteTo(0, KcpProtocalType.SYN);
			    cacheBytes.WriteTo(4, this.requestConn);
                //Log.Debug($"client connect: {this.Conn}");
                this.socket.Send(cacheBytes,8, this.remoteEndPoint);
            }, "", 200, true);
            Global.GetComponent<Timer>().Add(th);
        }
        public void HandleAccept()//server do this
        {
            cacheBytes.WriteTo(0, KcpProtocalType.ACK);
            cacheBytes.WriteTo(4, this.Id);
            cacheBytes.WriteTo(8, this.requestConn);
            this.socket.Send(cacheBytes, 12, remoteEndPoint);
        }
        public void HandleConnnect(uint id)//client do this
        {
            if (this.isConnected)
            {
                return;
            }
            this.Id = id;
            kcphandle handle = new kcphandle();
            handle.Out = buffer => {
                this.socket.Send(buffer.ToArray(), buffer.Length, this.remoteEndPoint);
            };
            kcp = new Kcp(this.Id, handle);
            kcp.SetMtu(1400);
            kcp.NoDelay(1, 10, 2, 1);  //fast
            this.isConnected = true;
            th.kill = true;
            th = new Timerhandler((string s) => {
                cacheBytes.WriteTo(0, KcpProtocalType.PING);
                //Log.Debug($"client connect: {this.Conn}");
                this.socket.Send(cacheBytes, 4, this.remoteEndPoint);
            }, "", 6000, true);
            Global.GetComponent<Timer>().Add(th);
        }
        public void HandleRecv(UdpReceiveResult urr)
        {
            if (remoteEndPoint != urr.RemoteEndPoint)
            {
                kService.EPChannels.Remove(remoteEndPoint);
                remoteEndPoint = urr.RemoteEndPoint;
            }
            this.kcp.Input(urr.Buffer);
            int len;
            while ((len = kcp.PeekSize()) > 0)
            {
                var buffer = new byte[len];
                if (kcp.Recv(buffer) >= 0)
                {
                    onUserLevelReceivedCompleted.Invoke(ref buffer);
                }
            }
        }
        public void HandlePing()
        {
            lastpingtime = (uint)TimeHelper.ClientNowSeconds();
        }
        public void Update(uint timeNow)
        {
            if (timeNow - lastpingtime > 600)
            {
                disconnect();
            }
            // 如果还没连接上，发送连接请求
            if (!this.isConnected)
            {
                return;
            }
            //this.kcp.Check(DateTime.UtcNow);
            this.kcp.Update(DateTime.UtcNow);
        }
        void disconnect()
        {
            kService.idChannels.Remove(Id);
            kService.EPChannels.Remove(remoteEndPoint);
        }
    }
    internal class kcphandle : IKcpCallback
    {
        public Action<Memory<byte>> Out;
        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            Out(buffer.Memory.Slice(0, avalidLength));
        }
    }
}
