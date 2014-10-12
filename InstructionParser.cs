﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GDBStub
{
    //the instruction class object
    class InstructionParser
    {

        uint type = 0;          // xx   2 determines IPUBWL, or Opcode


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

                    instruct = new dataManipulation();
                    //instruct = parseDataManipulation(command);


                    break;
                case 1:
                    //ldr/str 01
                    // check the PUBWL 
                    if (command.TestFlag(0, 24) || !command.TestFlag(0, 21))
                    {
                        instruct = new dataMovement();

                        //instruct = parseLoadStore(command);
                        //instruct.rm = (uint)((command.ReadWord(0) & 0xF));
   

                    }
                    else
                    {
                        //unpredictable
                    }


                    break;
                case 2:
                    //10
                    if (!command.TestFlag(0, 25))
                    {
                        //load store multiple
                        instruct = new dataMoveMultiple();
                        //instruct = parseLoadStoreMultiple(command);
                    }
                    else
                    {
                        if (command.TestFlag(0, 25) && command.TestFlag(0, 27) && !command.TestFlag(0, 26))
                        {
                            //branch command.
                            instruct = new Branch();
                            //instruct = parseBranch(command);
                        }
                    }
                    // branch with link
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

            instruct.parse(command);
            instruct.cond = (uint)command.ReadByte(3) >> 4;
            instruct.type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
            instruct.originalBits = (uint)command.ReadWord(0);
            instruct.rn = (uint)((command.ReadWord(0) & 0x000F0000) >> 16);
            instruct.rd = (uint)((command.ReadWord(0) & 0x0000F000) >> 12);
            return instruct; 

        }




    }

}
