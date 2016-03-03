using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace WebTvTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("WebTvTest v0.1");
            if (File.Exists("Configure.xml"))
            {
                Manager.Instance.Configure = Configure.LoadFromXmlFile("Configure.xml");
            }
            else
            {
                Console.WriteLine("[Conf\t] Using default configure.");
            }
            var server = new WebServer(Manager.Instance.Configure.WebPort);
            server.OnRequest += WebAction.Action;
            server.Start();

            
            var reset = new ManualResetEvent(false);
            Console.CancelKeyPress += (a, b) =>
            {
                server.Stop();
                Console.WriteLine("[Conf\t] Saving configure...");
                Manager.Instance.Configure.SaveToXmlFile("Configure.xml");
            };
            reset.WaitOne();
        }
    }
}
