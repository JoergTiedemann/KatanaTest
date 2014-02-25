using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Windows.Threading;
/* 
 * Klasse in der die Pipverbindung zum Auftragsserver aufggebaut und verarbeitet wird
 */

    class PipeThread
    {
        // Definitionen fuer die Nummern der Oberflächendienste 
        const int ClientEnde = 0;
        const int MessageInfo = 1;
        const int StatusInfo = 2;
        const int FktInfo = 3;
        const int AbbruchInfo = 4;
        const int KranLeerenInfo = 5;
        const int AusrInfo = 6;
        const int PlatzLeerInfo = 7;
        const int UmraeumInfo = 8;
        const int ZwischenLagInfo = 9;
        const int PrioRequest = 10;
        const int SetPrio = 11;
        const int OptionRequest = 12;
        const int SetOption = 13;
        const int LockEditInfo = 14;
        const int UnLockEditInfo = 15;
        const int ListenUpdate = 16;
        const int ServiceData = 17;
        const int BadMessage = 18;
        const int QuitBadMessage = 19;
        const int HolzmaListRequest = 20;
        const int ManMessage = 21;
        const int QuitManMessage = 22;
        const int DebugWindow = 23;
        const int RequestFahrState = 24;
        const int CloseFahrState = 25;
        const int MasterAbbrInfo = 26;
        const int MasterFktStart = 27;
        const int ProdListOptions = 28;
        const int AbraeumData = 29;
        const int GetPlatzState = 30;
        const int ResetPlatzState = 31;
        const int ServerMessage = 32;
        const int QuitServerMessage = 33;
        const int BlkInfoUpdate = 34;
        const int SaegenKommu = 35;
        const int OptionUpdate = 36;
        const int IntelliStoreRequest = 37;
        const int IntelliStoreWrite = 38;
        const int StapelAbdecken = 39;
        const int MessageArr = 40;
        const int StapelAusfoerdern = 41;
        const int MessageFromDesktop = 43;
        const int GewichtRequest = 44;
        const int GewichtRequestAusgabe = 45;
        const int PlatzFuellenMessage = 46;
        const int QuitPlatzFuellenMessage = 47;
        const int EntladenData = 48;
        const int ClientWache = 49;
        const int MessageQuit = 50;
        const int TeilAbgegeben = 51;
        const int LicenseInfo = 52;
        const int TeilAufgenommen = 53;
        const int EinAbraeumData = 54;
        const int PlatzUpdate = 55;
        const int Etikettieren = 56;

        static NamedPipeClientStream SendPipeClient;
        static NamedPipeClientStream EmpfPipeClient;
        public Thread WorkerThread;
        static int ThreadStop;
        string PipeNametoSrv;
        string PipeNamefromSrv;
        string ServerName;
        static DModul.sFlagTab FlagTab;
        public static Dispatcher MainThreadDispatcher;
        public delegate void MyDelegate(int i, string str);
        public static MyDelegate del;

        public PipeThread(string sPipeNametoSrv, string sPipeNamefromSrv, string sServerName)
        {
            ServerName = sServerName;
            PipeNametoSrv = sPipeNametoSrv;
            PipeNamefromSrv = sPipeNamefromSrv;
            ThreadStop = 0;
            FlagTab = new DModul.sFlagTab();
            del = new MyDelegate(this.FuncToExec);   
        }
        public void FuncToExec(int i, string str)
        {
            Console.WriteLine("Delegate via Dispatcher from Mainthread;i={0} str={1}",i,str);
            // Do something   
        }   

        public void StartPipeWorkerThread()
        {

            SendPipeClient = new NamedPipeClientStream(ServerName,PipeNametoSrv,
                PipeDirection.InOut, PipeOptions.None,
                TokenImpersonationLevel.Impersonation);

            SendPipeClient.Connect();
            SendPipeClient.ReadMode = PipeTransmissionMode.Message;
            if (SendPipeClient.IsConnected)
            {
                // Sender und Empfaengerpipe verbunden;
                EmpfPipeClient = new NamedPipeClientStream(ServerName, PipeNamefromSrv,
                    PipeDirection.InOut, PipeOptions.None,
                    TokenImpersonationLevel.None);
                  
                EmpfPipeClient.Connect();
                EmpfPipeClient.ReadMode = PipeTransmissionMode.Message;
                WorkerThread = new Thread(new ThreadStart(EmpfangsThread));
                WorkerThread.Name = "PipeEmpfangsThread";
                WorkerThread.Start();
                //ReadEmpfangsPipe();

                //ssSend.WriteString("A");
                //ssSend.ReadString();
            }
        }
        // dies ist der verdammte Empfangsthread
        static void EmpfangsThread()
        {
            try
            {
                int len = 500000;
                byte[] inBuffer = new byte[len];
                while (ThreadStop != -1)
                {
                    if (EmpfPipeClient.IsConnected)
                    {
                        int anzread = EmpfPipeClient.Read(inBuffer, 0, len);
                        if (anzread > 0)
                        {
                            int Infotyp = BitConverter.ToInt32(inBuffer, 0);
                            // Read from the pipe. 
                            switch (Infotyp)
                            {
                                case ClientEnde:
                                    // Thread wird beendet 
                                    Console.WriteLine("ClientEnde");
                                    MainThreadDispatcher.InvokeShutdown();
                                    ThreadStop = -1;
                                    break;
                                case StatusInfo:
                                    Console.WriteLine("StatusInfo");
                                    byte[] lBuffer = new byte[len];
                                    Array.Copy(inBuffer, sizeof(int), lBuffer, 0, len - sizeof(int));
                                    FlagTab.LoadFromArray(lBuffer);
                                    // nun via Updatetimer den Delegaten aufrufen der die Benachrichtigung durchführt
                                    MainThreadDispatcher.Invoke(del, new object[] { Infotyp, "StatusInfo" });
                                    //if (m_UpdateTimerRunning != 1)
                                    //    theApp.m_pMainWnd->PostMessage(WM_COMMAND, ID_StatusUpdate);
                                    break;
                                case MessageInfo:
                                    Console.WriteLine("MessageInfo");
                                    //WaitForSingleObject(hMutex,INFINITE);
                                    //pMessInfo= new MessInfo;
                                    //m_PipeMessageArr.Add(pMessInfo);
                                    //memcpy(&pMessInfo->ErrLev,&chBuf[0]+sizeof(Infotyp),sizeof(m_MessInfo.ErrLev));
                                    //memcpy(&pMessInfo->ErrNr,&chBuf[0]+sizeof(Infotyp)+sizeof(m_MessInfo.ErrLev),sizeof(m_MessInfo.ErrNr));
                                    //memcpy(&pMessInfo->str[0],&chBuf[0]+sizeof(Infotyp)+sizeof(m_MessInfo.ErrLev)+sizeof(m_MessInfo.ErrNr),cbRead-(sizeof(Infotyp)+sizeof(m_MessInfo.ErrLev)+sizeof(m_MessInfo.ErrNr)));
                                    //theApp.m_pMainWnd->PostMessage(WM_COMMAND, ID_MessUpdate);
                                    //ReleaseMutex(hMutex);
                                    break;
                                case MessageArr:
                                    Console.WriteLine("MessageArr");
                                    //memcpy(&m_MessageArray,&chBuf[0]+sizeof(Infotyp),sizeof(m_MessageArray));
                                    //theApp.m_pMainWnd->SendMessage(WM_COMMAND, ID_MessSync);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string strerr = e.ToString();
                Console.WriteLine("Empfangsthread abgebrochen:\n{0}",strerr);
            }
            EmpfPipeClient.Close();
            SendPipeClient.Close();
        }

        
        public void ReadEmpfangsPipe()
        {
            int len;
            //len = ioStream.ReadByte() * 256;
            //len += ioStream.ReadByte();
            len = 500000;
            byte[] inBuffer = new byte[len];

            int anzread=EmpfPipeClient.Read(inBuffer, 0, len);
            int infotyp = BitConverter.ToInt32(inBuffer, 0);
            Console.WriteLine("Infotyp: {0}",infotyp);
            return ;
        }

        public int WriteEmpfangsPipe(byte[] SendeBuffer)
        {
            //byte[] outBuffer = streamEncoding.GetBytes(outString);
            //int len = outBuffer.Length;
            //if (len > UInt16.MaxValue)
            //{
            //    len = (int)UInt16.MaxValue;
            //}
            //ioStream.WriteByte((byte)(len / 256));
            //ioStream.WriteByte((byte)(len & 255));
            int len = 500000;
            //byte[] outBuffer = new byte[len];
            int infotyp = 37;
            //infotyp.CopyToByteArray(outBuffer, 0);

            byte[] outBuffer = BitConverter.GetBytes(infotyp);
            len = sizeof(int);
            EmpfPipeClient.Write(outBuffer, 0, len);
            EmpfPipeClient.Flush();
            return len;
        }

        public void ShutdownClienttoSrv()
        {
        	int infotyp=ClientEnde;
            byte[] arrinfotyp = BitConverter.GetBytes(infotyp);
            int DownGrund=0;// nur ein Client faehrt runter
            byte[] arrDownGrund = BitConverter.GetBytes(DownGrund);
            MemoryStream memStream = new MemoryStream();
            memStream.Write(arrinfotyp,0,arrinfotyp.Length);
            memStream.Write(arrDownGrund, 0, arrDownGrund.Length);
            int len = (int)memStream.Length;
            byte[] outBuffer = memStream.ToArray();
            if (SendPipeClient.IsConnected)
            {
                try
                {
                    SendPipeClient.Write(outBuffer, 0, len);
                    SendPipeClient.Flush();
                }
                catch (Exception e)
                {
                    string strerr = e.ToString();
                    Console.WriteLine("Schreiben des Shutdown nicht möglich:\n{0}", strerr);
                    WorkerThread.Abort();
                }
            }
            else
            {
                // ok  keine Verbindung mehr zu Server und runterfahren wollten wir auch -> ergo den Thread anhalten
                WorkerThread.Abort();
            }
        }
   }
    
    
