using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBStub
{
    class Program
    {
        static void Main(string[] args)
        {
            Handler handle = new Handler();
            handle.Listen();
        }
    }
}
