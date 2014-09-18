using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBStub
{

    class Handler
    {

        
        public void Listen()
        {
            Console.Write("Please input a command: ");
            string input = Console.ReadLine();
            while (input != "")
            {
                Console.Write("Please input a command: ");
                //Run, Step, Stop/Break, and Reset
                input = Console.ReadLine();
               // Option.Instance.               
            }

        }
    }
}
