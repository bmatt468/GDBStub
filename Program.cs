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
            
            StreamWriter log = new StreamWriter("log.text");
            log.WriteLine("Test");

            if (Option.Instance.parseArgs(args))
            {
                Handler handle = new Handler();
                handle.Listen();
            }
        }
    }
}
