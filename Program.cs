using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;



//I guess this doesn't work..... worth a try
//using School.BJU.Class.CPS310.MicroProcessor.Version.100

namespace GDBStub
{
   
    class Program
    {
        static void Main(string[] args)
        {
            if (Option.Instance.parseArgs(args))    //verify the proper command line input
            {
                Logger.Instance.clearLog();

                //runs test cases
                if (Option.Instance.test)
                {                   
                    TestRam.RunTests();
                    TestSimulator.RunTests();
                    TestDecodeExecute TFDE = new TestDecodeExecute();
                    TFDE.RunTests();

                    Logger.Instance.writeLog("TEST: Finished\n\n\n------------------------------");                    
                }

                Logger.Instance.openTrace();

                // pre-loads a file
                if (Option.Instance.load)
                {
                    DoWork();
                }
            }           
        }

        static void DoWork()
        {
            // handler thread
            Handler h = new Handler();
            Thread handlerThread = new Thread(new ThreadStart(h.Start));

            // loading thread
            Thread loaderThread = new Thread(new ThreadStart(Computer.Instance.Start));

            // start threads
            handlerThread.Start();
            //loaderThread.Start();

            // spool to allow for life to enter the threads
            while (!handlerThread.IsAlive || !loaderThread.IsAlive) ;
        }  
    }
}


