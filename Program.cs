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
            h.Listen2(8080);
            
            /*StreamWriter log = new StreamWriter("log.txt");
            log.WriteLine("Test");
            log.Close();
            if (Option.Instance.parseArgs(args))
            {
                if (Option.Instance.getTest())
                {
                    //run tests
                }
                Handler handle = new Handler();
                handle.Listen();
            }
            else
            {
                //invalid command line arguments
            }*/
        }
    }
}
