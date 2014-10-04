using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                if (Option.Instance.getFile() != "")
                {
                    //wanted some data to play with
                    //specified through command line, load the file
                    Computer.Instance.readELF(Option.Instance.getFile(), Option.Instance.getMemSize());
                }

                if (Option.Instance.getDebug())
                {   

                    Handler h = new Handler();
                    
                    h.Listen(8080);
	            }
                //if debug flag is not set. run

                
                danielTesting(args);

            
            }
            
           
        }


        static void danielTesting(string[] args)
        {

                if (Option.Instance.getTest())
                {
                    //run tests
                    TestRam.RunTests();
                    TestSimulator.RunTests();
                    Console.WriteLine("All tests passed see log.txt for details");
                   

                }
                    if (Option.Instance.getFile() != "")
                    {
                        //specified through command line, load the file
                        Logger.Instance.writeLog("File: Loading");
                        Computer.Instance.readELF(Option.Instance.getFile(), Option.Instance.getMemSize());
                        Console.WriteLine("File Loaded");

                        Logger.Instance.writeLog("File: Loaded");
                    }
            //THis will be replaces later
            //with information of just running
                    Console.WriteLine("\n");
                    Console.Write("Please input a command: ");
                    string input = Console.ReadLine();
                    Logger.Instance.openTrace();
                    while (input != "q" && input != "")
                    {
                        Computer.Instance.command(input);
                        Console.Write("\nPlease input a command: ");
                        //Run, Step, Stop/Break, and Reset
                        input = Console.ReadLine();
                    }

         }//DanielTest



    }//programClass


}//namespace


