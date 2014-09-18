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
        Register r0, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14 = new Register();
        int pc = 0;
        Memory RAM = new Memory();
        CPU cpu;
        Thread programThread;

        public string printArray(byte[] array)
        {
            string output = "";

            output = "The Array\n";
            for (int i = 0; i < array.Length; i++)
            {
                output += array[i].ToString("X2");
                i++;
                output += array[i].ToString("X2") + " ";
                if ((i + 1) % 16 == 0)
                {
                    output += "\n";
                }
            }
            return output;

        }

        /*
         * 
         * Unused might be good in the future
        public static byte[] stringToByteArray(string input){

            byte[] bA = new byte[input.Length * sizeof(char)];

            char[] inputArray = input.ToCharArray ();
            System.Buffer.BlockCopy(inputArray, 0, bA, 0, bA.Length);
            return bA;
        }
        */
        private void clearRegisters()
        {
            r0.CLEAR();
            r1.CLEAR();
            r2.CLEAR();
            r3.CLEAR();
            r4.CLEAR();
            r5.CLEAR();
            r6.CLEAR();
            r7.CLEAR();
            r8.CLEAR();
            r9.CLEAR();
            r10.CLEAR();
            r11.CLEAR();
            r12.CLEAR();
            r13.CLEAR();
            r14.CLEAR();
        }

        public int readELF(string file, int memSize)
        {
           /* RAM.CLEAR();
            clearRegisters();
            *///opens the log file to append
            StreamWriter log = new StreamWriter("log.txt", true);
            log.WriteLine("ELF: Reading ELF file");
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
                //introspection!!!Woah!!!
                e.ReadHeader(elfArray);


                Memory ram = new Memory(memSize);
                writeElfToRam(e, elfArray, ref ram);

                output = 1;

                string ramOutput = ram.displayAtAddress(e.elfphs[0].p_vaddr - 4, 32);
                log.WriteLine(ramOutput);
            }
            catch
            {
                output = -1;
            }
            log.Close();
            return output;

        }

        public void writeElfToRam(ELFReader e, byte[] elfArray, ref Memory ram)
        {

            //log.WriteLine("RAM: Size {0}", ram.getSize());


            for (int prog = 0; prog < e.elfHeader.e_phnum; prog++)
            {
                int ramAddress = e.elfphs[prog].p_vaddr;
                //log.WriteLine("RAM: Writing to {0} ", ramAddress);

                int elfOffSet = (int)e.elfphs[prog].p_offset;
                //log.WriteLine("ELF: Reading from {0}", e.elfphs[prog].p_offset);

                //log.WriteLine("ELF: Size of Segment {0}", e.elfphs[prog].p_filesz);
                int RamAddressCounter = ramAddress;
                int elfOffSetCounter = elfOffSet;

                for (; elfOffSetCounter < elfArray.Length &&
                        RamAddressCounter < e.elfphs[prog].p_filesz + ramAddress;
                            RamAddressCounter++, elfOffSetCounter++)
                {
                    ram.WriteByte(RamAddressCounter, elfArray[elfOffSetCounter]);
                }//for


            }//for

        }//writeElfToRam


        //Run, Step, Stop/Break, and Reset
        internal string command(string input)
        {
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
                    break;
                case "display":
                    //display the ram at an address
                    int addr = 0;
                    int length = 10;
                    try
                    {
                        addr = Convert.ToInt32(command[1]);
                        length = Convert.ToInt32(command[2]);
                    }
                    catch
                    {
                        
                    }
                    displayRam(addr, length);

                    break;
                default:
                    output += "Invalid Command: valid commands are:\nrun \nstep \nstop/break \nreset \ndisplay [addr] [lines]";
                    break;
            }
            return output;
        }

        //Displays the RAM
        private void displayRam(int addr, int length)
        {
            StreamWriter log = new StreamWriter("log.txt", true);
            if (RAM.getArray() != null)
            {
                string ramPrintOut = RAM.displayAtAddress(addr, length);
                log.WriteLine(ramPrintOut);
                Console.WriteLine(ramPrintOut);
            }
            else
            {
                log.WriteLine("Could not display ram, it is NULL");
            }
            log.Close();
        }




        public string reset()
        {
            //reset logic
            pc = 0;
            return "Reset";
        }

        public string stop()
        {
            if (is_running)
            {
                //stop logic
                programThread.Abort();
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
                uint word = cpu.fetch();
                string command = cpu.decode(word);

                cpu.execute(command);

                pc++;
                //Console.WriteLine(pc);
            } while (is_running);
        }

    }
}
