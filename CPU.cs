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
                this.RAM = RAM;
                this.reg = reg;
            }


        //fetches data from RAM 
        public uint fetch()
        {

            return RAM.ReadWord(reg[15].ReadWord(0)); 
           

        }

        //decodes the int into a command.  like mov r0, r1
        public instruction decode(uint data)
        {
            instruction output = new instruction(data);


            
            return output;
        }

        //executes the actual data by movine registers and stuff
        public void execute(instruction command)
        {

        }

    }
}
