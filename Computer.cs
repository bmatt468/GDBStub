﻿using System;
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
        Register[] reg = new Register[15];
        
        uint pc = 0;
        Memory RAM;
        CPU cpu;
        Thread programThread;


        //instantiate the Computer!!! 
        //I don't have a computer yet?  woah!
        //r14 is the program counter

        public Computer()
        {
            RAM = new Memory(Option.Instance.getMemSize());

//defines 15 registers, 0 - 14
            for (int i = 0; i < 15; i++){
                reg[i] = new Register();
            }
        


            cpu = new CPU(ref RAM,ref reg);
        }

        //returns the register values from r0 - r15
        public string dumpRegisters(){
            string output = "";
            for (int i = 0; i < 15; ++i)
            {
                output += reg[i].getRegister();
            }
            return output;
        }
        private void clearRegisters()
        {

            for (int i = 0; i < 15; i++)
            {
                reg[i].CLEAR();
            }
               
        }


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
                /*
                if (Option.Instance.getTest())
                {
                    TestRam.RunTests();
                    TestSimulator.RunTests();
                }
                */
                ELFReader e = new ELFReader();
                byte[] elfArray = File.ReadAllBytes(file);
                if (elfArray.Length <= Option.Instance.getMemSize())
                {
                    //introspection!!!Woah!!!
                    e.ReadHeader(elfArray);

                    reg[14].WriteWord(0,e.elfHeader.e_entry);
                    //RAM = new Memory(memSize);
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


        // Run, Step, Stop/Break, and Reset
        // this method will probably be 
        // refactored into the gdb handler class.
        internal string command(string input)
        {
            StreamWriter log = new StreamWriter("log.txt", true);
            log.WriteLine("Comp: Command = " + input);
            log.Close();
            string output = "";
            string[] command = input.Split(' ');
            switch (command[0].ToLower())
            {
                case "run":
                    //dostuff
                    output = this.run();
                    break;
                case "step":
                    output = this.step();
                    break;
                case "stop":
                case "break":
                    output = this.stop();
                    break;
                case "reset":
                    output = this.reset();
                    break;
                case "load":
                    Option.Instance.setFile(command[1]);
                    readELF(Option.Instance.getFile(), Option.Instance.getMemSize());
                    output = "RAM: Hash is " + RAM.getHash();
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
                    this.log(addr, length);
                    
                    break;
                default:
                    output += "Invalid Command: valid commands are:\nrun \nstep \nstop/break \nreset \ndisplay [addr] [lines]";
                    break;
            }
            
            return output;
        }

    
        public string reset()
        {
            //reset logic
            RAM.CLEAR();
            clearRegisters();
            readELF(Option.Instance.getFile(), Option.Instance.getMemSize());


            return "Reset";
        }

        public string stop()
        {
            if (is_running)
            {
                //stop logic
                //programThread.Abort();
                is_running = false;
                return "Stopped";
            }
            else
            {
                return "Already Stopped";
            }
        }

        public string step()
        {
            if (!is_running)
            {
                //step logic

                // Start the thread
                programThread = new Thread(new ThreadStart(this.go));

                programThread.Start();
                while (!programThread.IsAlive) ;

                return "Step";
            }
            else
            {
                return "Cannot step, program is running";
            }
        }

        public string run()
        {
            if (!is_running)
            {
                is_running = true;
                //run logic

                // Start the thread
                programThread = new Thread(new ThreadStart(this.go));

                programThread.Start();
                while (!programThread.IsAlive) ;

                return "Running";
            }
            else
            {
                return "Already Running";
            }
        }

        private void go()
        {
         
            do
            {
                //fetch, decode, execute commands here
                uint word = cpu.fetch(reg[14].ReadWord(0));
                //break if we fetched a zero!
                if (word == 0) {
                    is_running = false;
                    break;
                }
                //decode the uint!
                string command = cpu.decode(word);

                //exeucte the decoded Command!!
                cpu.execute(command);
                incrementPC();
            } while (is_running);

            //finished running, now display!!
            //show the updated registers and disassembly panel to reflect the state of the simulation.\
            this.log();
        }

        private void log(uint addr = 1, int len = 10)
        {
            if (addr == 1)
            {
                addr = reg[14].ReadWord(0);
            }
            StreamWriter log = new StreamWriter("log.txt", true);
            string ramString = RAM.getAtAddress(addr, len);
            log.WriteLine("RAM");
            log.WriteLine(ramString);
            log.WriteLine("Registers");
            log.WriteLine(this.dumpRegisters());
            log.Close();
        }
        private void incrementPC(uint iter = 4)
        {
            pc = reg[14].ReadWord(0);
            pc += iter;
            reg[14].WriteWord(0, pc);
        }

    }
}
