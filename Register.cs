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
        public string getRegister()
        {
            string output = "";
            uint regData = ReadWord(0);
            output = regData.ToString("X2").PadLeft(8, '0');
            return output;
        }

    }
}
