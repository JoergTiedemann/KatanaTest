using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using System.Net.Http;
using System.Security.Principal;
using System.Diagnostics;
using System.Windows.Threading;

public static class Int32Extension
{
    public static void CopyToByteArray(this int source, byte[] destination, int offset)
    {
        if (destination == null)
            throw new ArgumentException("Destination array cannot be null");

        // check if there is enough space for all the 4 bytes we will copy
        if (destination.Length < offset + 4)
            throw new ArgumentException("Not enough room in the destination array");

        destination[offset + 3] = (byte)(source >> 24); // fourth byte
        destination[offset + 2] = (byte)(source >> 16); // third byte
        destination[offset + 1] = (byte)(source >> 8); // second byte
        destination[offset] = (byte)source; // last byte is already in proper position
    }
}

namespace Demo_bugs_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // das muss voeher in einer Konsole gemacht werden damit die useraccesscontrol 
            // uns einen WebServerdienst starten laesst
            //netsh http add urlacl url=http://+:9000/ user=jeder
            //Uri myUri = new Uri(@"http://localhost:9000");
            // das hier damit auf alle adressen reagiert wird auf localhost und auf die Ip Adresse
            string baseAddress = "http://*:12345/";
            //string baseAddress = "http://localhost:12345/";

            // Starte den Thread fuer die Pipe-Verbindung zum Auftragsserver
            PipeThread m_PipeThread = new PipeThread("ToSrv", "FromSrv", ".");
            PipeThread.MainThreadDispatcher = Dispatcher.CurrentDispatcher;
            m_PipeThread.StartPipeWorkerThread();

            
            // das Ganze Zeug hier tut immer 
            //WebApp.Start<Startup>(url: baseAddress);
            //using (WebApp.Start<Startup>(new StartOptions(baseAddress){ ServerFactory = "Microsoft.Owin.Host.HttpListener" }))
            //using (WebApp.Start<Startup>(new StartOptions(baseAddress)))
            //using (WebApp.Start<Startup>(url: baseAddress))
            Console.WriteLine("starte owin host");
            WebApp.Start<Startup>(url: baseAddress);
            {
                // Launch the browser
                //Process.Start(baseAddress);

                // Keep the server going until we're done
                Console.WriteLine("Webservers hosted on " + baseAddress + " It can be tested now, Press Enter to Exit");
                /*
                 * Messageque starten durch Dispatcher.Run
                 * hier bleiben wir haengen und bearbeiten eine Messagequeue, 
                 * erst wenn InvokeShutdown aufgerufen wurde wird der Dispatcher beendet
                 * bei einer Anwendung mit User Interface wie WinForm oder WPF brauchen wir das nicht weil es da schon einen Dispatcher
                 * gibt der laeuft und daher brauchen wir keinen starten
                 * Wir starten aber noch vorher einen annoymen thread in dem wir ein Enter abfangen um den Dispatcher zu beende
                */
                Dispatcher mainDispatcher = Dispatcher.CurrentDispatcher;
                new Thread((ThreadStart)delegate()
                {
                    Console.WriteLine("Press <Return> to close the application");
                    Console.ReadLine();
                    AppShutdownDispatcher(mainDispatcher);
                }).Start();
                Dispatcher.Run();
                //Console.ReadLine();
            }
            m_PipeThread.ShutdownClienttoSrv();
            m_PipeThread.WorkerThread.Join();
            Console.WriteLine("Feierabend !, Press Enter to Exit");
            Console.ReadLine();
        }

        public static void AppShutdownDispatcher(Dispatcher theMainDispatcher)
        {
            if (theMainDispatcher.CheckAccess())
            {
                theMainDispatcher.InvokeShutdown();
            }
            else
            {
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Send, (Action)(() => 
                {
                    theMainDispatcher.InvokeShutdown(); 
                }));
            }
        }
    }
}




