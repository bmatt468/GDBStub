using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{

    /*
     * CPU class that represents the CPU. This should have methods named 
     * fetch, decode, and execute. The CPU class should have instance variables 
     * that hold references to the registers and RAM objects.
     * 
     */
    class CPU
    {
        //variables that hold references to the regs and RAM
        public uint fetch()
        {
            uint output = 0;


            return output;
        }

        public string decode(UInt32 data)
        {
            string output = "";
            //decode data


            return output;
        }

        public void execute(string command)
        {

        }

    }
}
