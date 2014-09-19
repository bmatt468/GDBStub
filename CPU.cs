using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;


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
        private Memory RAM;
        private Register[] reg;

        //variables that hold references to the regs and RAM

        //CPU instantiation

        public CPU(ref Memory RAM, ref Register[] reg)
            {
                // TODO: Complete member initialization
                this.RAM = RAM;
                this.reg = reg;
            }

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
