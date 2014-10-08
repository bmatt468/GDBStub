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



    }


    class dataManipulation : Instruction
    {
        public uint opcode { get; set; }
        public bool I { get; set; }
        public ShifterOperand shiftOp { get; set; }




    }

}
