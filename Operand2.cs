using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    // This class contains the information 
    // for bit 11 to 0 (aka Operand2)
    class Operand2
    {
        // Key Bits
        public bool b4 { get; set; }
        public bool b7 { get; set; }  

        // Shifting Information        
        public uint Sh { get; set; }
        public uint Rm { get; set; }
        public uint regShiftLength { get; set; }
        public uint regShiftImm { get; set; }             

        // Immediates
        public uint imm12 { get; set; }
        public uint imm8 { get; set; }
        public uint imm8rotate { get; set; }
        
        // Offset
        public uint offset { get; set; }

        public Operand2(Memory cmd)
        {
            uint c = cmd.ReadWord(0);
            // Check to see is command is with immediates
            // To do so we check bit 25
            if (cmd.TestFlag(0, 25))
            {
                // If it is an imm, we get the amount it is rotated by
                imm8rotate = (cmd.ReadWord(0) & 0x00000F00) >> 8;
                // next we get the 8 bit immediate
                imm8 = (cmd.ReadWord(0) & 0x000000FF);
                // then we take care of potential shifts
                // first we get the potential offset
                offset = ROR(imm8, ((int)imm8rotate * 2));
                // then the potential shift commands
                Sh = (uint)((cmd.ReadWord(0) & 0x00000060) >> 5);
                regShiftImm = (uint)((cmd.ReadWord(0) & 0x00000F80) >> 7);
            }
            // if the if statment fails then we are working with a register
            else
            {               
                // first we gather the potential 12 bit immediate
                imm12 = (cmd.ReadWord(0) & 0x00000FFF);
                // first we test to see if register shifted register
                // or immediate shifted register
                // this is done by testing bit number 4
                if (cmd.TestFlag(0, 4))
                {
                    // if bit 4 is set then we shift by a register
                    b4 = true;
                    if (!cmd.TestFlag(0, 7))
                    {
                        b7 = false;
                        Rm = (uint)(cmd.ReadWord(0) & 0x0000000F);
                        regShiftLength = (uint)((cmd.ReadWord(0) & 0x00000F00) >> 8);
                        Sh = (uint)((cmd.ReadWord(0) & 0x00000060) >> 5);
                    } 
                    else
                    {
                        Rm = (uint)(cmd.ReadWord(0) & 0x0000000F);
                        regShiftLength = (uint)((cmd.ReadWord(0) & 0x00000F00) >> 8);
                    }
                }
                // if test fails it is immediate shifted register
                else
                {
                    Rm = (uint)(cmd.ReadWord(0) & 0x0000000F);
                    Sh = (uint)((cmd.ReadWord(0) & 0x00000060) >> 5);
                    regShiftImm = (uint)((cmd.ReadWord(0) & 0x00000F80) >> 7);
                }
            }
        }

        public uint ROR(uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }

        internal void shiftRM(uint shiftee, uint shifter)
        {
            switch (Sh)
            {
                case 00:
                    // Logical Shift Left
                    offset = shiftee << (int)shifter;
                    break;
                case 01:
                    // Logical Shift Right
                    offset = shiftee >> (int)shifter;
                    break;
                case 2:
                    // Arithmatic Shift Right
                    offset = (uint)((int)shiftee >> (int)shifter);
                    break;
                case 3:
                    // Magical Rotations Happening
                    offset = ROR(shiftee, (int)shifter);
                    break;
                default:
                    // hopefully this never happens
                    offset = 0;
                    break;
            }
        }
    }
}
