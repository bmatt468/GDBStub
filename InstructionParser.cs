using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GDBStub
{
    //the instruction class object
    class InstructionParser
    {
        uint conditional = 0xe; // 1110  4
        string typeStr = "";
        uint type = 0;          // xx   2 determines IPUBWL, or Opcode
        bool I, P, U, B, W, L = false; // xxxxxx 6
        bool bit4, bit7 = false;
        uint opcode = 0;
        bool S = false;


        Instruction instruct = new Instruction();


        public Instruction parse(Memory command)
        {
            //initialize self based on command

            //decode data
            //Check special cases

            //Get the conditional

            //get the type number
            this.type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
            //Get immediate value or not.
            //get the RN register
            


            //switches based on type
            switch (this.type)
            {
                case 0:
                    //data manipulation 00


                    instruct = parseDataManipulation(command);


                    break;
                case 1:
                    //ldr/str 01
                    // check the PUBWL 
                    if (command.TestFlag(0, 24) || !command.TestFlag(0, 21))
                    {
                        instruct = parseLoadStore(command);
                        instruct.rm = (uint)((command.ReadWord(0) & 0xF));
   

                    }
                    else
                    {
                        //unpredictable
                    }


                    break;
                case 2:
                    //10
                    //load store multiple
                    if (!command.TestFlag(0, 25))
                    {
                        instruct = parseLoadStoreMultiple(command);
                    }
                    // branch with link
                   // instruct = parseLoadStoreMultiple(command);
                    break;
                case 3:
                    //11
                    //Coprocessor
                    if (command.TestFlag(0, 26) && command.TestFlag(0, 25))
                    {
                        //software interupt.
                    }
                    break;
                default:
                    break;
            }

            instruct.cond = (uint)command.ReadByte(3) >> 4;
            instruct.type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
            instruct.originalBits = (uint)command.ReadWord(0);
            instruct.rn = (uint)((command.ReadWord(0) & 0x000F0000) >> 16);
            instruct.rd = (uint)((command.ReadWord(0) & 0x0000F000) >> 12);
            return instruct; 

        }

        private Instruction parseLoadStoreMultiple(Memory command)
        {
            dataMoveMultiple DMultiple = new dataMoveMultiple();
            DMultiple.regFlags = new bool[16];
            for (byte i = 0; i < 16; ++i)
            {
                DMultiple.regFlags[i] = command.TestFlag(0, i);
            }


            return DMultiple;
            
        }

        public Instruction parseLoadStore(Memory command)
        {
            //PUBWL
            bool R = command.TestFlag(0, 25);
            

            dataMovement dataMoveinstruct = new dataMovement();
            if (!(command.TestFlag(0, 25) && command.TestFlag(0, 4)))
            {
                dataMoveinstruct.R = command.TestFlag(0, 25); ;
                dataMoveinstruct.P = command.TestFlag(0, 24);
                dataMoveinstruct.U = command.TestFlag(0, 23);
                dataMoveinstruct.B = command.TestFlag(0, 22);
                dataMoveinstruct.W = command.TestFlag(0, 21);
                dataMoveinstruct.L = command.TestFlag(0, 20);
                

                dataMoveinstruct.shiftOp = new ShifterOperand(command);

            }




            return dataMoveinstruct;


        }


        public Instruction parseDataManipulation(Memory command) 
        {

            //Get S Byte
            I = command.TestFlag(0, 25);
            S = command.TestFlag(0, 20);
            bit4 = command.TestFlag(0, 4);
            bit7 = command.TestFlag(0, 7);
            dataManipulation dataManinstruct = new dataManipulation();

            if (!(!I && bit4 && bit7))
            {
                //it's data man

                //get OpCode
                uint c = command.ReadWord(0);
                dataManinstruct.opcode = (uint)((c & 0x01E00000) >> 21);
                dataManinstruct.I = I;
                dataManinstruct.shiftOp = new ShifterOperand(command);

                return dataManinstruct;

            }
            //it's a multpiply

            return dataManinstruct;


        }

    }

}
