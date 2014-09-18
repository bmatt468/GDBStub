using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;

namespace GDBStub
{
    class Simulator
    {
        
        //public static OptionParser options;

        public static void printArray(byte[] array)
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
            //log.WriteLine(output);

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

        public static int run(string file, int memSize)
        {
            int output = -1;
            try
            {
                /*
                if (options.getTest())
                {
                    TestRam.RunTests(//log);
                    TestArmSim.RunTests(//log);
                }
                */
                ELFReader e = new ELFReader();
                byte[] elfArray = File.ReadAllBytes(file);
                //introspection!!!Woah!!!
                e.ReadHeader(elfArray);


                Memory ram = new Memory(memSize);
                writeElfToRam(e, elfArray, ref ram);

                output = 1;
                printArray(ram.getArray());

            }
            catch
            {
                output = -1;
            }
            return output;

        }

        public static void writeElfToRam(ELFReader e, byte[] elfArray, ref Memory ram)
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
    }//class

}//namespace
