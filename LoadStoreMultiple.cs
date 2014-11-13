using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class LoadStoreMultiple : LoadStore
    {
        public bool[] registers { get; set; }

        public override void ParseCommand(Memory command)
        {
            this.registers = new bool[16];
            this.b25 = command.TestFlag(0, 25); ;
            this.P = command.TestFlag(0, 24);
            this.U = command.TestFlag(0, 23);
            this.B = command.TestFlag(0, 22);
            this.W = command.TestFlag(0, 21);
            this.L = command.TestFlag(0, 20);
            for (byte i = 0; i < 16; ++i)
            {
                this.registers[i] = command.TestFlag(0, i);
            }
        }

        public override void Run(ref Register[] ra, ref Memory mem)
        {
            Logger.Instance.writeLog(string.Format("Command Type: Data Move Multiple : 0x{0}", Convert.ToString(this.initialBytes, 16)));

            int RnVal = (int)ra[this.Rn].ReadWord(0, true);
            uint reg = 0;
            string cmd = "";
            string registers = "";

            // Check to see if we traverse up or down in memory
            if (this.U)
            {
                if (this.P)
                {                    
                    RnVal += 4;
                }

                for (int i = 0; i < 16; ++i)
                {
                    if (this.registers[i])
                    {
                        // Check for load / store multiple
                        if (this.L)
                        {
                            ra[i].WriteWord(0, mem.ReadWord((uint)RnVal));
                            cmd = "ldm";
                        }
                        else
                        {
                            mem.WriteWord((uint)RnVal, ra[i].ReadWord(0, true));
                            cmd = "stm";
                        }
                        // continue incrementing
                        RnVal += 4;
                        registers += string.Format(", r{0}", i);
                        reg++;
                    }
                }
            }
            // Pretty much same as above, but we are subtracting this time
            else
            {
                if (this.P)
                {
                    RnVal -= 4;
                }

                for (int i = 15; i > -1; --i)
                {
                    if (this.registers[i])
                    {
                        // Check for load / store multiple
                        if (this.L)
                        {
                            ra[i].WriteWord(0, mem.ReadWord((uint)RnVal));
                            cmd = "ldm";
                        }
                        else
                        {
                            mem.WriteWord((uint)RnVal, ra[i].ReadWord(0, true));
                            cmd = "stm";
                        }
                        RnVal -= 4;
                        registers += string.Format(", r{0}", i);
                        ++reg;
                    }
                }
            }

            // Handles the writeback if needed
            if (this.W)
            {
                uint n = (this.U) ? ra[this.Rn].ReadWord(0, true) + (4 * reg) : ra[this.Rn].ReadWord(0, true) - (4 * reg);                
                ra[this.Rn].WriteWord(0, n);
            }
            Logger.Instance.writeLog(string.Format("Specific Command: {0} r{1}{2}", cmd, this.Rn, registers));
        }
    }    
}
