using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    //Registers are a type of Memory.
    class Register : Memory
    {
        //creates a register. they are all 32 bit or 4 bytes. yay!!!
        public Register()
        {
            theArray = new byte[4];
        }



    }
}
