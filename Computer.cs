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
        //is running flag
        bool is_running = false;
        bool N, Z, C, F = false;
        string checkSum = "";
        uint step_number = 0;
        Logger myLog;
        Register[] reg = new Register[16];
        Memory RAM;
        CPU cpu;
        Thread programThread;


        //instantiate the Computer!!! 
        //I don't have a computer yet?  woah!
        //r15 is the program counter
        public Computer()
        {
            this.RAM = new Memory(Option.Instance.getMemSize());
            this.myLog = new Logger();
            this.myLog.clearLog();
            this.myLog.toggleTrace();

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
        public bool getN() { return N; }
        public bool getZ() { return Z; }
        public bool getC() { return C; }
        public bool getF() { return F; }

        public string getCheckSum() { return RAM.getHash(); }
        public Register getReg(uint r) { return reg[r]; }
        public Memory getRAM() { return RAM; }
        public CPU getCPU() { return cpu; }
        public Logger logger() { return myLog; }
        public uint getStepNumber(){ return step_number;}

        // returns the tracing data for gdb
        public bool getTraceStatus(){ return myLog.getTraceStatus(); }


//----------Dumpers Kind of like getters--------


        //dumps the requested Ram into a byte array
        public byte[] dumpRAM(uint addr, int length)
            { return RAM.dump(addr, length); }


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
        public byte[] dumpRegister(UInt32 n) { return reg[n].getRegister(); }


//-------------- End Getters//

//------------------- Setters
        private void toggleTrace() { myLog.toggleTrace(); }


        /// <summary>
        /// Clears the data from all of 
        /// The Registers.
        /// </summary>
        private void clearRegisters()
        {
            for (int i = 0; i < 16; i++)
            {
                reg[i].CLEAR();
            }
        }

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
            StreamWriter log = new StreamWriter("log.txt", true);
            log.WriteLine("ELF: Reading ELF file");
            log.Close();
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

                    writeElfToRam(e, elfArray, ref RAM);


                    string ramOutput = RAM.getAtAddress((uint)e.elfphs[0].p_vaddr, 8);

                    log = new StreamWriter("log.txt", true);
                    log.WriteLine(ramOutput);
                    log.Close();
                    Console.WriteLine(ramOutput);
                    output = 0;
                }
                else //file to large
                {
                    output = -3;
                    log = new StreamWriter("log.txt", true);
                    log.WriteLine("Err: File to Large");
                    log.Close();
                }
               
            }
            catch (System.IO.FileNotFoundException) 
            {
                output = -2;
                log = new StreamWriter("log.txt", true);
                log.WriteLine("Err: File not found");
                log.Close();
            }
            catch //general exception
            {
                output = -1;
                log = new StreamWriter("log.txt", true);
                log.WriteLine("Err: Something went wrong");
                log.Close();
            }
            return output;

        }


        //writes the ELF file to the RAM array
        public void writeElfToRam(ELFReader e, byte[] elfArray, ref Memory ram)
        {

            //log.WriteLine("RAM: Size {0}", ram.getSize());


            for (int prog = 0; prog < e.elfHeader.e_phnum; prog++)
            {
                uint ramAddress = (uint)e.elfphs[prog].p_vaddr;
                //log.WriteLine("RAM: Writing to {0} ", ramAddress);

                uint elfOffSet = (uint)e.elfphs[prog].p_offset;
                //log.WriteLine("ELF: Reading from {0}", e.elfphs[prog].p_offset);

                //log.WriteLine("ELF: Size of Segment {0}", e.elfphs[prog].p_filesz);
                uint RamAddressCounter = ramAddress;
                uint elfOffSetCounter = elfOffSet;

                for (; elfOffSetCounter < elfArray.Length &&
                        RamAddressCounter < e.elfphs[prog].p_filesz + ramAddress;
                            RamAddressCounter++, elfOffSetCounter++)
                {
                    ram.WriteByte(RamAddressCounter, elfArray[elfOffSetCounter]);
                }//for


            }//for

        }//writeElfToRam

//End ELF code

//-------End Setters------


        // Run, Step, Stop/Break, and Reset
 


 //---------------Actions
 //Load, Reset, Stop, Step, and Run and Go.
        private void load(string file)
        {
            Option.Instance.setFile(file);
            readELF(Option.Instance.getFile(), Option.Instance.getMemSize());
            checkSum = RAM.getHash();
            step_number = 0;
            myLog.writeLog("RAM: Hash is " + RAM.getHash());
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
            RAM.CLEAR();
            clearRegisters();
            readELF(Option.Instance.getFile(), Option.Instance.getMemSize());

            step_number = 0;
            myLog.writeLog("Reset");
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
                myLog.writeLog("Stopped");
            }
            else
            {
               myLog.writeLog("Already Stopped");
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

                // Start the thread
                programThread = new Thread(new ThreadStart(this.go));

                programThread.Start();
                //waits for thread to get moving.
                while (!programThread.IsAlive) ;

                myLog.writeLog("Step");
            }
            else
            {
               myLog.writeLog("Cannot step, program is running");
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

               myLog.writeLog("Running");
            }
            else
            {
                myLog.writeLog("Already Running");
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
         
            do
            {
                //fetch, decode, execute commands here
                uint word = cpu.fetch(reg[15].ReadWord(0));
                //break if we fetched a zero!
                if (word != 0) {

                    //decode the uint!
                    string command = cpu.decode(word);

                    //exeucte the decoded Command!!
                    cpu.execute(command);
                    step_number++;

                    incrementPC();
                }
                else
                {
                    is_running = false;
                }
                //write to the trace log...
                myLog.writeTrace(this);
                    
            } while (is_running);


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
            myLog.writeLog("Comp: Command = " + input);

            string output = "";
            string[] command = input.Split(' ');
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
                case "trace":
                    this.myLog.toggleTrace();
                    break;
                case "display":
                    //display the ram at an address and registers.
                    UInt32 addr = 0;
                    int length = 10;
                    try
                    {
                        addr = Convert.ToUInt32(command[1]);
                        length = Convert.ToInt32(command[2]);
                    }
                    catch { }
                    //log data
                    myLog.writeLog(RAM.getAtAddress(addr, length));

                    break;
                default:
                    output += "Invalid Command: valid commands are:\nrun \nstep \nstop/break \nreset \ndisplay [addr] [lines]";
                    break;
            }
            myLog.writeLog(output);
        }


    }
}
