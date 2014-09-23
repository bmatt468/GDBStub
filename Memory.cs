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
    class Memory
    {
        protected byte[] theArray;

        //program counter
        //Int32 pc;

        //Memory Constructor that takes no arguments
        //auto designates ram size to 32768
        public Memory()
        {
            if (theArray == null)
            {
                theArray = new byte[32768];
            }
        }

        public Memory(int memSize)
        {
            if (theArray == null)
            {
                theArray = new byte[memSize];
            }
        }
        
        public string getHash()
        {
            MD5 hasher = new MD5();

            return hasher.Hash(theArray);
        }


        public string getAtAddress(uint addr, int desiredLines = 8)
        {
            int numOfLines = 0;
            int numOfBytes = 0;
            string output = "";
            output = "RAM: starting at: " + addr.ToString() + "\n";
            for (; numOfLines < desiredLines && addr < theArray.Length; addr++)
            { 
                output += theArray[addr].ToString("X2");
                if ((numOfBytes + 1) % 2 == 0 && numOfBytes != 0)
                {
                    output += " ";
                }
                if ((numOfBytes + 1) % 16 == 0 && numOfBytes != 0)
                {
                    output += "\n";
                    numOfLines += 1;
                }
                numOfBytes++;
            }
            return output;
        }



        public byte[] getArray()
        {
            return theArray;
        }


        public bool TestFlag(UInt32 addr, byte bit)
        {
            if (bit >= 0 && bit < 32)
            {
                uint word = ReadWord(addr);
                string binary = Convert.ToString(word, 2);
                binary = binary.PadLeft(32, '0');
                if (binary[bit] == '1')
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;

        }

        public void SetFlag(UInt32 addr, byte bit, bool flag)
        {
            if (bit >= 0 && bit < 32)
            {
                uint word = ReadWord(addr);
                string binary = Convert.ToString(word, 2);
                binary = binary.PadLeft(32, '0');
                char[] binA = binary.ToCharArray();
                if (flag)
                {
                    binA[bit] = '1';
                }
                else
                {
                    binA[bit] = '0';
                }
                binary = new string(binA);
                word = Convert.ToUInt32(binary, 2);
                WriteWord(addr, word);
            }
        }

        public uint ReadWord(UInt32 addr)
        {
            uint output = 0;
            //if address is not divisible by 4 escape
            if (addr % 4 == 0)
            {
                output = BitConverter.ToUInt32(theArray, (int)addr);
            }
            return output;
        }//ReadWord

        public ushort ReadHalfWord(UInt32 addr)
        {
            ushort output = 0;
            //if address is not divisible by 4 escape
            if (addr % 2 == 0)
            {
                output = BitConverter.ToUInt16(theArray, (int)addr);
            }
            return output;
        }//ReadHalfWord

        public byte ReadByte(UInt32 addr)
        {
            byte output = theArray[addr];
            return output;
        }//ReadByte


        public void WriteWord(UInt32 addr, uint inpu)
        {
            if (addr % 4 == 0)
            {
                byte[] intBytes = BitConverter.GetBytes(inpu);
                Array.Copy(intBytes, 0, theArray, addr, 4);
            }
        }//WriteWord

        public void WriteHalfWord(UInt32 addr, ushort inpu)
        {
            if (addr % 2 == 0)
            {
                byte[] shortBytes = BitConverter.GetBytes(inpu);
                Array.Copy(shortBytes, 0, theArray, addr, 2);
            }
        }//WriteHalfWord

        public void WriteByte(UInt32 addr, byte inpu)
        {
            theArray[addr] = inpu;
        }//WriteByte

        public void CLEAR()
        {
            Array.Clear(theArray, 0, theArray.Length);
        }


    }
}
