using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{

    //11..0
    class ShifterOperand : Operand2
    {
        bool I;
        //if(I){
            //11..8//
            byte rotate_imm;
            //7..0
            byte immed_8;
        //} else{
            //6..5
            byte shift;
            //3..0
            byte Rm;
        // if (test bit 4){
                //11..8    
                byte Rs;
                //7
                bool bit7;
            //} else {
                //11..7
                byte shift_imm;
            //}

                public ShifterOperand(Memory command)
                {

                }
        
        


    }
}
