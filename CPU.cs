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
     */
    class CPU
    {
        private Memory _RAM;
        private Register[] _reg;

        //CPU constructor
        public CPU(ref Memory RAM, ref Register[] reg)
        {
            _RAM = RAM;
            _reg = reg;
        }

        //fetches data from RAM 
        public Memory fetch()
        {
            Memory cmd = new Memory(4);
            cmd.WriteWord(0, _RAM.ReadWord(_reg[15].ReadWord(0)));
            Logger.Instance.writeLog(String.Format("CMD: 0x{0}", Convert.ToString(cmd.ReadWord(0), 16)));

            return cmd;
        }

        //instruction decoder
        public Instruction decode(Memory data)
        {
            InstructionParser parser = new InstructionParser();
            Instruction inst = parser.parse(data);            
            return inst;
        }

        //executes the actual data by movine registers and stuff
        public bool[] execute(Instruction command, bool[] flags)
        {            
            if (command.checkCond(flags))
            {
                command.Run(ref _reg, ref _RAM);                
            }

            return null;
        }
    }
}
