using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class Instruction
    {
        // variable declarations
        // initial state
        public uint initial { get; set; }
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
        
        public virtual void ParseCommand(Memory command)
        {
            Logger.Instance.writeLog("CMD: UNDISCOVERED");
        }

        public virtual void Run(ref Register[] reg, ref Memory RAM)
        {
            Logger.Instance.writeLog("CMD: UNDISCOVERED");
        }

        public ShifterOperand Shift(bool I, ShifterOperand shiftOp, uint RmVal, Register[] reg)
        {
            if (!I)
            {
                //it's a register!
                if (shiftOp.bit4 && !shiftOp.bit7)
                {
                    //shifted by a register!
                    shiftOp.shiftRM(RmVal, reg[shiftOp.Rs].ReadWord(0, true));

                }
                else
                {
                    //shifted by an immediate value!
                    shiftOp.shiftRM(RmVal, shiftOp.shift_imm);
                }
            }
            return shiftOp;
        }

        internal bool checkCond(bool[] flags)
        {
            bool N = flags[0];
            bool Z = flags[1];
            bool C = flags[2];
            bool F = flags[3];
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
                    if ((C && !F)) { return true; }
                    break;
                case 0x9:
                    if ((!C && F)) { return true; }
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
            return false;
        }
    }    
}
