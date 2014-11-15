using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace GDBStub
{
    class Logger
    {
        StreamWriter log, trace;
        bool trace_is_open = false;
        private Object thisLock = new Object();
        public bool lastInstructionWasBranch { get; set; }
        public string useThisValueForBranchedr15 { get; set; }
        //variables that hold references to the regs and RAM

        //CPU instantiation


//Trace Functions
        private static Logger instance;

        private Logger() 
        {
            toggleTrace();
            lastInstructionWasBranch = false;
        }

        
        public static Logger Instance{
            get {
                if (instance == null){
                    instance = new Logger();
                }
            return instance;
            }
        }


        //returns the status fo trace
        public bool getTraceStatus() { return trace_is_open; }

        //switches trace from on->off or off->on
        public bool toggleTrace()
        {
            if (trace_is_open) 
            {
                this.closeTrace();
                this.writeLog("Trace: Closed");
                return false;
            }
            else
            {
                if (!trace_is_open)
                {
                    this.openTrace();
                    this.writeLog("Trace: Opened");
                }
                return true;
            }
        }
        //closes the trace file
        // flushes the buffer out
        //closes for reading
        public void closeTrace() 
        {
            lock (thisLock)
            {
                if (trace_is_open)
                {
                    this.trace.Flush();
                    this.trace.Close();
                }
                this.trace_is_open = false;
            }
        }

        //Opens trace (also clears the file)
        public void openTrace()
        {
            lock (thisLock)
            {
                if (!trace_is_open) //checks to see if it's not opened
                {
                    this.trace = new StreamWriter("trace.log");
                    this.trace_is_open = true;
                }
               // closeTrace();
            }
        }


        // write to the trace
        // formatted to (hopefully) look exactly like the test
        public void writeTrace(Computer comp)
        {
            lock (thisLock)
            {
                if (trace_is_open)
                {
                    string r15 = "";
                    if (lastInstructionWasBranch)
                    {
                        r15 = useThisValueForBranchedr15;
                    }
                    else
                    {
                        r15 = comp.getReg(15).getRegString().ToUpper().Trim();
                    }
                    
                    // build NZCF to ease later code
                    string nzcf = Convert.ToInt32(comp.getFlag('N')).ToString() 
                        + Convert.ToInt32(comp.getFlag('Z')).ToString()
                        + Convert.ToInt32(comp.getFlag('C')).ToString() 
                        + Convert.ToInt32(comp.getFlag('F')).ToString(); 

                    // build trace string and objects
                    object[] objects = { (comp.getStepNumber()).ToString().PadLeft(6, '0') //0
                                           , r15 //1
                                           , "[sys]" //2
                                           , nzcf //3
                                           , "0=" + comp.getReg(0).getRegString().ToUpper().Trim() //4
                                           , "1=" + comp.getReg(1).getRegString().ToUpper().Trim() //5
                                           , "2=" + comp.getReg(2).getRegString().ToUpper().Trim() //6
                                           , "3=" + comp.getReg(3).getRegString().ToUpper().Trim() //7
                                           , Environment.NewLine //8
                                           , "\t" //9
                                           , "4=" + comp.getReg(4).getRegString().ToUpper().Trim() //10
                                           , " 5=" + comp.getReg(5).getRegString().ToUpper().Trim() //11
                                           , " 6=" + comp.getReg(6).getRegString().ToUpper().Trim() //12
                                           , " 7=" + comp.getReg(7).getRegString().ToUpper().Trim() //13
                                           , " 8=" + comp.getReg(8).getRegString().ToUpper().Trim() //14
                                           , "9=" + comp.getReg(9).getRegString().ToUpper().Trim() //15
                                           , "10=" + comp.getReg(10).getRegString().ToUpper().Trim() //16
                                           , " 11=" + comp.getReg(11).getRegString().ToUpper().Trim() //17
                                           , " 12=" + comp.getReg(12).getRegString().ToUpper().Trim() //18
                                           , " 13=" + comp.getReg(13).getRegString().ToUpper().Trim() //19
                                           , " 14=" + comp.getReg(14).getRegString().ToUpper().Trim() //20
                                           , "\t" //21
                                           };
                    // I know I'm going to lose points for this line of code
                    // but I'm honestly just happy that it works
                    this.trace.WriteLine(String.Format(@"{0} {1} {2} {3} {4} {5} {6} {7}{8}{9}{10} {11} {12} {13} {14} {15}{8}{21}{16}{17}{18}{19}{20}", objects));                    
                    this.trace.Flush();
                    lastInstructionWasBranch = false;
                }
            }
        }       

        //Log functions
        public void clearLog()
        {
            log.Close();
            log = new StreamWriter("log.txt");
            log.Close();
        }

        //opens the file for writing. writes to it, and closes.
        internal void writeLog(string p)
        {
           lock(thisLock)
           {
                //if(log == null)
                log = new StreamWriter("log.txt", true);
                DateTime now = DateTime.Now;
                log.WriteLine(now.ToString() + " | " + p);
                log.Close();
            }
        }//write Log


        internal string byteArrayToString(byte[] x)
        {
            string output = "";
            for (int i = 0; i < x.Length; ++i)
            {
                output += Convert.ToString(x[i], 16).PadLeft(2, '0');
            }
            return output;
        }
    }//trace class

}//namespace

