using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDBStub
{
    //Option Parser is a singleton class. there is one of them that holds the current 
    // options that have been set.
    public class Option
    {
        private static Option instance;

        private Option() {}
        
        public static Option Instance{
            get {
                if (instance == null){
                    instance = new Option();
                }
            return instance;
            }
        }
        
        public bool exec { get; set; }
        public bool load { get; set; }
        public bool test { get; set; }
        public int memSize { get; set; }
        public string file { get; set; }
        public bool debug { get; set; }

        public void DisplayUsage(string input = "")
        {
            if (input != "") { Console.WriteLine(input); }
            Console.WriteLine("armsim [--load elf-file] [ --mem memory-size ] [ --test] [--exec] [--debug]");
        }

        public bool parseArgs(string[] input)
        {
            file = "";
            test = false;
            memSize = 32768;
            bool valid = true;
            debug = false;
            if (input.Length == 0) {
                DisplayUsage();
                return (valid = false);
            }
            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case "--load":
                        i++;
                        file = input[i];
                        load = true;
                        break;

                    case "--mem":
                        i++;
                        memSize = Convert.ToInt32(input[i]);
                        if (memSize > 1048576)
                        {
                            DisplayUsage("Memsize is too large, cannot be over 1048576 bytes.");
                            valid = false;
                        }
                        break;

                    case "--test":
                        test = true;
                        break;
                    case "--exec":
                        exec = true;
                        break;
                    case "--debug":
                        debug = true;
                        break;
                    case "--help":
                    case "-?":
                        DisplayUsage();
                        break;

                    default:
                        //this can be the helper instructions
                        DisplayUsage(input[i] + " is an invalid option.");
                        valid = false;
                        break;
                }
            }
            return valid;
        }

    }

}
