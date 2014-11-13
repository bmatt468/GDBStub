using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class Branch : Instruction
    {
        public bool L { get; set; }
        //24 bit signed offset (signed_immed_24)
        public int offset { get; set; }
        public bool imm { get; set; }

        /// <summary>
        /// Override parent's ParseCommand method
        /// </summary>
        /// <param name="cmd"></param>
        public override void ParseCommand(Memory cmd)
        {
            if ((0x01 & cmd.ReadByte(3)) == 0x01
            && (0x2F == cmd.ReadByte(2))
            && (0xFF == cmd.ReadByte(1))
            && (0x10 & cmd.ReadByte(0)) == 0x10)
            {
                imm = false;
                this.Rn = (cmd.ReadWord(0) & 0xF);
            }
            else
            {
                this.L = cmd.TestFlag(0, 24);
                bool signed = cmd.TestFlag(0, 23);
                uint tempbits = cmd.ReadWord(0,true);
                tempbits &= 0x00FFFFFF;
                if (signed)
                {
                    tempbits |= 0x3F000000;
                }
                this.offset = ((int)tempbits << 2);
            }
        }

        public override void Run(ref Register[] ra, ref Memory mem)
        {
            uint addressToWhichWeBranch = 0;
            if (imm)
            {
                uint initialAddress = ra[15].ReadWord(0, true);
                if (this.L)
                {
                    //store return address for post-branch
                    ra[14].WriteWord(0, initialAddress - 4);
                }
                addressToWhichWeBranch = (uint)(initialAddress + this.offset);
            }
            else
            {
                addressToWhichWeBranch = (ra[this.Rn].ReadWord(0, true) & 0xFFFFFFFE);
            }

            addressToWhichWeBranch -= 4;
            ra[15].WriteWord(0, addressToWhichWeBranch);
            Logger.Instance.writeLog(string.Format("Assembly: BX 0x{0} : 0x{1}", Convert.ToString(addressToWhichWeBranch + 4,16), Convert.ToString(this.initialBytes, 16)));
        }
    }
}
