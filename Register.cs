using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    //Registers are a type of Memory.
    class Register : Memory
    {
        //creates a register. they are all 32 bits or 4 bytes. yay!!!
        public Register()
        {
            theArray = new byte[4];
        }

        //displays the register
        public byte[] getRegister()
        {
            return theArray;
        }


        internal string getRegString()
        {
            string output = "";
            output = ReadWord(0).ToString().PadLeft(8, '0');
            return output;
        }
    }
}
