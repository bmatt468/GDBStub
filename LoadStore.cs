using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class LoadStore : Instruction
    {
        // declare the magic bits
        public bool b4 { get; set; }
        public bool b25 { get; set; }
        public bool P { get; set; }
        public bool U { get; set; }
        public bool B { get; set; }
        public bool W { get; set; }
        public bool L { get; set; }

        public Operand2 shifter { get; set; }

        /// <summary>
        /// Overwrites parent's ParseCommand Method
        /// </summary>
        /// <param name="cmd"></param>
        public override void ParseCommand(Memory cmd)
        {
            //PUBWL
            b25 = cmd.TestFlag(0, 25);
            b4 = cmd.TestFlag(0, 4);
            Rm = (cmd.ReadWord(0) & 0x0000000F);
            this.shifter = new Operand2(cmd);

            if (!(b25 && b4))
            {
                this.b25 = cmd.TestFlag(0, 25);
                this.P = cmd.TestFlag(0, 24);
                this.U = cmd.TestFlag(0, 23);
                this.B = cmd.TestFlag(0, 22);
                this.W = cmd.TestFlag(0, 21);
                this.L = cmd.TestFlag(0, 20);
            }
        }

        /// <summary>
        /// The run method for Load Store.
        /// Overwrites parent's method
        /// </summary>
        /// <param name="ra"></param>
        /// <param name="mem"></param>
        public override void Run(Register[] ra, Memory mem)
        {
            Logger.Instance.writeLog(string.Format("Command Type: Data Movement : 0x{0}", Convert.ToString(this.initialBytes, 16)));

            // Gather information about registers
            uint RdVal = ra[this.Rd].ReadWord(0, true);
            uint RnVal = ra[this.Rn].ReadWord(0, true);
            uint RmVal = ra[this.Rm].ReadWord(0, true);

            this.shifter = HandleShift(this.b25, this.shifter, RmVal, ra);
            //addressing mode
            uint addr = FindAddressingMode(ref ra);
            string cmd = "";
            
            // Check for Load / Store
            if (this.L)
            {
                cmd = "ldr";
                if (this.B)
                {                    
                    ra[this.Rd].WriteWord(0, 0);
                    ra[this.Rd].WriteByte(0, mem.ReadByte(addr));
                }
                else
                {
                    ra[this.Rd].WriteWord(0, mem.ReadWord(addr));
                }
            }
            // If not load then store
            else
            {
                cmd = "str";
                if (this.B)
                {                   
                    mem.WriteByte(addr, ra[this.Rd].ReadByte(0));
                }
                else
                {
                    mem.WriteWord(addr, RdVal);
                }
            }

            Logger.Instance.writeLog(string.Format("Specific Command: {0} {1}, 0x{2} : 0x{3} ", cmd, RdVal,
                Convert.ToString(addr, 16), Convert.ToString(this.initialBytes, 16)));
        }

        /// <summary>
        /// Deal with some of the knitty gritty that happens
        /// when we deal with operand2
        /// </summary>
        /// <param name="b25"></param>
        /// <param name="shifter"></param>
        /// <param name="RmVal"></param>
        /// <param name="ra"></param>
        /// <returns></returns>
        public Operand2 HandleShift(bool b25, Operand2 shifter, uint RmVal, Register[] ra)
        {
            // if this bit is set then we are dealing with a register
            if (b25) {shifter = Shift(!b25, shifter, RmVal, ra);}
            
            // otherwise it is an immediate value
            else {shifter.offset = shifter.imm12;}
            return shifter;
        }

        /// <summary>
        /// Finds out addressing specifics about the instruction
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        private uint FindAddressingMode(ref Register[] reg)
        {
            uint RdVal = reg[this.Rd].ReadWord(0, true);
            uint RnVal = reg[this.Rn].ReadWord(0, true);
            uint addr = 0;
            
            // if pre-indexed....
            if (this.P)
            {
                // ... and positively offset add offset to value
                // otherwise we need to subtract
                addr = (this.U) ? RnVal + this.shifter.offset : RnVal - this.shifter.offset;               

                // check for writeback
                if (this.W)
                {
                    reg[this.Rn].WriteWord(0, addr);
                }
            }
            // otherwise post-indexed
            else
            {
                addr = RnVal;
                uint val = (this.U) ? RnVal + this.shifter.offset : RnVal - this.shifter.offset;
                reg[this.Rn].WriteWord(0, val);                
            }
            return addr;
        }
    } 
}
