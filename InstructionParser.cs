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
            instruct.cond = (uint)command.ReadByte(3) >> 4;

            //get the type number
            instruct.type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
            //Get immediate value or not.
            //get the RN register
            


            //switches based on type
            switch (instruct.type)
            {
                case 0:
                    //data manipulation 00


                    instruct = parseDataManipulation(command);
                    instruct.rn = (uint)((command.ReadHalfWord(2) & 0x0F));
                    instruct.rd = (uint)(((command.ReadHalfWord(0) & 0xF000) >> 12));
                    instruct.cond = (uint)command.ReadByte(3) >> 4;
                    instruct.type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
                    instruct.originalBits = (uint)command.ReadWord(0);

                    break;
                case 1:
                    //ldr/str 01
                    // check the PUBWL 

                    parseLoadStore(command);

                    break;
                case 2:
                    //10
                    //load store multiple
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

        
            return instruct; 

        }

        private void parseLoadStoreMultiple(Memory command)
        {

            ;
        }

        private void parseLoadStore(Memory command)
        {
            //PUBWL
            bool R = command.TestFlag(0, 25);
            P = command.TestFlag(0, 24);
            U = command.TestFlag(0, 23);
            B = command.TestFlag(0, 22);
            W = command.TestFlag(0, 21);
            L = command.TestFlag(0, 20);


        }


        Instruction parseDataManipulation(Memory command) 
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
