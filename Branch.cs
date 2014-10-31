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

        /// <summary>
        /// Override parent's ParseCommand method
        /// </summary>
        /// <param name="cmd"></param>
        public override void ParseCommand(Memory cmd)
        {
            this.L = cmd.TestFlag(0, 24);
            this.offset = ((int)cmd.ReadWord(0, true) & 0x00FFFFFF) << 2;
        }

        public override void Run(Register[] ra, Memory mem)
        {
            uint initialAddress = ra[15].ReadWord(0, true);
            if (this.L)
            {
                //store return address for post-branch
                ra[14].WriteWord(0, initialAddress);
            }
            uint branchAddress = (uint)(initialAddress + this.offset);
            ra[15].WriteWord(0, branchAddress);
            Logger.Instance.writeLog(string.Format("Assembly: BX 0x{0} : 0x{1}", branchAddress, Convert.ToString(this.initialBytes, 16)));
        }
    }
}
