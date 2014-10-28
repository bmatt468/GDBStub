using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class DataProcessing : Instruction
    {
        public uint opcode { get; set; }
        public bool I { get; set; }
        public ShifterOperand shiftOp { get; set; }
        public bool bit4 { get; set; }
        public bool bit7 { get; set; }

        public override void ParseCommand(Memory command)
        {
            //Get S Byte
            this.I = command.TestFlag(0, 25);
            this.S = command.TestFlag(0, 20);
            this.bit4 = command.TestFlag(0, 4);
            this.bit7 = command.TestFlag(0, 7);
            //dataManipulation dataManinstruct = new dataManipulation();
            this.shiftOp = new ShifterOperand(command);

            if (!(!I && bit4 && bit7))
            {
                //it's data man

                //get OpCode
                uint c = command.ReadWord(0, true);
                this.opcode = (uint)((c & 0x01E00000) >> 21);
                return;

            }
            else
            {
                //it's a multpiply
                this.opcode = 0x1F;
                return;
            }
        }

        public override void Run(ref Register[] reg, ref Memory RAM)
        {
            Logger.Instance.writeLog(string.Format("CMD: Data Manipulation 0x{0}", Convert.ToString(this.initial, 16)));

            switch (this.opcode)
            {
                case 0:
                    //and
                    this.and(ref reg, ref RAM);
                    break;
                case 1: //EOR
                    this.eor(ref reg, ref RAM);
                    break;
                case 2: //SUb
                    this.sub(ref reg, ref RAM);
                    break;
                case 3: //RSB
                    this.rsb(ref reg, ref RAM);
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
                    this.cmp(ref reg, ref RAM);
                    break;
                case 11: //cmn
                    break;
                case 12: //oor
                    this.oor(ref reg, ref RAM);
                    break;
                case 13: //mov
                    this.mov(ref reg, ref RAM);
                    break;
                case 14: //bic
                    this.bic(ref reg, ref RAM);
                    break;
                case 15: //mvn
                    this.mvn(ref reg, ref RAM);
                    break;
                case 0x1F:
                    this.mul(ref reg, ref RAM);
                    break;

                default:
                    break;
            }
        }

        private void mul(ref Register[] reg, ref Memory RAM)
        {
            uint RmValue = reg[this.shiftOp.Rm].ReadWord(0, true);
            uint RsValue = reg[this.shiftOp.Rs].ReadWord(0, true);
            uint product = RmValue * RsValue;
            if (product > 0xFFFFFFFF) { Logger.Instance.writeLog("ERR: Multiply to large"); }
            reg[this.Rn].WriteWord(0, product);
            Logger.Instance.writeLog(String.Format("CMD: MUL R{0},R{1},R{2} : 0x{3}",
                this.Rd, this.shiftOp.Rm, this.shiftOp.Rs, Convert.ToString(this.initial, 16)));

        }

        private void bic(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            uint RnValue = reg[this.Rn].ReadWord(0, true);

            reg[this.Rd].WriteWord(0, (RnValue & (~this.shiftOp.offset)));

            Logger.Instance.writeLog(String.Format("CMD: BIC R{0},R{1},{2} : 0x{3}",
                this.Rd, this.Rn, this.shiftOp.offset, Convert.ToString(this.initial, 16)));
        }


        // This can be refactored
        //maybe pass in two values in the order you need them and an operator....
        private void eor(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            uint RnValue = reg[this.Rn].ReadWord(0, true);
            reg[this.Rd].WriteWord(0, (RnValue ^ this.shiftOp.offset));
            Logger.Instance.writeLog(String.Format("CMD: EOR R{0},R{1},{2} : 0x{3}",
                this.Rd, this.Rn, this.shiftOp.offset, Convert.ToString(this.initial, 16)));
        }

        private void oor(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            uint RnValue = reg[this.Rn].ReadWord(0, true);
            reg[this.Rd].WriteWord(0, (RnValue | this.shiftOp.offset));
            Logger.Instance.writeLog(String.Format("CMD: OOR R{0},R{1},{2} : 0x{3}",
                this.Rd, this.Rn, this.shiftOp.offset, Convert.ToString(this.initial, 16)));
        }

        private void and(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            uint RnValue = reg[this.Rn].ReadWord(0, true);
            reg[this.Rd].WriteWord(0, (RnValue & this.shiftOp.offset));
            Logger.Instance.writeLog(String.Format("CMD: AND R{0},R{1},{2} : 0x{3}",
                this.Rd, this.Rn, this.shiftOp.offset, Convert.ToString(this.initial, 16)));
        }

        private void rsb(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            uint RnValue = reg[this.Rn].ReadWord(0, true);
            reg[this.Rd].WriteWord(0, (this.shiftOp.offset - RnValue));
            Logger.Instance.writeLog(String.Format("CMD: rsb R{0},R{1},{2} : 0x{3}",
                this.Rd, this.Rn, this.shiftOp.offset, Convert.ToString(this.initial, 16)));
        }

        private void mvn(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            reg[this.Rd].WriteWord(0, ~this.shiftOp.offset);
            Logger.Instance.writeLog(String.Format("CMD: mvn R{0},{1} : 0x{2}",
                this.Rd, this.shiftOp.offset, Convert.ToString(this.initial, 16)));
        }

        private void cmp(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            uint RnVal = reg[this.Rn].ReadWord(0, true);
            uint cmpVal = RnVal - this.shiftOp.offset;
            Memory alu = new Memory(4);
            alu.WriteWord(0, cmpVal);
            //set N flag
            N = alu.TestFlag(0, 31);
            Z = alu.ReadWord(0, true) == 0;
            Logger.Instance.writeLog("Fix compare C and V flags");
            C = false;
            F = false;
            Logger.Instance.writeLog(String.Format("CMD: cmp R{0}, {1} : 0x{2}",
                this.Rn, this.shiftOp.offset, Convert.ToString(this.initial, 16)));
        }

        public void add(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            uint RnValue = reg[this.Rn].ReadWord(0, true);
            reg[this.Rd].WriteWord(0, (RnValue + this.shiftOp.offset));
            Logger.Instance.writeLog(String.Format("CMD: ADD R{0},R{1},{2} : 0x{3}",
                this.Rd, this.Rn, this.shiftOp.offset, Convert.ToString(this.initial, 16)));
        }

        public void sub(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            uint RnValue = reg[this.Rn].ReadWord(0, true);
            reg[this.Rd].WriteWord(0, (RnValue - this.shiftOp.offset));
            Logger.Instance.writeLog(String.Format("CMD: sub R{0},R{1},{2} : 0x{3}",
                this.Rd, this.Rn, this.shiftOp.offset, Convert.ToString(this.initial, 16)));

        }

        private void mov(ref Register[] reg, ref Memory RAM)
        {
            this.shiftOp = Shift(this.I, this.shiftOp, reg[this.shiftOp.Rm].ReadWord(0, true), reg);
            reg[this.Rd].WriteWord(0, this.shiftOp.offset);
            Logger.Instance.writeLog(String.Format("CMD: mov R{0},{1} : 0x{2}",
                this.Rd, this.shiftOp.offset, Convert.ToString(this.initial, 16)));
        }
    }
}
