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
                return;
            }

            if (command is dataMoveMultiple)
            {
                runLoadStoreMultiple((dataMoveMultiple)command);
                return;
            }

            if (command is dataMovement)
            {
                runLoadStore((dataMovement)command);
                return;
            }


            Logger.Instance.writeLog("\n\n");
        }

        private void runLoadStoreMultiple(dataMoveMultiple command)
        {
            Logger.Instance.writeLog(string.Format("CMD: Data Move Multiple : 0x{0}", Convert.ToString(command.originalBits,16)));
            if (command.cond == 0XE)
            {
                int RnVal = (int)reg[command.rn].ReadWord(0);
                int incrementer = 4;
                uint numReg = 0;
                if (command.U)
                {
                    incrementer = 4;
                }
                else
                {
                    incrementer = -4;
                }

                if (command.P)
                {
                    //RnVal excluded
                    RnVal += incrementer;
                }
                string Scom = "";
                string registers = "";
                for (int i = 0; i < 16; ++i)
                {
                    if (command.regFlags[i])
                    {
                        if (command.L)
                        {
                            RAM.WriteWord((uint)RnVal, reg[i].ReadWord(0));
                            Scom = "ldm";
                        }
                        else
                        {
                            reg[i].WriteWord(0, RAM.ReadWord((uint)RnVal));
                            Scom = "stm";
                        }
                        RnVal += incrementer;
                        registers += string.Format("r{0}, ", i);
                        ++numReg;
                    }
                }

                if (command.W)
                {
                    uint n;
                    if (command.U)
                    {
                        n = reg[command.rn].ReadWord(0) + (4 * numReg);
                    }
                    else
                    {
                        n = reg[command.rn].ReadWord(0) - (4 * numReg);
                    }
                    reg[command.rn].WriteWord(0, n);
                }
                
            Logger.Instance.writeLog(string.Format("CMD: r{0} {1}, {2}",Scom, command.rn, registers));
            }//if E


        }//LoadMultStoreMult

        private void runLoadStore(dataMovement command)
        {
            Logger.Instance.writeLog(string.Format("CMD: Data Movement : 0x{0}", Convert.ToString(command.originalBits, 16)));
            if (command.cond == 0xE)
            {
                switch (command.L)
                {
                    case true:
                        //load
                        this.load(command);
                        break;
                    case false:
                        //store
                        this.store(command);
                        break;
                    default:
                        break;
                }
            }
        }

        private void store(dataMovement command)
        {
            //from register info to memory!!!
            // --->
            uint RdValue = reg[command.rd].ReadWord(0);
            uint RnValue = reg[command.rn].ReadWord(0);
            uint RmValue = reg[command.rm].ReadWord(0);
            
            command.shiftOp = loadStoreShift(command.R, command.shiftOp, RmValue);
            //addressing mode
            uint addr = figureOutAddressing(command);

            if(command.B)
            {
                byte inpu = reg[command.rd].ReadByte(0); 
                RAM.WriteByte(addr, inpu);
            }
            else
            {
                RAM.WriteWord(addr, RdValue);
            }


            Logger.Instance.writeLog(string.Format("CMD: str {0}, 0x{1} : ", RdValue, 
                Convert.ToString(addr, 16), Convert.ToString(command.originalBits, 16)));
            
        }

        private void load(dataMovement command)
        {
            //from memory info to register!!!
            // <---
            uint RdValue = reg[command.rd].ReadWord(0);
            uint RnValue = reg[command.rn].ReadWord(0);
            uint RmValue = reg[command.rm].ReadWord(0);

            command.shiftOp = loadStoreShift(command.R, command.shiftOp, RmValue);
            //addressing mode

            
            uint addr = figureOutAddressing(command);
           
            if (command.B)
            {
                byte inpu = RAM.ReadByte(addr);
                //clear it out first
                reg[command.rd].WriteWord(0, 0);

                reg[command.rd].WriteByte(0, inpu);
                
            }
            else
            {
                uint inpu = RAM.ReadWord(addr);
                reg[command.rd].WriteWord(0, inpu);
            }


            Logger.Instance.writeLog(string.Format("CMD: ldr {0}, 0x{1}", RdValue, Convert.ToString(addr, 16)));
            
        }

        private uint figureOutAddressing(dataMovement command)
        {

            uint RdValue = reg[command.rd].ReadWord(0);
            uint RnValue = reg[command.rn].ReadWord(0);
            uint addr = 0;
            if (command.P)
            {
                if (command.U)
                {
                    addr = RnValue + command.shiftOp.offset;
                }
                else
                {
                    //will be subtraction later
                    addr = RnValue + command.shiftOp.offset;
                }

                //offset addressing
                if (command.W)
                {
                    //pre-indexed
                    reg[command.rn].WriteWord(0, addr);
                }

            }
            else
            {
                //post-indexed addressing
                addr = RnValue;
                if (command.U)
                {
                    reg[command.rn].WriteWord(0, RnValue + command.shiftOp.offset);
                }
                else
                {
                    reg[command.rn].WriteWord(0, RnValue - command.shiftOp.offset);
                }
            }
            return addr;
        }


        public ShifterOperand loadStoreShift(bool R, ShifterOperand shiftOp, uint RmValue)
        {
            if (R)
            {
                //it's a register
                shiftOp = figureOutShift(!R, shiftOp, RmValue);
            }
            else
            {
                //it's an immediate 12 bit value
                shiftOp.offset = shiftOp.immed_12;
            }

            return shiftOp;
        }

        public ShifterOperand figureOutShift(bool I, ShifterOperand shiftOp, uint RmVal)
        {
            if (!I)
            {
                //it's a register!
                if (shiftOp.bit4 && !shiftOp.bit7)
                {
                    //shifted by a register!
                    shiftOp.shiftRM(RmVal, reg[shiftOp.Rs].ReadWord(0));

                }
                else
                {
                    //shifted by an immediate value!
                    shiftOp.shiftRM(RmVal, shiftOp.shift_imm);
                }
            }
            return shiftOp;
        }



        private void mov(dataManipulation dman)
        {
            
            dman.shiftOp = figureOutShift(dman.I, dman.shiftOp, reg[dman.shiftOp.Rm].ReadWord(0));
           
            reg[dman.rd].WriteWord(0, dman.shiftOp.offset);
            Logger.Instance.writeLog(String.Format("CMD: mov {0},{1} : 0x{2}",
                dman.rd,dman.shiftOp.offset, Convert.ToString(dman.originalBits,16)));
        }




        private void sub(dataManipulation dman)
        {
            dman.shiftOp = figureOutShift(dman.I, dman.shiftOp, reg[dman.shiftOp.Rm].ReadWord(0));
            uint RnValue = reg[dman.rn].ReadWord(0);
            reg[dman.rd].WriteWord(0, (RnValue - dman.shiftOp.offset));
            Logger.Instance.writeLog(String.Format("CMD: sub {0},{1},{2} : 0x{3}",
                dman.rd, dman.rn, dman.shiftOp.offset, Convert.ToString(dman.originalBits, 16)));

        }

        private void runOpcode(dataManipulation dman)
        {
            Logger.Instance.writeLog(string.Format("CMD: Data Manipulation 0x{0}", Convert.ToString(dman.originalBits, 16)));

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



    }
}
