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

        public virtual void run(ref Register[] reg, ref Memory RAM)
        {
            Logger.Instance.writeLog("CMD: UNDISCOVERED");
        }


        public ShifterOperand figureOutShift(bool I, ShifterOperand shiftOp, uint RmVal, Register[] reg)
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

        public override void run(ref Register[] reg, ref Memory RAM)
        {
            //base.run(ref reg, ref RAM);
            Logger.Instance.writeLog(string.Format("CMD: Data Movement : 0x{0}", Convert.ToString(this.originalBits, 16)));
            if (this.cond == 0xE)
            {
                //from register info to memory!!!
                // --->
                uint RdValue = reg[this.rd].ReadWord(0);
                uint RnValue = reg[this.rn].ReadWord(0);
                uint RmValue = reg[this.rm].ReadWord(0);

                this.shiftOp = loadStoreShift(this.R, this.shiftOp, RmValue, reg);
                //addressing mode
                uint addr = figureOutAddressing(ref reg);
                string cmd = "";
                if (this.L)
                {
                    cmd = "ldr";
                    if (this.B)
                    {
                        byte inpu = RAM.ReadByte(addr);
                        //clear it out first
                        reg[this.rd].WriteWord(0, 0);

                        reg[this.rd].WriteByte(0, inpu);

                    }
                    else
                    {
                        uint inpu = RAM.ReadWord(addr);
                        reg[this.rd].WriteWord(0, inpu);
                    }
                }
                else
                {
                    cmd = "str";
                    if (this.B)
                    {
                        byte inpu = reg[this.rd].ReadByte(0);
                        RAM.WriteByte(addr, inpu);
                    }
                    else
                    {
                        RAM.WriteWord(addr, RdValue);
                    }
                }

                Logger.Instance.writeLog(string.Format("CMD: {0} {1}, 0x{2} : 0x{3} ", cmd, RdValue,
                    Convert.ToString(addr, 16), Convert.ToString(this.originalBits, 16)));
            }


        }



        public ShifterOperand loadStoreShift(bool R, ShifterOperand shiftOp, uint RmValue, Register[] reg)
        {
            if (R)
            {
                //it's a register
                shiftOp = figureOutShift(!R, shiftOp, RmValue, reg);
            }
            else
            {
                //it's an immediate 12 bit value
                shiftOp.offset = shiftOp.immed_12;
            }

            return shiftOp;
        }



        private uint figureOutAddressing(ref Register[] reg)
        {

            uint RdValue = reg[this.rd].ReadWord(0);
            uint RnValue = reg[this.rn].ReadWord(0);
            uint addr = 0;
            if (this.P)
            {
                if (this.U)
                {
                    addr = RnValue + this.shiftOp.offset;
                }
                else
                {
                    //will be subtraction later
                    addr = RnValue + this.shiftOp.offset;
                }

                //offset addressing
                if (this.W)
                {
                    //pre-indexed
                    reg[this.rn].WriteWord(0, addr);
                }

            }
            else
            {
                //post-indexed addressing
                addr = RnValue;
                if (this.U)
                {
                    reg[this.rn].WriteWord(0, RnValue + this.shiftOp.offset);
                }
                else
                {
                    reg[this.rn].WriteWord(0, RnValue - this.shiftOp.offset);
                }
            }
            return addr;
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


        public override void run(ref Register[] reg, ref Memory RAM)
        {
            Logger.Instance.writeLog(string.Format("CMD: Data Manipulation 0x{0}", Convert.ToString(this.originalBits, 16)));

            //if always DO IT!
            if (this.cond == 0xE)
            {
                switch (this.opcode)
                {
                    case 0:
                        //and
                        break;
                    case 1: //EOR
                        break;
                    case 2: //SUb
                        this.sub(ref reg, ref RAM);
                        break;
                    case 3: //RSB
                        break;
                    case 4: //ADD
                        this.add(ref reg, ref RAM);
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
                        this.mov(ref reg, ref RAM);
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


        public void add(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = figureOutShift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0), reg);
            uint RnValue = reg[this.rn].ReadWord(0);
            reg[this.rd].WriteWord(0, (RnValue + this.shiftOp.offset));
            Logger.Instance.writeLog(String.Format("CMD: ADD {0},{1},{2} : 0x{3}",
                this.rd, this.rn, this.shiftOp.offset, Convert.ToString(this.originalBits, 16)));
        }


        public void sub(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = figureOutShift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0), reg);
            uint RnValue = reg[this.rn].ReadWord(0);
            reg[this.rd].WriteWord(0, (RnValue - this.shiftOp.offset));
            Logger.Instance.writeLog(String.Format("CMD: sub {0},{1},{2} : 0x{3}",
                this.rd, this.rn, this.shiftOp.offset, Convert.ToString(this.originalBits, 16)));

        }





        private void mov(ref Register[] reg, ref Memory RAM)
        {

            this.shiftOp = figureOutShift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0), reg);

            reg[this.rd].WriteWord(0, this.shiftOp.offset);
            Logger.Instance.writeLog(String.Format("CMD: mov {0},{1} : 0x{2}",
                this.rd, this.shiftOp.offset, Convert.ToString(this.originalBits, 16)));
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



        public override void run(ref Register[] reg, ref Memory RAM)
        {
            if (this.cond == 0xE)
            {
                uint curAddr = reg[15].ReadWord(0);

                if (this.LN)
                {
                    //store a return address
                    reg[14].WriteWord(0, curAddr);
                }
                uint newAddress = (uint)(curAddr + this.offset);
                reg[15].WriteWord(0, newAddress);

                Logger.Instance.writeLog(string.Format("CMD: BX 0x{0} : 0x{1}", newAddress, Convert.ToString(this.originalBits, 16)));
            }
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


        public override void run(ref Register[] reg, ref Memory RAM)
        {
            Logger.Instance.writeLog(string.Format("CMD: Data Move Multiple : 0x{0}", Convert.ToString(this.originalBits, 16)));
            if (this.cond == 0XE)
            {
                int RnVal = (int)reg[this.rn].ReadWord(0);
                int incrementer = 4;
                uint numReg = 0;
                string Scom = "";
                string registers = "";
    

                if (this.U)
                {
                    //go up in memory!
                    if (this.P)
                    {
                        //RnVal excluded
                        RnVal += 4;
                    }


                    for (int i = 0; i < 16; ++i)
                    {
                        if (this.regFlags[i])
                        {
                            if (this.L)
                            {
                                reg[i].WriteWord(0, RAM.ReadWord((uint)RnVal));

                                Scom = "ldm";
                            }
                            else
                            {
                                RAM.WriteWord((uint)RnVal, reg[i].ReadWord(0));

                                Scom = "stm";
                            }
                            RnVal += 4;
                            registers += string.Format("r{0}, ", i);
                            ++numReg;
                        }
                    }



                }
                else
                {
                    //go down in memory
                    if (this.P)
                    {
                        //RnVal excluded
                        RnVal -= 4;
                    }

                    for (int i = 15; i > -1; --i)
                    {
                        if (this.regFlags[i])
                        {
                            if (this.L)
                            {
                                reg[i].WriteWord(0, RAM.ReadWord((uint)RnVal));

                                Scom = "ldm";
                            }
                            else
                            {
                                RAM.WriteWord((uint)RnVal, reg[i].ReadWord(0));

                                Scom = "stm";
                            }
                            RnVal -= 4;
                            registers += string.Format("r{0}, ", i);
                            ++numReg;
                        }
                    }


                }



                if (this.W)
                {
                    uint n;
                    if (this.U)
                    {
                        n = reg[this.rn].ReadWord(0) + (4 * numReg);
                    }
                    else
                    {
                        n = reg[this.rn].ReadWord(0) - (4 * numReg);
                    }
                    reg[this.rn].WriteWord(0, n);
                }

                Logger.Instance.writeLog(string.Format("CMD: {0} r{1}, {2}", Scom, this.rn, registers));
            }//if E


        }//LoadMultStoreMult
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
