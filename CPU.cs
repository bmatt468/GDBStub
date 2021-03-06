﻿using System;
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
    // This class is the main computer for the simulator.
    // This class controls the fetching, decoding, and execution
    // of instructions.
    class CPU
    {
        private Memory _ram;
        private Register[] _reg;

        public Register[] GetReg()
        {
            return _reg;
        }
        
        /// <summary>
        /// CPU constructor
        /// </summary>
        /// <param name="RAM"></param>
        /// <param name="reg"></param>
        public CPU(Memory RAM, Register[] reg)
        {
            _ram = RAM;
            _reg = reg;
        }

        /// <summary>
        /// fetch the next command from memory
        /// </summary>
        /// <returns></returns>  
        public Memory Fetch()
        {
            Memory cmd = new Memory(4);
            cmd.WriteWord(0, _ram.ReadWord(_reg[15].ReadWord(0)));
            Logger.Instance.writeLog(String.Format("Command Received: 0x{0}", Convert.ToString(cmd.ReadWord(0), 16)));
            return cmd;
        }

        /// <summary>
        /// decode the instruction from memory
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Instruction Decode(Memory data)
        {            
            return Instruction.DecodeInstruction(data);
        }

        /// <summary>
        /// executes the decoded instruction
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="flags"></param>
        /// <returns></returns> 
        public bool[] Execute(Instruction cmd, bool[] flags)
        {            
            if (cmd.checkCond(flags))
            {
                for (int i = 0; i < _reg.Length; ++i)
                {
                    //Console.WriteLine("Reg " + i + " = " + Convert.ToString(_reg[i].ReadWord(0),16));
                }
                cmd.Run(ref _reg, ref _ram);
                for (int i = 0; i < _reg.Length; ++i)
                {
                    //Console.WriteLine("Reg " + i + " = " + Convert.ToString(_reg[i].ReadWord(0), 16));
                }
                if (flags[0] != cmd.N || flags[1] != cmd.Z || flags[2] != cmd.C || flags[3] != cmd.F)
                {
                    flags[0] = cmd.N;
                    flags[1] = cmd.Z;
                    flags[2] = cmd.C;
                    flags[3] = cmd.F;
                    return flags;
                }                
            }
            return null;
        }
    }
}
