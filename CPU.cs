using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;


namespace GDBStub
{

    /*
     * CPU class that represents the CPU. This should have methods named 
     * fetch, decode, and execute. The CPU class should have instance variables 
     * that hold references to the registers and RAM objects.
     * 
     */
    class CPU
    {
        private Memory RAM;
        private Register[] reg;

        //variables that hold references to the regs and RAM

        //CPU instantiation

        public CPU(ref Memory RAM, ref Register[] reg)
            {
                this.RAM = RAM;
                this.reg = reg;
            }


        //fetches data from RAM 
        public Memory fetch()
        {
            Memory cmd = new Memory(4);
            cmd.WriteWord(0, RAM.ReadWord(reg[15].ReadWord(0)));
            Logger.Instance.writeLog(String.Format("CMD: 0x{0}", Convert.ToString(cmd.ReadWord(0), 16)));

            return cmd;
           

        }

        //decodes the int into a command.  like mov r0, r1
        public Instruction decode(Memory data)
        {
            //data.WriteWord(0,0xe3a02030);//e3a02030 mov r2, #48
            InstructionParser parser = new InstructionParser();
            Instruction inst = parser.parse(data);


            
            return inst;
        }

        //executes the actual data by movine registers and stuff
        public void execute(Instruction command)
        {
        //won't let me do a switch statement so bare with the ifs....

            if (command is dataManipulation)
            {
                runOpcode((dataManipulation)command);

            }
            else
            {
                if (command is dataMovement)
                {

                }
            }

        }


        private void runOpcode(dataManipulation dman)
        {
            Logger.Instance.writeLog("CMD: Data Manipulation");

            //if always DO IT!
            if (dman.cond == 0xE)
            {
                switch (dman.opcode)
                {
                    case 0:
                        //and
                        break;
                    case 1: //EOR
                        break;
                    case 2: //SUb
                        this.sub(dman);
                        break;
                    case 3: //RSB
                        break;
                    case 4: //ADD
                        break;
                    case 5: //ADC
                        break;
                    case 6: //SBC
                        break;
                    case 7: //RSC
                        break;
                    case 8: //TST
                        break;
                    case 9: //teq
                        break;
                    case 10: //cmp
                        break;
                    case 11: //cmn
                        break;
                    case 12: //oor
                        break;
                    case 13: //mov
                        this.mov(dman);
                        break;
                    case 14: //bic
                        break;
                    case 15: //movn
                        break;

                    default:
                        //something bad
                        break;
                }//switc
            }//if
        }

        private void setshiftOffset(ref dataManipulation dman)
        {
            uint RmVal = reg[dman.shiftOp.Rm].ReadWord(0);
            if (!dman.I)
            {
                //it's a register!
                if (dman.shiftOp.bit4 && !dman.shiftOp.bit7)
                {
                    //shifted by a register!
                    dman.shiftOp.shiftRM(RmVal, reg[dman.shiftOp.Rs].ReadWord(0));

                }
                else
                {
                    //shifted by an immediate value!
                    dman.shiftOp.shiftRM(RmVal, dman.shiftOp.shift_imm);
                }
            }
        }


        private void mov(dataManipulation dman)
        {
            
            this.setshiftOffset(ref dman);
            
            
            reg[dman.rd].WriteWord(0, dman.shiftOp.offset);
            Logger.Instance.writeLog(String.Format("CMD: mov {0},{1} : 0x{2}",
                dman.rd,dman.shiftOp.offset, Convert.ToString(dman.originalBits,16)));
        }




        private void sub(dataManipulation dman)
        {
            this.setshiftOffset(ref dman);
            uint RnValue = reg[dman.rn].ReadWord(0);
            reg[dman.rd].WriteWord(0, (RnValue - dman.shiftOp.offset));
            Logger.Instance.writeLog(String.Format("CMD: sub {0},{1},{2} : 0x{3}",
                dman.rd, dman.rn, dman.shiftOp.offset, Convert.ToString(dman.originalBits, 16)));

        }

    }
}
