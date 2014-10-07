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
     * A Computer or System class that represents your simulated computer;
     * contains registers, CPU, and RAM objects; 
     * and has relevant methods like run and step.
     * The run method should call the CPU’s fetch, decode, and execute in a loop, until fetch returns a 0.
     * The step method should call the CPU’s fetch, decode, and execute methods (just once, not in a loop).
     * 
     * 
     */
    class Computer
    {
        public struct status
        {
            public char statchar { get; set; }
            public string statval { get; set; }

        }
        public status compStatus { get; set; }

        
        //is running flag
        bool is_running = false;
        bool N, Z, C, F = false;
        string checkSum = "";
        uint step_number = 0;
        Dictionary<uint, uint> storedCommands = new Dictionary<uint,uint>();

        Register[] reg = new Register[16];
        Memory RAM;
        CPU cpu;
        Thread programThread;
        private Object thisLock = new Object();

        private static Computer instance;

        public static Computer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Computer();
                }
                return instance;
            }
        }

        //instantiate the Computer!!! 
        //I don't have a computer yet?  woah!
        //r15 is the program counter
        public Computer()
        {
            this.RAM = new Memory(Option.Instance.getMemSize());
            
            Logger.Instance.clearLog();

            //defines 15 registers, 0 - 15
            for (int i = 0; i < 16; i++){
                this.reg[i] = new Register();
            }
        
            //activate trace for the first time
            //trace = new StreamWriter("trace.log", false);

            this.cpu = new CPU(ref RAM,ref reg);
        }


//--------------- Getters ---------//

        public bool getIsRunning(){ return is_running;}
//flags

        public bool getFlag(char flag)
        {
            bool output = false;
            switch (flag)
            {
                case 'N':
                    output = N;
                    break;
                case 'Z':
                    output = Z;
                    break;
                case 'C':
                    output = C;
                    break;
                case 'F':
                    output = F;
                    break;
                default:
                    Logger.Instance.writeLog("FLAG: INVALID FLAG REQUESTED");
                    break;
            }
            Logger.Instance.writeLog(String.Format("FLAG: {0} = {1}", flag, output.ToString()));
            return output;
        }

        public string getCheckSum() { return RAM.getHash(); }
        public Register getReg(uint r) { return reg[r]; }
        public Memory getRAM() { return RAM; }
        public CPU getCPU() { return cpu; }
        public uint getStepNumber(){ return step_number;}

        // returns the tracing data for gdb
        public bool getTraceStatus()
        {
            bool output =  Logger.Instance.getTraceStatus();
            Logger.Instance.writeLog(String.Format("TRACE: Status {0}", output.ToString()));
            return output;
        }

        public bool getThreadStatus() { return programThread.IsAlive; }

//----------Dumpers Kind of like getters--------


        //dumps the requested Ram into a byte array
        public byte[] dumpRAM(uint addr, int length)
        {
            Logger.Instance.writeLog(String.Format("RAM: Address = {0}", Convert.ToString(addr, 16)));
            Logger.Instance.writeLog(String.Format("RAM: Length = {0}", length));
            Logger.Instance.writeLog(RAM.getAtAddress(addr,length));
            return RAM.dump(addr, length); 
        }


        //returns the register values from r0 - r15
        // in a one byte array.
        public byte[] dumpRegisters()
        {
            byte[] output = new byte[16*4];
            int outputIndex = 0;
                for (int regIndex = 0; regIndex < 16; ++regIndex)
                {
                    for (uint byteIndex = 0; byteIndex < 4; ++byteIndex, ++outputIndex)
                    { 
                        output[outputIndex] = reg[regIndex].ReadByte(byteIndex);
                    }
                }
                Logger.Instance.writeLog("REG: Returned");
            return output;
        }



        /// dumps a single register of data
        /// in the form XX.. where XX is the hex
        /// data from the specified register
        /// Registers range from 0-15
        /// 
        /// param name="n" Unsigned 32 bit integer that references
        /// the register asked for
        /// returns A byte array
        public byte[] dumpRegister(UInt32 n) 
        {
            Logger.Instance.writeLog(string.Format("REG: # Requested {0}", n));  
            return reg[n].getRegister(); 
        }



        private bool isBreakPoint(Memory rawInstruction)
        {
            bool output = false;
            if ((rawInstruction.ReadWord(0) & 0xE1200070) == 0xE1200070)
                output = true;
            
            return output;
        }

//-------------- End Getters//



//------------------- Setters

        /// <summary>
        /// writes data to a specified register
        /// </summary>
        /// <param name="r">The # of the Register</param>
        /// <param name="x">The amount to be written to a register. Form of a byteArray
        /// </param>
        public void writeRegister(uint r, byte[] x)
        {
            if (r < 16)
            {
                for (uint i = 0; i < x.Length; ++i)
                {
                    reg[r].WriteByte(i, x[i]);
                }
            }
            Logger.Instance.writeLog(String.Format("REG: Wrote {0} to REG", Logger.Instance.byteArrayToString(x)));

        }

        /// <summary>
        /// Write a byte array to an address in memory
        /// </summary>
        /// <param name="addr">The address to start in memory</param>
        /// <param name="x">The byte array to write to memory</param>
        public void writeRAM(uint addr, byte[] x , int len) 
        {
            uint baseAddr = addr;
            for (int i = 0; i < x.Length; ++addr, ++i)
            {
                RAM.WriteByte(addr, x[i]);
               // Logger.Instance.writeLog(string.Format("RAM: Wrote info to MEM at {0}", Convert.ToString(addr,16)));
               // Logger.Instance.writeLog(String.Format("RAM: Info = {0}", Convert.ToString(x[i],16)));
            }
            Logger.Instance.writeLog(string.Format("RAM: Wrote info to MEM at {0}", Convert.ToString(baseAddr,16)));



        }

        /// <summary>
        /// Clears the data from all of 
        /// The Registers.
        /// </summary>
        public void clearRegisters()
        {
            for (int i = 0; i < 16; i++)
            {
                reg[i].CLEAR();
            }
            Logger.Instance.writeLog("REG: Cleared");
        }


        public void CLEAR()
        {
            RAM.CLEAR();
            clearRegisters();
            Logger.Instance.writeLog("COMP: RESET");
        }

        /// <summary>
        /// Takes the address and the immediate value
        /// will default to 0
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="immed"></param>
        public void setBreakPoint(uint addr, ushort immed = 0)
        {

            uint top12 = (uint)(immed & 0xFFF0) << 4;
            uint bot4 = (uint)immed & 0x000F;
            UInt32 breakPointValue = 0xE1200070 + top12 + bot4;
            uint saveCommand = RAM.ReadWord(addr);

            // Presever the command if it's not a breakpoint already
            if ((saveCommand & 0xE1200070) != 0xE120070) 
                storedCommands[addr] = saveCommand;
            //Write the new breakpoint
            RAM.WriteWord(addr, breakPointValue);
            Logger.Instance.writeLog(string.Format("BREAK: Set At {0}", Convert.ToString(addr,16)));

        }

        /// <summary>
        /// Removes a breakpoint at addr location.  
        /// Returns -1 if it's not a breakpoint to remove.  
        /// Returns 0 if all went ok.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns>-1 as an error of not a breakpoint.  0 if it's ok.</returns>
        public int removeBreakPoint(uint addr)
        {
            try
            {
                RAM.WriteWord(addr, storedCommands[addr]);
                Logger.Instance.writeLog(string.Format("BREAK: Removed At {0}", Convert.ToString(addr,16)));

            }
            catch 
            {
                //not a Breakpoint.  No breakpoint removed
                return -1;
            }
            return 0;
        }

//-------End Setters------


//------- ELF code
        //reads the ELF
        /* Error codes:
         *  0 = OK
         * -1 = General
         * -2 = File not found
         * -3 = file to large
         */
        public int readELF(string file, int memSize)
        {
           /* RAM.CLEAR();
            clearRegisters();
            *///opens the log file to append
           // StreamWriter log = new StreamWriter("log.txt", true);
            Logger.Instance.writeLog("ELF: Reading ELF file");
            
            int output = -1;
            try
            {

                ELFReader e = new ELFReader();
                byte[] elfArray = File.ReadAllBytes(file);
                if (elfArray.Length <= Option.Instance.getMemSize())
                {
                    //introspection!!!Woah!!!
                    e.ReadHeader(elfArray);

                    reg[15].WriteWord(0,e.elfHeader.e_entry);

                    writeElfToRam(e, elfArray);


                    string ramOutput = RAM.getAtAddress((uint)e.elfphs[0].p_vaddr, 8);
                    Logger.Instance.writeLog("File: Loaded");
                    Logger.Instance.writeLog(ramOutput);
                    //Console.WriteLine(ramOutput);
                    output = 0;
                }
                else //file to large
                {
                    output = -3;
                    Logger.Instance.writeLog("Err: File to Large");
                    
                }
               
            }
            catch (System.IO.FileNotFoundException) 
            {
                output = -2;
                Logger.Instance.writeLog("Err: File not found");
                
            }
            catch //general exception
            {
                output = -1;
                Logger.Instance.writeLog("Err: Something went wrong");
                
            }
            return output;

        }


        //writes the ELF file to the RAM array
        public void writeElfToRam(ELFReader e, byte[] elfArray)
        {

            for (int prog = 0; prog < e.elfHeader.e_phnum; prog++)
            {
                uint ramAddress = (uint)e.elfphs[prog].p_vaddr;
                //Logger.Instance.writeLog("RAM: Writing to {0} ", ramAddress);

                uint elfOffSet = (uint)e.elfphs[prog].p_offset;
                //Logger.Instance.writeLog("ELF: Reading from {0}", e.elfphs[prog].p_offset);

                //Logger.Instance.writeLog("ELF: Size of Segment {0}", e.elfphs[prog].p_filesz);
                uint RamAddressCounter = ramAddress;
                uint elfOffSetCounter = elfOffSet;

                for (; elfOffSetCounter < elfArray.Length &&
                        RamAddressCounter < e.elfphs[prog].p_filesz + ramAddress;
                            RamAddressCounter++, elfOffSetCounter++)
                {
                    RAM.WriteByte(RamAddressCounter, elfArray[elfOffSetCounter]);
                }//for


            }//for

        }//writeElfToRam

//End ELF code




 //---------------Actions
 //Load, Reset, Stop, Step, and Run and Go.
        public void load(string file, int memSize = -1)
        {
            if (memSize == -1)
            {
                memSize = Option.Instance.getMemSize();
            }
            else
            {
                Option.Instance.setMemSize(memSize);
            }
            Option.Instance.setFile(file);
            readELF(Option.Instance.getFile(), memSize);
            checkSum = RAM.getHash();
            step_number = 0;
            Logger.Instance.writeLog("RAM: Hash is " + RAM.getHash());
        }

    /*resets the program by:
     * clearing the Ram
     * clearing the REgisters
     * reading in the most recently loaded file
     * resetting the step number
     */
        public void reset()
        {
            //reset logic
            this.CLEAR();
            readELF(Option.Instance.getFile(), Option.Instance.getMemSize());

            step_number = 0;
            Logger.Instance.writeLog("*****\nReset\n*****\n");
        }

        /*
         * Stops the program by 
         * setting the is_running flag to false, which
         * will cancel any thread that is running
         */
        public void stop()
        {
            if (is_running)
            {
                //stop logic
                is_running = false;
                Logger.Instance.writeLog("Stopped");
            }
            else
            {
               Logger.Instance.writeLog("Already Stopped");
            }
        }

        /*
         * steps through the program
         * by going through one
         * fetch, decode, execute cycle
         * uses a thread
         */
        public void step()
        {
            if (!is_running)
            {
                //step logic

                this.go();

                Logger.Instance.writeLog("Step");
            }
            else
            {
               Logger.Instance.writeLog("Cannot step, program is running");
            }
        }
        /*
         * Runs through the program
         * reading from ram until it hits a zero
         * or the is_running flag is set to false
         * uses a thread
         */
        public void run()
        {
            if (!is_running)
            {
                is_running = true;
                //run logic

                // Start the thread
                programThread = new Thread(new ThreadStart(this.go));
                programThread.Start();
                //wait for thread to get going.
                while (!programThread.IsAlive) ;

               Logger.Instance.writeLog("Running");
            }
            else
            {
                Logger.Instance.writeLog("Already Running");
            }
        }

        /*
         * the Go method.
         * goes through a fetch, decode execute cycle
         * until the is_running flag is set to false
         * is a do__while loop
         * so it will occur at least once
         * this lets the step function call it without changing the flag
         * MULTITHREADED FUNCTIONS CALL THIS!!! 
         * TO BE USED BY A THREAD
         */
        private void go()
        {
         //mutex lock
            lock(thisLock){
                do
                {
                    //fetch, decode, execute commands here
                    Memory rawInstruction = cpu.fetch();
                    //break if we fetched a zero!
                    //will change to a finish command like .exit
                    if (rawInstruction.ReadWord(0) != 0)
                        {
                            if (!isBreakPoint(rawInstruction))
                            {
                                //decode the uint!
                                Instruction cookedInstruction = cpu.decode(rawInstruction);

                                //exeucte the decoded Command!!
                                cpu.execute(cookedInstruction);

                                step_number++;
                                incrementPC();
                            }
                            else
                            {
                                //breakpoint
                                
                                is_running = false;
                                status breakedStatus = new status();
                                breakedStatus.statchar = 'S';
                                breakedStatus.statval = "05";
                                compStatus = breakedStatus;
                                Logger.Instance.writeLog("COMP: Stopped");
                                Logger.Instance.writeLog(string.Format("BREAK: Hit a BreakPoint at step# {0}", step_number));
                                return;
                            }
                        }else
                        {
                            //finished
                            is_running = false;
                            Logger.Instance.writeLog("COMP: Finished");

                            //W00
                            Logger.Instance.writeLog(reg[15].getRegString());
                            status endedStatus = new status();
                            endedStatus.statchar = 'W';
                            endedStatus.statval = "00";
                            compStatus = endedStatus;
                            this.CLEAR();
                            return;
                        }

                    //write to the trace log...
                    Logger.Instance.writeTrace(this);
                    
                } while (is_running);
                //SO5
                status stoppedStatus = new status();
                stoppedStatus.statchar = 'S';
                stoppedStatus.statval = "05";
                compStatus = stoppedStatus;
                Logger.Instance.writeLog("COMP: Stopped");

            }

            //mutex unlock

        }



        private void incrementPC(uint iter = 4)
        {
            uint pc = reg[15].ReadWord(0);
            pc += iter;
            reg[15].WriteWord(0, pc);
        }


// end Actions
        // this method will probably be 
        // refactored into the gdb handler class.
        internal void command(string input)
        {
            Logger.Instance.writeLog("Comp: Command = " + input);

            string output = "";
            string[] command = input.Split(' ');
            uint addr;
            switch (command[0].ToLower())
            {
                case "run":
                    //dostuff
                    this.run();
                    break;
                case "step":
                    this.step();
                    break;
                case "stop":
                case "break":
                    this.stop();
                    break;
                case "reset":
                    this.reset();
                    break;
                case "load":
                    this.load(command[1]);
                    break;
                case "breakpoint":
                    addr = Convert.ToUInt16(command[1]);
                    this.setBreakPoint(addr, 0);
                    break;
                case "rbkp":
                    addr = Convert.ToUInt16(command[1]);
                    this.removeBreakPoint(addr);
                    break;
                case "trace":
                    Logger.Instance.toggleTrace();
                    break;
                case "display":
                    //display the ram at an address and registers.
                    addr = 0;
                    int length = 10;
                    try
                    {
                        addr = Convert.ToUInt32(command[1]);
                        length = Convert.ToInt32(command[2]);
                    }
                    catch { }
                    //log data
                    Logger.Instance.writeLog(RAM.getAtAddress(addr, length));

                    break;
                default:
                    output += "Invalid Command: valid commands are:\nrun \nstep \nstop/break \nreset \ndisplay [addr] [lines]";
                    break;
            }
            Logger.Instance.writeLog(output);
        }




    }
}
