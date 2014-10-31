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
                    TestSimulator LETSFINDOUT = new TestSimulator();
                    LETSFINDOUT.IFTHISTHINGWORKS();
                    Logger.Instance.writeLog("TEST: Finished\n\n\n------------------------------");                    
                }

                Logger.Instance.openTrace();

                // pre-loads a file
                if (Option.Instance.load || Option.Instance.debug)
                {
                    MAKETHISTHINGWORK(Option.Instance.load, Option.Instance.exec, Option.Instance.debug);
                }              
            }           
        }

        static void MAKETHISTHINGWORK(bool iCanHazFilePlz, bool iCanBeRunPlz, bool iCanHazDebugModePlz)
        {            
            // first do loady type stuff
            if (iCanHazFilePlz)
            {
                Computer.Instance.load(Option.Instance.file, Option.Instance.memSize);
                // then do executy type stuff
                if (iCanBeRunPlz)
                {
                    Handler potentialHandlerIfTheRightFlagIsSet = null;
                    if (iCanHazDebugModePlz)
                    {
                        potentialHandlerIfTheRightFlagIsSet = new Handler();
                        Thread thisThreadWillHandleTheHandler_AKATheHandleHandler = new Thread(
                            new ThreadStart(potentialHandlerIfTheRightFlagIsSet.StartReadInteruptOnly));
                        thisThreadWillHandleTheHandler_AKATheHandleHandler.Start();
                    }
                    Computer.Instance.run();
                    while (Computer.Instance.getThreadStatus())
                    {
                        if (iCanHazDebugModePlz && potentialHandlerIfTheRightFlagIsSet.interput)
                        {
                            Console.WriteLine("Interupt Found.... now what?");
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            } 
            // if not loady type stuff then try debugy type stuff
            else if (iCanHazDebugModePlz)
            {
                Handler GDBMiesterHandlerOfSuperAwesomenessReadySetGoYouCanTellImASeniorBecauseMyVariableNamesAreLongAndPointlessYeaRollTide = new Handler();
                GDBMiesterHandlerOfSuperAwesomenessReadySetGoYouCanTellImASeniorBecauseMyVariableNamesAreLongAndPointlessYeaRollTide.Start();
            }

            System.Environment.Exit(0);
        }  
    }
}


