using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GDBStub
{
    //the instruction class object
    class Instruction
    {
        uint conditional = 0xe; // 1110  4
        uint type = 0;          // xx   2 determines IPUBWL, or Opcode
        bool I, P, U, B, W, L = false; // xxxxxx 6
        uint opcode = 0;
        bool S = false;
        uint rn = 0;          // reg operand1 4
        
        uint rd = 0;        //reg destination 4
        Operand2 o2;


        public Instruction(Memory command)
        {
            //initialize self based on command

            //decode data
            //Check special cases

            //Get the conditional
            conditional = (uint)command.ReadByte(3)>>4;
            //get the type number
            type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
            //Get immediate value or not.
            I = command.TestFlag(0, 25);

            //get the RN register
            rn = (uint)((command.ReadHalfWord(2) & 0x0F));

            //get the RD reg
            rd = (uint)(((command.ReadHalfWord(0) & 0xF000) >> 12));


            //Debugging purposes

            string binaryParsedCommand = "";
            binaryParsedCommand += Convert.ToString(conditional, 2).PadLeft(4, '0');
            binaryParsedCommand += Convert.ToString(type, 2).PadLeft(2, '0');
            binaryParsedCommand += Convert.ToInt32(I);
            //----


            //switches based on type
            switch (type)
            {   
                case 0:
                    //data manipulation 00

                    parseDataManipulation(command);
                    

                    binaryParsedCommand += Convert.ToString(opcode, 2).PadLeft(4, '0');
                    binaryParsedCommand += Convert.ToInt32(S);
                    break;
                case 1:
                    //ldr/str 01
                    // check the PUBWL 
                    parseLoadStore(command);
                    
                    binaryParsedCommand += Convert.ToInt32(P);
                    binaryParsedCommand += Convert.ToInt32(U);
                    binaryParsedCommand += Convert.ToInt32(B);
                    binaryParsedCommand += Convert.ToInt32(W);
                    binaryParsedCommand += Convert.ToInt32(L);

                    break;
                case 2:
                    //10
                    //load store multiple
                    // branch with link
                    parseLoadStoreMultiple(command);
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




            binaryParsedCommand += Convert.ToString(rn, 2).PadLeft(4, '0');
            binaryParsedCommand += Convert.ToString(rd, 2).PadLeft(4, '0');

            
            Logger.Instance.writeLog(Convert.ToString(command.ReadWord(0), 2));
            Logger.Instance.writeLog(binaryParsedCommand.PadRight(32,'-'));






            // next check instruction typebit 25..27 
            /*
             * Determine category
             * data processing
             * 25 & 4 can distinguish operand 2 types
             * check the PUBWL 
             */
            //


        }

        private void parseLoadStoreMultiple(Memory command)
        {
            ;
        }

        private void parseLoadStore(Memory command)
        {
            //PUBWL
            P = command.TestFlag(0, 24);
            U = command.TestFlag(0, 23);
            B = command.TestFlag(0, 22);
            W = command.TestFlag(0, 21);
            L = command.TestFlag(0, 20);


        }


        void parseDataManipulation(Memory command) 
        {
            //get OpCode
            opcode = (uint)((command.ReadHalfWord(2) & 0x01E0)>>5);
            //Get S Byte
            S = command.TestFlag(0, 20);




        }

    }

}
