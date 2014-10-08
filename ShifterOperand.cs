using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{



    //11..0
    class ShifterOperand
    {

        bool I;
        
        uint rotate_imm;
        uint immed_8;
        uint shift;
        uint Rm;
        uint Rs;
        bool bit7;
        uint shift_imm;
            

        public uint offset { get; set; }

        public ShifterOperand(Memory command)
        {
            uint c = command.ReadWord(0);
            //if it's an Immediate rotate it!
            if (command.TestFlag(0, 25))
            {
                //immediate rotated
                rotate_imm = (command.ReadWord(0) & 0x00000F00) >> 8;
                immed_8 = (command.ReadWord(0)    & 0x000000FF);
                offset = RotateRight(immed_8, (int)rotate_imm);
            }// immediate
            else
	        {
                //it's a register!!!
                if (command.TestFlag(0,4))
                {
                    if(!command.TestFlag(0,7))
                    {
                        //shifted by a register!
                        Rm = (uint)(command.ReadWord(0) & 0x0000000F);
                        Rs = (uint)((command.ReadWord(0) & 0x00000F00) >> 8);
                        shift = (uint)((command.ReadWord(0) & 0x00000060) >> 5);

                    } // shifted by reg
                    else 
                    {
                            // psych it's a multiply!!
                            // do craziness
                    }
                }else
                {
                    //shifted by an immediate value!
                    Rm = (uint)(command.ReadWord(0) & 0x0000000F);
                    shift = (uint)((command.ReadWord(0) & 0x00000060) >> 5);
                    shift_imm = (uint)((command.ReadWord(0) & 0x00000F80) >> 7);          
                }//immediate val
                        

	        }//shifteroperand

        }
        

    public uint RotateRight(uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }

       
    }
}
