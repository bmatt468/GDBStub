using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{



    //11..0
    class ShifterOperand
    {
        public uint rotate_imm { get; set; }
        public uint immed_8 { get; set; }
        public uint shiftType { get; set; }
        public uint Rm { get; set; }
        public uint Rs { get; set; }
        public uint shift_imm { get; set; }
        public bool bit7 { get; set; }
        public bool bit4 { get; set; }



        public uint offset { get; set; }

        public ShifterOperand(Memory command)
        {
            uint c = command.ReadWord(0);
            //if it's an Immediate rotate it!
            if (command.TestFlag(0, 25))
            {
                //immediate rotated
                rotate_imm = (command.ReadWord(0) & 0x00000F00) >> 8;
                immed_8 = (command.ReadWord(0) & 0x000000FF);
                offset = RotateRight(immed_8, ((int)rotate_imm * 2));
            }// immediate

            else
            {
                //it's a register!!!
                //this is set here but done in execute
                if (command.TestFlag(0, 4))
                {
                    bit4 = true;
                    if (!command.TestFlag(0, 7))
                    {
                        bit7 = false;
                        //shifted by a register!
                        Rm = (uint)(command.ReadWord(0) & 0x0000000F);
                        Rs = (uint)((command.ReadWord(0) & 0x00000F00) >> 8);
                        shiftType = (uint)((command.ReadWord(0) & 0x00000060) >> 5);

                    } // shifted by reg
                    else
                    {
                        // psych it's a multiply!!
                        //shouold probably catch this case earlier
                        // do craziness
                    }
                }
                else
                {
                    //shifted by an immediate value!
                    Rm = (uint)(command.ReadWord(0) & 0x0000000F);
                    shiftType = (uint)((command.ReadWord(0) & 0x00000060) >> 5);
                    shift_imm = (uint)((command.ReadWord(0) & 0x00000F80) >> 7);
                }//immediate val


            }//shifteroperand

        }


        public uint RotateRight(uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }






        internal void shiftRM(uint toBeShifted, uint shiftedBy)
        {
            switch (shiftType)
            {
                case 00:
                    //lsl
                    offset = toBeShifted << (int)shiftedBy;
                    break;
                case 01:
                    //LSR
                    offset = toBeShifted >> (int)shiftedBy;
                    break;
                case 2:
                    //ASR
                    offset = (uint)((int)toBeShifted >> (int)shiftedBy);
                    break;
                case 3:
                    //ROR ROX
                   
                    break;

                default:
                    offset = 0;
                    break;
            }
        }
    }//class


    class coProcessorOperand
    {

    }
}
