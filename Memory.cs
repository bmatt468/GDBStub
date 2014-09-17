using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    class Memory
    {
        private byte[] theArray;

        //program counter
        //Int32 pc;

        public Memory(int memSize)
        {
            theArray = new byte[memSize];
        }
        /*
        public string getHash()
        {
            MD5 hasher = new MD5();

            return hasher.Hash(theArray);
        }
        */


        public byte[] getArray()
        {
            return theArray;
        }

        public int getSize()
        {
            return theArray.Length;
        }

        public bool TestFlag(Int32 addr, byte bit)
        {
            if (bit >= 0 && bit < 32)
            {
                int word = ReadWord(addr);
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

        public void SetFlag(Int32 addr, byte bit, bool flag)
        {
            if (bit >= 0 && bit < 32)
            {
                int word = ReadWord(addr);
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
                word = Convert.ToInt32(binary, 2);
                WriteWord(addr, word);
            }
        }

        public int ReadWord(Int32 addr)
        {
            //if address is not divisible by 4 escape
            if (addr % 4 == 0)
            {
                int output = BitConverter.ToInt32(theArray, addr);
                return output;
            }
            return -1;
        }//ReadWord

        public short ReadHalfWord(Int32 addr)
        {
            //if address is not divisible by 4 escape
            if (addr % 2 == 0)
            {
                short output = BitConverter.ToInt16(theArray, addr);
                return output;
            }
            return -1;
        }//ReadHalfWord

        public byte ReadByte(Int32 addr)
        {
            byte output = theArray[addr];
            return output;
        }//ReadByte


        public void WriteWord(Int32 addr, int inpu)
        {
            if (addr % 4 == 0)
            {
                byte[] intBytes = BitConverter.GetBytes(inpu);
                Array.Copy(intBytes, 0, theArray, addr, 4);
            }
        }//WriteWord

        public void WriteHalfWord(Int32 addr, short inpu)
        {
            if (addr % 2 == 0)
            {
                byte[] shortBytes = BitConverter.GetBytes(inpu);
                Array.Copy(shortBytes, 0, theArray, addr, 2);
            }
        }//WriteHalfWord

        public void WriteByte(Int32 addr, byte inpu)
        {
            theArray[addr] = inpu;
        }//WriteByte

        public void CLEAR()
        {
            Array.Clear(theArray, 0, theArray.Length);
        }
    }
}
