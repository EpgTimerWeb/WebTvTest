using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WebTvTest
{
    public class WebServer
    {
        private TcpListener Listener = null;
        private bool IsListen = false;
        private int Port = 8080;

        public event Action<HttpContext> OnRequest;
        public WebServer(int Port)
        {
            this.Port = Port;
        }
        
        public void Start()
        {
            if (Listener != null)
                return;

            var EndPoint = new IPEndPoint(IPAddress.Any, Port);
            Listener = new TcpListener(EndPoint);
            Console.WriteLine("[Web\t] Starting...");
            try
            {
                Listener.Start();
                Listener.BeginAcceptTcpClient(AcceptRequest, Listener);
                IsListen = true;
                Console.WriteLine("[Web\t] Started at {0}", Port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Web\t] ERROR: {0}", ex.Message);
                Thread.Sleep(5000);
                Environment.Exit(1);
            }
        }
        public void Stop()
        {
            Console.WriteLine("[Web\t] Stopping..");
            if (Listener == null) return;
            IsListen = false;
            Listener.Stop();
            Listener = null;
            Console.WriteLine("[Web\t] Stopped");
        }
        
        private void AcceptRequest(IAsyncResult Result)
        {
            try
            {
                var RequsetListener = (TcpListener)Result.AsyncState;
                if (!IsListen) return;
                RequsetListener.BeginAcceptTcpClient(AcceptRequest, RequsetListener);
                var Client = RequsetListener.EndAcceptTcpClient(Result);
                
                var IP = ((IPEndPoint)Client.Client.RemoteEndPoint).Address;
                try
                {
                    if (OnRequest != null)
                        OnRequest(new HttpContext(Client));
                }
                catch (TimeoutException to)
                {
                    Debug.Print("Timeout: {0}", to.Message);
                }
                Client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("エラー: {0}", ex.Message);
            }
        }
    }
}
