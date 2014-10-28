using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class LoadStore : Instruction
    {
        public bool R { get; set; }
        public bool P { get; set; }
        public bool U { get; set; }
        public bool B { get; set; }
        public bool W { get; set; }
        public bool L { get; set; }

        public ShifterOperand shiftOp { get; set; }

        public override void ParseCommand(Memory command)
        {
            //PUBWL
            bool R = command.TestFlag(0, 25);
            Rm = (command.ReadWord(0) & 0x0000000F);
            this.shiftOp = new ShifterOperand(command);

            if (!(command.TestFlag(0, 25) && command.TestFlag(0, 4)))
            {
                this.R = command.TestFlag(0, 25);
                this.P = command.TestFlag(0, 24);
                this.U = command.TestFlag(0, 23);
                this.B = command.TestFlag(0, 22);
                this.W = command.TestFlag(0, 21);
                this.L = command.TestFlag(0, 20);
            }
        }

        public override void Run(ref Register[] reg, ref Memory RAM)
        {
            //base.run(ref reg, ref RAM);
            Logger.Instance.writeLog(string.Format("CMD: Data Movement : 0x{0}", Convert.ToString(this.initial, 16)));

            //from register info to memory!!!
            // --->
            uint RdValue = reg[this.Rd].ReadWord(0, true);
            uint RnValue = reg[this.Rn].ReadWord(0, true);
            uint RmValue = reg[this.Rm].ReadWord(0, true);

            this.shiftOp = loadStoreShift(this.R, this.shiftOp, RmValue, reg);
            //addressing mode
            uint addr = AddressingMode(ref reg);
            string cmd = "";
            if (this.L)
            {
                cmd = "ldr";
                if (this.B)
                {
                    byte inpu = RAM.ReadByte(addr);
                    //clear it out first
                    reg[this.Rd].WriteWord(0, 0);

                    reg[this.Rd].WriteByte(0, inpu);

                }
                else
                {
                    uint inpu = RAM.ReadWord(addr);
                    reg[this.Rd].WriteWord(0, inpu);
                }
            }
            else
            {
                cmd = "str";
                if (this.B)
                {
                    byte inpu = reg[this.Rd].ReadByte(0);
                    RAM.WriteByte(addr, inpu);
                }
                else
                {
                    RAM.WriteWord(addr, RdValue);
                }
            }

            Logger.Instance.writeLog(string.Format("CMD: {0} {1}, 0x{2} : 0x{3} ", cmd, RdValue,
                Convert.ToString(addr, 16), Convert.ToString(this.initial, 16)));
        }

        public ShifterOperand loadStoreShift(bool R, ShifterOperand shiftOp, uint RmValue, Register[] reg)
        {
            if (R)
            {
                //it's a register
                shiftOp = Shift(!R, shiftOp, RmValue, reg);
            }
            else
            {
                //it's an immediate 12 bit value
                shiftOp.offset = shiftOp.immed_12;
            }

            return shiftOp;
        }

        private uint AddressingMode(ref Register[] reg)
        {
            uint RdValue = reg[this.Rd].ReadWord(0, true);
            uint RnValue = reg[this.Rn].ReadWord(0, true);
            uint addr = 0;
            if (this.P)
            {
                if (this.U)
                {
                    addr = RnValue + this.shiftOp.offset;
                }
                else
                {
                    addr = RnValue - this.shiftOp.offset;
                }

                //offset addressing
                if (this.W)
                {
                    //pre-indexed
                    reg[this.Rn].WriteWord(0, addr);
                }
            }
            else
            {
                //post-indexed addressing
                addr = RnValue;
                if (this.U)
                {
                    reg[this.Rn].WriteWord(0, RnValue + this.shiftOp.offset);
                }
                else
                {
                    reg[this.Rn].WriteWord(0, RnValue - this.shiftOp.offset);
                }
            }
            return addr;
        }
    }

    class LoadStoreMultiple : LoadStore
    {
        public bool[] regFlags { get; set; }

        public override void ParseCommand(Memory command)
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

        public override void Run(ref Register[] reg, ref Memory RAM)
        {
            Logger.Instance.writeLog(string.Format("CMD: Data Move Multiple : 0x{0}", Convert.ToString(this.initial, 16)));

            int RnVal = (int)reg[this.Rn].ReadWord(0, true);
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
                            RAM.WriteWord((uint)RnVal, reg[i].ReadWord(0, true));

                            Scom = "stm";
                        }
                        RnVal += 4;
                        registers += string.Format(", r{0}", i);
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
                            RAM.WriteWord((uint)RnVal, reg[i].ReadWord(0, true));
                            Scom = "stm";
                        }
                        RnVal -= 4;
                        registers += string.Format(", r{0}", i);
                        ++numReg;
                    }
                }
            }

            if (this.W)
            {
                uint n;
                if (this.U)
                {
                    n = reg[this.Rn].ReadWord(0, true) + (4 * numReg);
                }
                else
                {
                    n = reg[this.Rn].ReadWord(0, true) - (4 * numReg);
                }
                reg[this.Rn].WriteWord(0, n);
            }

            Logger.Instance.writeLog(string.Format("CMD: {0} r{1}{2}", Scom, this.Rn, registers));
        }
    }
}
