using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    // this class is the parent class of all
    // the other instructions. The instuction types
    // build off the methods in this class
    class Instruction
    {
        // variable declarations
        // initial state
        public uint initialBytes { get; set; }
        // breakdown of bytes
        public uint Cond { get; set; }
        public uint Type { get; set; }        
        public uint Rd { get; set; }
        public uint Rn { get; set; }       
        public bool S { get; set; }
        public bool N {get; set; } 
        public bool Z {get; set; }
        public bool C {get; set; }
        public bool F {get; set; }
        public uint Rm { get; set; }

        /// <summary>
        /// Decode the instruction
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static Instruction DecodeInstruction(Memory command)
        {
            Instruction i = new Instruction();
            uint type = 0;
            // get type            
            type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
            switch (type)
            {
                case 0:
                    // Data Processing  (00X)
                    i = new DataProcessing();
                    break;
                case 1:
                    // Load / Store (01X)
                    if (command.TestFlag(0, 24) || !command.TestFlag(0, 21))
                    {
                        i = new LoadStore();
                    }
                    break;
                case 2:
                    // Load / Store Multiple (100)
                    if (!command.TestFlag(0, 25))
                    {
                        //load store multiple
                        i = new LoadStoreMultiple();
                    }
                    // Branch Instruction (101)
                    else
                    {
                        if (command.TestFlag(0, 27) && !command.TestFlag(0, 26) && command.TestFlag(0, 25))
                        {
                            i = new Branch();
                        }
                    }
                    break;
                case 3:
                    //11
                    //Coprocessor
                    if (command.TestFlag(0, 26) && command.TestFlag(0, 25))
                    {
                        //Interupts: To be tackled much later
                    }
                    break;
                default:
                    break;
            }

            i.ParseCommand(command);
            // get condition
            i.Cond = (uint)command.ReadByte(3) >> 4;
            // get type
            i.Type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
            //
            i.initialBytes = (uint)command.ReadWord(0);
            // source
            i.Rn = (uint)((command.ReadWord(0) & 0x000F0000) >> 16);
            // destination
            i.Rd = (uint)((command.ReadWord(0) & 0x0000F000) >> 12);
            return i;
        }
        
        /// <summary>
        /// Parent ParseCommand
        /// Shouldn't ever be needed
        /// </summary>
        /// <param name="command"></param>
        public virtual void ParseCommand(Memory command)
        {
            Logger.Instance.writeLog("Warning: Unkown Command");
        }

        /// <summary>
        /// Parent Run
        /// Shouldn't ever be needed
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="RAM"></param>
        public virtual void Run(ref Register[] reg, ref Memory RAM)
        {
            Logger.Instance.writeLog("Warning: Unkown Command");
        }       

        /// <summary>
        /// This is responsible for the shifts that have
        /// to take place at times in the operand2
        /// </summary>
        /// <param name="S"></param>
        /// <param name="shifter"></param>
        /// <param name="RmVal"></param>
        /// <param name="reg"></param>
        /// <returns></returns>
        public Operand2 Shift(bool S, Operand2 shifter, uint RmVal, Register[] reg)
        {
            // Look at our magic bit to see what to do
            if (!S)
            {
                // Check to see if shift is done on a register
                if (shifter.b4 && !shifter.b7) {  shifter.shiftRM(RmVal, reg[shifter.regShiftLength].ReadWord(0, true));}
                // otherwise the shift is an immediate value
                else {shifter.shiftRM(RmVal, shifter.regShiftImm);}
            }
            return shifter;
        }

        /// <summary>
        /// This method checks the status of the flags and
        /// returns true or false based on their condition
        /// </summary>
        /// <param name="flagArray"></param>
        /// <returns></returns>
        public bool checkCond(bool[] flagArray)
        {
            N = flagArray[0];
            Z = flagArray[1];
            C = flagArray[2];
            F = flagArray[3];
            switch (Cond)
            {
                case 0x0:
                    if (Z) { return true; }
                    break;
                case 0x1:
                    if (!Z) { return true; }
                    break;
                case 0x2:
                    if (C) { return true; }
                    break;
                case 0x3:
                    if (!C) { return true; }
                    break;
                case 0x4:
                    if (N) { return true; }
                    break;
                case 0x5:
                    if (!N) { return true; }
                    break;
                case 0x6:
                    if (F) { return true; }
                    break;
                case 0x7:
                    if (!F) { return true; }
                    break;
                case 0x8:
                    if ((C && !Z)) { return true; }
                    break;
                case 0x9:
                    if ((!C && Z)) { return true; }
                    break;
                case 0xa:
                    if ((N == F)) { return true; }
                    break;
                case 0xb:
                    if ((N != F)) { return true; }
                    break;
                case 0xc:
                    if ((!Z && N == F)) { return true; }
                    break;
                case 0xd:
                    if ((Z || N != F)) { return true; }
                    break;
                case 0xe:
                    return true;
                case 0xf:
                    return false;
                default:
                    return false;
            }
            // should never ever ever ever need this
            // but C# doth compain if it is not here
            return false;
        }
    }    
}
