using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class Branch : Instruction
    {
        public bool LN { get; set; }
        //23bit long offset
        public int offset { get; set; }

        public override void ParseCommand(Memory command)
        {
            this.LN = command.TestFlag(0, 24);
            this.offset = ((int)command.ReadWord(0, true) & 0x00FFFFFF) << 2;
        }

        public override void Run(ref Register[] reg, ref Memory RAM)
        {
            uint curAddr = reg[15].ReadWord(0, true);
            if (this.LN)
            {
                //store a return address
                reg[14].WriteWord(0, curAddr);
            }
            uint newAddress = (uint)(curAddr + this.offset);
            reg[15].WriteWord(0, newAddress);
            Logger.Instance.writeLog(string.Format("CMD: BX 0x{0} : 0x{1}", newAddress, Convert.ToString(this.initial, 16)));
        }
    }
}
