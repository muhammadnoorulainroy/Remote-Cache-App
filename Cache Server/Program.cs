using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Helper_Classes;
using Topshelf;

namespace Cache_Server
{
    class Program
    {

        static void Main(string[] args)
        {
            var ipAddress = IPAddress.Parse(ConfigurationManager.AppSettings["IPAddress"]);
            var port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, port);
            try
            {
                var exitCode = HostFactory.Run(x =>
                {
                    x.Service<Server>(s =>
                    {
                        s.ConstructUsing(serverr => new Server(iPEndPoint));
                        s.WhenStarted(server => server.StartServer());
                        s.WhenStopped(server => server.StopServer());
                    });
                    x.RunAsLocalSystem();

                    x.SetServiceName("CacheServer");
                    x.SetDisplayName("Cache Server");

                    x.SetDescription("Cache Server for Remote Cache Application");
                });
                int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
                Environment.ExitCode = exitCodeValue;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }

        }

    }
}