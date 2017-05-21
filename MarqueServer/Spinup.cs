using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MarqueServer
{
    class Spinup : IDisposable
    {
        private GameObject oGameObject;

        public Spinup(int iPort, GameObject oGameObject)
        {
            // WMIC path win32_process get Commandline | grep EnableMarqueServer
            // "D:\Emulators\RocketLauncher\RocketLauncher.exe" -EnableMarqueServer -s "Atari 7800" -r "D:\Emulators\Roms\Atari 7800\3D Asteroids (1987) (Atari) (Prototype).a78"

            this.oGameObject = oGameObject;
            XMLSettings.LoadPlatformXML();
            
            Console.WriteLine("Available IP Addresses");
            foreach (var i in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            foreach (var ua in i.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine("   " + ua.Address);
                }
            }

            Console.WriteLine();

            if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["IPAddress"]))
            {
                Console.WriteLine("Using IP Address bindings from App.config...");

                var saConfigIPAddresses = ConfigurationManager.AppSettings["IPAddress"].Split(',');
                IPAddress oIPAddress;

                foreach (string sConfigIPAddress in saConfigIPAddresses)
                {
                    if (IPAddress.TryParse(sConfigIPAddress, out oIPAddress))
                    {
                        Console.WriteLine("   Binding to " + oIPAddress.ToString() + " on port " + iPort);
                        HttpServer httpServer = new MyHttpServer(oIPAddress, iPort);
                        httpServer.oGameObject = oGameObject;
                        Thread thread = new Thread(new ThreadStart(httpServer.listen));
                        thread.Start();
                    }
                }
            }
            else
            {
                Console.WriteLine("Binding to all available IP Addresses...");

                foreach (var i in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                foreach (var oIPAddress in i.GetIPProperties().UnicastAddresses)
                {
                    if (oIPAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Console.WriteLine("   Binding to " + oIPAddress.Address.ToString() + " on port " + iPort);

                        try
                        {
                            HttpServer httpServer = new MyHttpServer(oIPAddress.Address, iPort);
                            httpServer.oGameObject = oGameObject;
                            Thread thread = new Thread(new ThreadStart(httpServer.listen));
                            thread.Start();
                        }
                        catch
                        {
                            Console.WriteLine("Could not bind to " + oIPAddress.Address.ToString());
                        }
                    }
                }
            }
            
        }

        public void Dispose()
        {
        }
    }
}