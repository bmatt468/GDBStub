using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;


namespace GDBStub
{
    class TestSimulator
    {
        public ELFReader e { get; set; }
        public Memory mem { get; set; }
        public Computer comp { get; set; }
        public Register[] reg { get; set; }
        public CPU cpu { get; set; }
        public string resultHash { get; set; }
        public string hash { get; set; }

        public TestSimulator()
        {
            e = new ELFReader();
            mem = new Memory(32768);
            comp = new Computer();
            reg = new Register[16];
            resultHash = "";
            hash = "";
        }
        
        public void TestFail(string msg) 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Logger.Instance.writeLog(msg);
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public void TestWarn(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Logger.Instance.writeLog(msg);
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public void TestPass(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Logger.Instance.writeLog(msg);
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public void Output(string msg)
        {
            Logger.Instance.writeLog(msg);
            Console.WriteLine(msg);
        }

        private void TestCommand(uint bytes)
        {
            mem.WriteWord(0, bytes);
            reg[15].WriteWord(0, 0);
            Memory m = cpu.Fetch();
            Debug.Assert(m.ReadWord(0) == bytes);
            if (!(m.ReadWord(0) == bytes))
            {
                this.TestFail("ERROR: Fetch failed");
            }
            else 
            {
                this.TestPass("Fetch Passed");
            }
            Instruction i = cpu.Decode(m);
            bool[] flags = { false, false, false, false };
            cpu.Execute(i, flags);
        }

        public void IFTHISTHINGWORKS()
        {
            this.Output("_____Beginning Tests_____");
            this.Output("Console Running in Verbatim Mode for Unit Tests");
            this.Output("Beginning Hash Test");
            
            this.Output("Testing hash of test1.exe");
            try
            {
                comp.load("test1.exe", 32768);
                resultHash = comp.getRAM().getHash();
                hash = "3500a8bef72dfed358b25b61b7602cf1";

                if (hash.ToUpper() == resultHash)
                {
                    this.TestPass("test1.exe hash passed");
                }
                else
                {
                    this.TestFail("ERROR: test1.exe hash failed");
                }
            }
            catch (FileNotFoundException)
            {
                this.TestWarn("WARNING: test1.exe not found. Test bypassed");
            }            
            comp.CLEAR();
            //
            this.Output("Testing hash of test2.exe");
            try
            {
                comp.load("test2.exe", 32768);
                resultHash = comp.getRAM().getHash();
                hash = "0a81d8b63d44a192e5f9f52980f2792e";
                if (hash.ToUpper() == resultHash)
                {
                    this.TestPass("test2.exe hash passed");
                }
                else
                {
                    this.TestFail("ERROR: test2.exe hash failed");
                }
            }
            catch (Exception)
            {
                this.TestWarn("WARNING: test2.exe not found. Test bypassed");
            }            
            comp.CLEAR();
            //
            this.Output("Testing hash of test3.exe");
            try
            {
                comp.load("test3.exe", 32768);
                resultHash = comp.getRAM().getHash();
                hash = "977159b662ac4e450ed62063fba27029";

                if (hash.ToUpper() == resultHash)
                {
                    this.TestPass("test2.exe hash passed");
                }
                else
                {
                    this.TestFail("ERROR: test3.exe hash failed");
                }
            }
            catch (Exception)
            {
                this.TestWarn("WARNING: test3.exe not found. Test bypassed");
            }
            comp.CLEAR();
            this.Output("End hash test");
            
            this.Output("Beginning RAM tests");
            this.Output("Testing Read/Write Byte");
            byte bytetest = mem.ReadByte(0);
            if (!(bytetest == 0))
            {
                this.TestFail("ERROR: Read/Write Byte fail");
            }
            mem.WriteByte(0, 0xfd);
            bytetest = mem.ReadByte(0);
            if (!(bytetest == 0xfd))
            {
                this.TestFail("ERROR: Read/Write Byte fail");
            }
            else
            {
                this.TestPass("Read/Write Byte passed");
            }
            mem.CLEAR();

            this.Output("Testing Read/Write HalfWord");
            ushort shorttest = mem.ReadHalfWord(0);            
            if (!(shorttest == 0))
            {
                this.TestFail("ERROR: Read/Write HalfWord fail");
            }
            mem.WriteHalfWord(0, 0xabc);
            shorttest = mem.ReadHalfWord(0);
            if (!(shorttest == 0xabc))
            {
                this.TestFail("ERROR: Read/Write HalfWord fail");
            }
            else
            {
                this.TestPass("Read/Write HalfWord passed");
            }
            mem.CLEAR();

            this.Output("Testing Read/Write Word");
            uint inttest = mem.ReadWord(0);
            if (!(inttest == 0))
            {
                this.TestFail("ERROR: Read/Write Word fail");
            }
            mem.WriteWord(0, 0xabc123);
            inttest = mem.ReadWord(0);
            if (!(inttest == 0xabc123))
            {
                this.TestFail("ERROR: Read/Write Word fail");
            }
            else
            {
                this.TestPass("Read/Write Word passed");
            }
            mem.CLEAR();            
            
            this.Output("Testing Set/Test Flag");

            bool flagtest = mem.TestFlag(0, 4);            
            if (!(flagtest == false))
            {
                this.TestFail("ERROR: Flag test failed");
            }
            else
            {
                this.TestPass("Flag test test passed");
            }

            //false >> true
            mem.SetFlag(0, 4, true);
            flagtest = mem.TestFlag(0, 4);
            if (!(flagtest == true))
            {
                this.TestFail("ERROR: Flag test failed");
            }
            else
            {
                this.TestPass("Flag test test passed");
            }

            // true >> true
            mem.SetFlag(0, 4, true);
            flagtest = mem.TestFlag(0, 4);
            if (!(flagtest == true))
            {
                this.TestFail("ERROR: Flag test failed");
            }
            else
            {
                this.TestPass("Flag test test passed");
            }

            // true >> false
            mem.SetFlag(0, 4, false);
            flagtest = mem.TestFlag(0, 4);
            if (!(flagtest == false))
            {
                this.TestFail("ERROR: Flag test failed");
            }
            else
            {
                this.TestPass("Flag test test passed");
            }

            // false >> false
            mem.SetFlag(0, 4, false);
            flagtest = mem.TestFlag(0, 4);
            if (!(flagtest == false))
            {
                this.TestFail("ERROR: Flag test failed");
            }
            else
            {
                this.TestPass("Flag test test passed");
            }

            this.Output("All Ram Tests completed");
            Logger.Instance.closeTrace();
            mem.CLEAR();

            this.Output("Starting CPU unit tests");            
            //0xe3a02030 mov r2, #48
            //defines 16 registers, 0 - 15
            for (int i = 0; i < 16; i++)
            {
                reg[i] = new Register();
            }
            // create cpu
            cpu = new CPU(mem, reg);
            // build instruction
            this.Output("Test Command: MOV R1,4016 : 0xe3a01efb");
            this.TestCommand(0xe3a01efb);
            if (!(reg[1].ReadWord(0) == 4016))
            {
                this.TestFail("ERROR: MOV Execution failed");
            }
            else
            {
                this.TestPass("MOV Execution passed");
            }

            this.Output("Test Command: ADD r10, r3, #9 : 0xe283A009");
            reg[3].WriteWord(0, 1);
            this.TestCommand(0xe283A009);
            if (!(reg[10].ReadWord(0) == 10))
            {
                this.TestFail("ERROR: ADD Execution failed");
            }
            else
            {
                this.TestPass("ADD Execution passed");
            }

            this.Output("Test Command: SUB r1, r10, #3 : 0xe24A1003");
            this.TestCommand(0xe24A1003);
            if (!(reg[1].ReadWord(0) == 7))
            {
                this.TestFail("ERROR: SUB Execution failed");
            }
            else
            {
                this.TestPass("SUB Execution passed");
            }
            this.Output("All Decode/Execute Tests Finished"); 
        }
    }    
}
