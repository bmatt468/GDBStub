using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class Instruction
    {
        public uint rd { get; set; }
        public uint rn { get; set; }

        public uint cond { get; set; }
        public uint type { get; set; }

        public bool S { get; set; }

        public Instruction()
        {
            ;
        }


    }


    class dataMovement : Instruction
    {
        public bool R { get; set; }
        public bool P { get; set; }
        public bool U { get; set; }
        public bool B { get; set; }
        public bool W { get; set; }
        public bool L { get; set; }

        public ShifterOperand shiftOp { get; set; }



    }


    class dataManipulation : Instruction
    {
        public uint opcode { get; set; }
        public bool I { get; set; }
        public ShifterOperand shiftOp { get; set; }




    }

    class Branch : Instruction
    {
        public bool LN { get; set; }
        //23bit long offset
        public int offset { get; set; }
    }

    class Multiply : Instruction
    {

    }

    class Swap : Instruction
    {

    }

    class MRS : Instruction
    {

    }

    class MSR : Instruction
    {

    }

    class dataMoveMultiple : Instruction
    {

    }

    class CoProcessorInstruction : Instruction
    {
        coProcessorOperand operand;
    }

    class Transfer : CoProcessorInstruction
    {

    }

    class Op : CoProcessorInstruction
    {

    }

    class RTransfer : CoProcessorInstruction
    {

    }

    class SWI : CoProcessorInstruction
    {

    }


}
