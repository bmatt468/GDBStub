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
        //variables that hold references to the regs and RAM

        //CPU instantiation


//Trace Functions
        private static Logger instance;

        private Logger() 
        {
            toggleTrace();
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
        public void toggleTrace()
        {
            if (trace_is_open) 
            {
                this.closeTrace();
                this.writeLog("Trace: Closed");
            }
            else
            {
                if (!trace_is_open)
                {
                    this.openTrace();
                    this.writeLog("Trace: Opened");
                }
            }
        }
        //closes the trace file
        // flushes the buffer out
        //closes for reading
        public void closeTrace() 
        {
            if (trace_is_open)
            {
                this.trace.Flush();
                this.trace.Close();
            }
            this.trace_is_open = false;
        }

        //Opens trace (also clears the file)
        public void openTrace()
        {
            closeTrace();
            this.trace = new StreamWriter("trace.log");
            this.trace_is_open = true;
        }

        
        // if enabled will write to the trace.log file
        // to keep a trace
        public void writeTrace(Computer  myComp)
        {
            if (trace_is_open)
            {
                //step_number program_counter checksum nzcf r0 r1 r2 r3
                this.trace.WriteLine(myComp.getStepNumber().ToString().PadLeft(6, '0') + ' ' +
                                myComp.getReg(15).getRegString() + ' ' +
                                myComp.getCheckSum() + ' ' +
                                Convert.ToInt32(myComp.getN()) + Convert.ToInt32(myComp.getZ()) +
                                Convert.ToInt32(myComp.getC()) + Convert.ToInt32(myComp.getF()) + ' ' +
                                "0=" + myComp.getReg(0).getRegString() + ' ' +
                                "1=" + myComp.getReg(1).getRegString() + ' ' +
                                "2=" + myComp.getReg(2).getRegString() + ' ' +
                                "3=" + myComp.getReg(3).getRegString());

                //r4 r5 r6 r7 r8 r9
                this.trace.WriteLine("4=" + myComp.getReg(4).getRegString() + ' ' +
                                "5=" + myComp.getReg(5).getRegString() + ' ' +
                                "6=" + myComp.getReg(6).getRegString() + ' ' +
                                "7=" + myComp.getReg(7).getRegString() + ' ' +
                                "8=" + myComp.getReg(8).getRegString() + ' ' +
                                "9=" + myComp.getReg(9).getRegString());
                //r10 r11 r12 r13 r14
                this.trace.WriteLine("10=" + myComp.getReg(10).getRegString() + ' ' +
                                "11=" + myComp.getReg(11).getRegString() + ' ' +
                                "12=" + myComp.getReg(12).getRegString() + ' ' +
                                "13=" + myComp.getReg(13).getRegString() + ' ' +
                                "14=" + myComp.getReg(14).getRegString());
                this.trace.Flush();
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
            try
            {
                log = new StreamWriter("log.txt", true);
                log.WriteLine(p);
                
            }
            catch { Console.WriteLine(p); }
        }

    }

}

