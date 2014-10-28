using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GDBStub
{    
    class InstructionParser
    {
        uint type = 0;          // xx   2 determines IPUBWL, or Opcode
        Instruction i = new Instruction();

        public Instruction parse(Memory command)
        {
            // get type
            this.type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
            
            switch (this.type)
            {
                case 0:
                    //data manipulation 00
                    i = new DataProcessing();
                    break;
                case 1:
                    //ldr/str 01
                    // check the PUBWL 
                    if (command.TestFlag(0, 24) || !command.TestFlag(0, 21))
                    {
                        i = new LoadStore();
                    }
                    break;
                case 2:
                    //10
                    if (!command.TestFlag(0, 25))
                    {
                        //load store multiple
                        i = new LoadStoreMultiple();
                    }
                    else
                    {
                        if (command.TestFlag(0, 27) && !command.TestFlag(0, 26) && command.TestFlag(0, 25))
                        {
                            //branch command.
                            i = new Branch();
                        }
                    }
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

            i.ParseCommand(command);
            i.Cond = (uint)command.ReadByte(3) >> 4;
            i.Type = (uint)((command.ReadByte(3) & 0x0c) >> 2);
            i.initial = (uint)command.ReadWord(0);
            i.Rn = (uint)((command.ReadWord(0) & 0x000F0000) >> 16);
            i.Rd = (uint)((command.ReadWord(0) & 0x0000F000) >> 12);
            return i;
        }
    }
}
