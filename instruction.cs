using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class Instruction
    {
        public uint originalBits { get; set; }

        public uint rd { get; set; }
        public uint rn { get; set; }

        public uint cond { get; set; }
        public uint type { get; set; }

        public bool S { get; set; }

        public Instruction()
        {
            ;
        }
        public virtual void parse(Memory command)
        {
            Logger.Instance.writeLog("CMD: UNDISCOVERED");
        }

        
        public uint rm { get; set; }
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

        public override void parse(Memory command)
        {
            //PUBWL
            bool R = command.TestFlag(0, 25);


            if (!(command.TestFlag(0, 25) && command.TestFlag(0, 4)))
            {
                this.R = command.TestFlag(0, 25); ;
                this.P = command.TestFlag(0, 24);
                this.U = command.TestFlag(0, 23);
                this.B = command.TestFlag(0, 22);
                this.W = command.TestFlag(0, 21);
                this.L = command.TestFlag(0, 20);


                this.shiftOp = new ShifterOperand(command);

            }






        }

    }


    class dataManipulation : Instruction
    {
        public uint opcode { get; set; }
        public bool I { get; set; }
        public ShifterOperand shiftOp { get; set; }
        public bool bit4 { get; set; }
        public bool bit7 { get; set; }

        public override void parse(Memory command)
        {

            //Get S Byte
            this.I = command.TestFlag(0, 25);
            this.S = command.TestFlag(0, 20);
            this.bit4 = command.TestFlag(0, 4);
            this.bit7 = command.TestFlag(0, 7);
            //dataManipulation dataManinstruct = new dataManipulation();

            if (!(!I && bit4 && bit7))
            {
                //it's data man

                //get OpCode
                uint c = command.ReadWord(0);
                this.opcode = (uint)((c & 0x01E00000) >> 21);
                this.shiftOp = new ShifterOperand(command);


            }
            //it's a multpiply



        }



    }

    class Branch : Instruction
    {
        public bool LN { get; set; }
        //23bit long offset
        public int offset { get; set; }


        public override void parse(Memory command)
        {
            this.LN = command.TestFlag(0, 24);
            this.offset = ((int)command.ReadWord(0) & 0x00FFFFFF) << 2;

            

        }

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

    class dataMoveMultiple : dataMovement
    {
        public bool[] regFlags { get; set; }


        public override void parse(Memory command)
        {
           // dataMoveMultiple this = (dataMoveMultiple)parseLoadStore(command);
            this.regFlags = new bool[16];
            this.R = command.TestFlag(0, 25); ;
            this.P = command.TestFlag(0, 24);
            this.U = command.TestFlag(0, 23);
            this.B = command.TestFlag(0, 22);
            this.W = command.TestFlag(0, 21);
            this.L = command.TestFlag(0, 20);
            for (byte i = 0; i < 16; ++i)
            {
                this.regFlags[i] = command.TestFlag(0, i);
            }
        }
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
