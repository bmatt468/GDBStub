using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;

namespace GDBStub
{
   
    class Program
    {



        static void Main(string[] args)
        {
           
            Handler h = new Handler();
            h.Listen(8080);
            
            //danielTesting(args);
           
        }


        static void danielTesting(string[] args)
        {
            StreamWriter log = new StreamWriter("log.txt");
            StreamWriter trace = new StreamWriter("trace.log");
            trace.Close();
            log.WriteLine("Opened the file");
            log.Close();
            if (Option.Instance.parseArgs(args))
            {
                if (Option.Instance.getTest())
                {
                    //run tests
                    TestRam.RunTests();
                    TestSimulator.RunTests();
                    Console.WriteLine("All tests passed see log.txt for details");

                }
                    Computer comp = new Computer();
                    if (Option.Instance.getFile() != "")
                    {
                        //specified through command line, load the file
                        comp.readELF(Option.Instance.getFile(), Option.Instance.getMemSize());
                    }

                    Console.WriteLine("\n");
                    Console.Write("Please input a command: ");
                    string input = Console.ReadLine();

                    while (input != "q" || input != "")
                    {
                        comp.command(input);
                        Console.Write("\nPlease input a command: ");
                        //Run, Step, Stop/Break, and Reset
                        input = Console.ReadLine();
                    }
             }
             else
             {
                   //invalid command line arguments
             }
         }//DanielTest



    }//programClass


}//namespace

