using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDBStub
{
    //the instruction class object
    class instruction
    {
        public instruction(uint command)
        {
            //initialize self based on command

            //decode data
            //Check special cases
            // next check instruction typebit 25..27 
            /*
             * Determine category
             * data processing
             * 25 & 4 can distinguish operand 2 types
             * check the PUBWL 
             */
            //
        }
    }
}
