using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    // class to handle all the data processing operators
    // builds off its parent class
    class DataProcessing : Instruction
    {
        public uint opcode { get; set; }
        public bool b25 { get; set; }
        public Operand2 shifter { get; set; }
        public bool b4 { get; set; }
        public bool b7 { get; set; }

        /// <summary>
        /// This method parses the command to get the valuable information
        /// </summary>
        /// <param name="command"></param>
        public override void ParseCommand(Memory command)
        {
            // obtain all the magic bits that we need           
            this.b4 = command.TestFlag(0, 4);
            this.b7 = command.TestFlag(0, 7);
            this.S = command.TestFlag(0, 20);
            this.b25 = command.TestFlag(0, 25);

            // create our op2
            this.shifter = new Operand2(command);

            // check weird bits for possible multiplication
            if (b4 && b7 && !b25)
            {
                this.opcode = 0x1F;
                return;
            }
            else
            {                
                uint c = command.ReadWord(0, true);
                this.opcode = (uint)((c & 0x01E00000) >> 21);
                return;
            }
        }

        /// <summary>
        /// Run the commands that are given
        /// </summary>
        /// <param name="ra"></param>
        /// <param name="mem"></param>
        public override void Run(ref Register[] ra, ref Memory mem)
        {
            string tempHexString = Convert.ToString(this.initialBytes, 16);
            Logger.Instance.writeLog(string.Format("Command Type: Data Manipulation 0x{0}", tempHexString));
            // Try to discover what type of command we have
            
            switch (this.opcode)
            {
                case 0: // Logical AND                    
                    this.CommonWork(ref ra, ref mem,"and");
                    Logger.Instance.writeLog(string.Format("Specific Command: Logical AND 0x{0}", tempHexString));
                    break;
                case 1: // Logical Exclusive OR
                    this.CommonWork(ref ra, ref mem,"eor");
                    Logger.Instance.writeLog(string.Format("Specific Command: Logical Exclusive OR 0x{0}", tempHexString));
                    break;
                case 2: // Subtract
                    this.CommonWork(ref ra, ref mem,"sub");
                    Logger.Instance.writeLog(string.Format("Specific Command: Subtrac 0x{0}", tempHexString));
                    break;
                case 3: // Reverse Subtract
                    this.CommonWork(ref ra, ref mem,"rsb");
                    Logger.Instance.writeLog(string.Format("Specific Command: Reverse Subtract 0x{0}", tempHexString));
                    break;
                case 4: // Add
                    this.CommonWork(ref ra, ref mem, "add");
                    Logger.Instance.writeLog(string.Format("Specific Command: Addition 0x{0}", tempHexString));
                    break;
                case 5: // Add with Carry (unimplemented)
                case 6: // Subtract with Carry (unimplemented)
                case 7: // Reverse Subtract with Carry (unimplemented)
                case 8: // Test (unimplemented)
                case 9: // Test Equivalence (unimplemented)
                    break;
                case 10: // Compare
                    this.cmp(ref ra, ref mem);
                    Logger.Instance.writeLog(string.Format("Specific Command: Compare 0x{0}", tempHexString));
                    break;
                case 11: // Compare Negated (unimplemented)
                    break;
                case 12: // Logical (inclusive) OR
                    this.CommonWork(ref ra, ref mem, "oor");
                    Logger.Instance.writeLog(string.Format("Specific Command: Logical (inclusive) OR 0x{0}", tempHexString));
                    break;
                case 13: // Move
                    this.CommonWork(ref ra, ref mem,"mov");
                    Logger.Instance.writeLog(string.Format("Specific Command: Move 0x{0}", tempHexString));
                    break;
                case 14: // Bit Clear
                    this.CommonWork(ref ra, ref mem,"bic");
                    Logger.Instance.writeLog(string.Format("Specific Command: Bit Clear 0x{0}", tempHexString));
                    break;
                case 15: // Move Not
                    this.CommonWork(ref ra, ref mem,"mvn");
                    Logger.Instance.writeLog(string.Format("Specific Command: Move Not 0x{0}", tempHexString));
                    break;
                case 0x1F: // Multiply
                    this.mul(ref ra, ref mem);
                    Logger.Instance.writeLog(string.Format("Specific Command: Multiply 0x{0}", tempHexString));
                    break;
                default:
                    break;
            }
        }              

        /// <summary>
        /// Method to handle the common work for data processing
        /// Each of the operations are similar so this method
        /// reduces the amount of copy/paste
        /// </summary>
        /// <param name="ra"></param>
        /// <param name="mem"></param>
        /// <param name="cmd"></param>
        private void CommonWork(ref Register[] ra, ref Memory mem, string cmd)
        {
            this.shifter = Shift(this.b25, this.shifter, ra[this.shifter.Rm].ReadWord(0, true), ra);
            uint source = ra[this.Rn].ReadWord(0, true);
            switch (cmd)
            {                
                case "add":
                    ra[this.Rd].WriteWord(0, (source + this.shifter.offset));
                    Logger.Instance.writeLog(String.Format("Assembly: ADD R{0},R{1},{2} : 0x{3}",
                    this.Rd, this.Rn, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
                    break;
                case "and":
                    ra[this.Rd].WriteWord(0, (source & this.shifter.offset));
                    Logger.Instance.writeLog(String.Format("Assembly: AND R{0},R{1},{2} : 0x{3}",
                    this.Rd, this.Rn, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
                    break;
                case "bic":
                    ra[this.Rd].WriteWord(0, (source & (~this.shifter.offset)));
                    Logger.Instance.writeLog(String.Format("Assembly: BIC R{0},R{1},{2} : 0x{3}",
                    this.Rd, this.Rn, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
                    break;
                case "eor":
                    ra[this.Rd].WriteWord(0, (source ^ this.shifter.offset));
                    Logger.Instance.writeLog(String.Format("Assembly: EOR R{0},R{1},{2} : 0x{3}",
                    this.Rd, this.Rn, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
                    break;
                case "mov":
                    ra[this.Rd].WriteWord(0, this.shifter.offset);
                    Logger.Instance.writeLog(String.Format("Assembly: MOV R{0},{1} : 0x{2}",
                    this.Rd, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
                    if (this.S)
                    {
                        flagWork(this.initialBytes, ra[this.Rd].ReadWord(0));
                    }
                    break;
                case "mvn":
                    ra[this.Rd].WriteWord(0, ~this.shifter.offset);
                    Logger.Instance.writeLog(String.Format("Assembly: MVN R{0},{1} : 0x{2}",
                    this.Rd, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
                    flagWork(this.initialBytes, ra[this.Rd].ReadWord(0));
                    break;
                case "oor":
                    ra[this.Rd].WriteWord(0, (source | this.shifter.offset));
                    Logger.Instance.writeLog(String.Format("Assembly: OOR R{0},R{1},{2} : 0x{3}",
                    this.Rd, this.Rn, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
                    break;
                case "rsb":
                    ra[this.Rd].WriteWord(0, (this.shifter.offset - source));
                    Logger.Instance.writeLog(String.Format("Assembly: RSB R{0},R{1},{2} : 0x{3}",
                    this.Rd, this.Rn, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
                    break;
                case "sub":
                    ra[this.Rd].WriteWord(0, (source - this.shifter.offset));
                    Logger.Instance.writeLog(String.Format("Assembly: sub R{0},R{1},{2} : 0x{3}",
                    this.Rd, this.Rn, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
                    break;
                default:
                    break;
            }
        }        
        
        private void cmp(ref Register[] ra, ref Memory mem)
        {
            this.shifter = Shift(this.b25, this.shifter, ra[this.shifter.Rm].ReadWord(0, true), ra);
            uint source = ra[this.Rn].ReadWord(0, true);
            uint comparer = source - this.shifter.offset;
            Memory m = new Memory(4);
            m.WriteWord(0, comparer);
            //set N flag
            N = m.TestFlag(0, 31);
            //set Z
            Z = m.ReadWord(0, true) == 0;
            //set C
            C = (this.shifter.offset <= source) ? true : false;
            //set O (which somehow I though was F...)
            int RnTest = (int)source;
            int opTest = (int)this.shifter.offset;
            bool case1 = ((RnTest >= 0) && (opTest < 0) && ((RnTest - opTest) < 0));
            bool case2 = ((RnTest < 0) && (opTest >= 0) && ((RnTest - opTest) >= 0)); 
            F = (case1 || case2) ? true : false;
            
            Logger.Instance.writeLog(String.Format("Assembly: CMP R{0}, {1} : 0x{2}",
                this.Rn, this.shifter.offset, Convert.ToString(this.initialBytes, 16)));
        }        
                
        private void mul(ref Register[] reg, ref Memory RAM)
        {
            uint rmOp = reg[this.shifter.Rm].ReadWord(0, true);
            uint rsOp = reg[this.shifter.regShiftLength].ReadWord(0, true);
            uint product = rmOp * rsOp;
            if (product > 0xFFFFFFFF) { Logger.Instance.writeLog("Error: Multiply Overflow"); }
            reg[this.Rn].WriteWord(0, product);
            Logger.Instance.writeLog(String.Format("Assembly: MUL R{0},R{1},R{2} : 0x{3}",
                this.Rd, this.shifter.Rm, this.shifter.regShiftLength, Convert.ToString(this.initialBytes, 16)));
        }

        private void flagWork(uint bytes, uint dest)
        {
            Memory m = new Memory(4);
            m.WriteWord(0, bytes);
            bool tempS = m.TestFlag(0, 20);
            if (tempS)
            {
                Memory m2 = new Memory(4);
                m2.WriteWord(0, dest);
                //set N flag
                N = m2.TestFlag(0, 31);
                //set Z
                Z = m2.ReadWord(0, true) == 0;
                C = false;
                F = false;
                
            }
        }
    }
}
