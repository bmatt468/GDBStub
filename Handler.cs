using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBStub
{

    class Handler
    {

        Computer comp;
        public void Listen()
        {

            /*tons of gdb logic
             * 
             * 
             * 
             * 
             * 
             * 
             */
            //you can do what you want with this was using it to test my 
            //thread logic.
            comp = new Computer();
            if (Option.Instance.getFile() != "")
            {
                //specified through command line, load the file
                comp.readELF(Option.Instance.getFile(), Option.Instance.getMemSize());
            }
            Console.WriteLine("\n");
            Console.Write("Please input a command: ");
            string input = Console.ReadLine();

            while (input != "")
            {
                string result = comp.command(input);
                Console.WriteLine(result);
                Console.Write("\nPlease input a command: ");
                //Run, Step, Stop/Break, and Reset
                input = Console.ReadLine();
            }

        }
    }
}
