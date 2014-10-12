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
    public class TestSimulator
    {

        public static void RunTests()
        {
            //append

            ELFReader e = new ELFReader();
            Memory ram = new Memory(32768);
            Computer comp = new Computer();
            Logger.Instance.closeTrace();
            Logger.Instance.writeLog("Test: Starting Simulator unit tests");

            Logger.Instance.writeLog("Test: Testing Hash of test1.exe");
            comp.load("test1.exe", 32768);

            string resultHash = comp.getRAM().getHash();
            string hash = "3500a8bef72dfed358b25b61b7602cf1";

            Debug.Assert(hash.ToUpper() == resultHash);
            /*
            Logger.Instance.writeLog("Test: Testing BreakPoint");

            uint bPoint = comp.getReg(15).ReadWord(0) + 8;
            comp.setBreakPoint(bPoint);
            
            comp.step();
            comp.step();
            
            //System.Threading.Thread.Sleep(10000);
            Logger.Instance.writeLog("Test: Hit Break Point");
            Debug.Assert(comp.compStatus.statchar == 'S');
            Debug.Assert(comp.compStatus.statval == "05");
            uint pc = comp.getReg(15).ReadWord(0);
            comp.step();
            Debug.Assert(pc == comp.getReg(15).ReadWord(0));

            comp.removeBreakPoint(bPoint);
            comp.step();
            Debug.Assert(pc < comp.getReg(15).ReadWord(0));
            comp.run();
            while (comp.compStatus.statchar != 'W')
            { ;}

            Debug.Assert(comp.compStatus.statchar == 'W');
            Debug.Assert(comp.compStatus.statval == "00");
            Logger.Instance.writeLog("Test: Removed Break Point");
             */

            comp.CLEAR();

            Logger.Instance.writeLog("Test: Testing Hash of test2.exe");
            comp.load("test2.exe", 32768);
            resultHash = comp.getRAM().getHash();

            hash = "0a81d8b63d44a192e5f9f52980f2792e";

            Debug.Assert(hash.ToUpper() == resultHash);
            

            comp.CLEAR();

            Logger.Instance.writeLog("Test: Testing Hash of test3.exe");
            comp.load("test3.exe", 32768);


            resultHash = comp.getRAM().getHash();
            hash = "977159b662ac4e450ed62063fba27029";

            Debug.Assert(hash.ToUpper() == resultHash);
            
            Logger.Instance.writeLog("Test: All Hashes correct\n");

            //Logger.Instance.toggleTrace();
            comp.reset();

        }

    }

    public class TestRam
    {

        public static void RunTests()
        {

            //append
            Logger.Instance.writeLog("Test: Starting RAM unit tests");
            Memory tram = new Memory(32768);
            Logger.Instance.closeTrace();

            Logger.Instance.writeLog("Test: Read/Write Byte");
            byte byteRes = tram.ReadByte(0);
            Debug.Assert(byteRes == 0);
            tram.WriteByte(0, 0xee);
            byteRes = tram.ReadByte(0);
            Debug.Assert(byteRes == 0xee);

            tram.CLEAR();

            Logger.Instance.writeLog("Test: Read/Write HalfWord");
            ushort shortRes = tram.ReadHalfWord(0);
            Debug.Assert(shortRes == 0);
            tram.WriteHalfWord(0, 0xeef);
            shortRes = tram.ReadHalfWord(0);
            Debug.Assert(shortRes == 0xeef);

            tram.CLEAR();

            Logger.Instance.writeLog("Test: Read/Write Word");
            uint intRes = tram.ReadWord(0);
            Debug.Assert(intRes == 0);
            tram.WriteWord(0, 0xabcdef);
            intRes = tram.ReadWord(0);
            Debug.Assert(intRes == 0xabcdef);

            tram.CLEAR();

            Logger.Instance.writeLog("Test: Set/Test Flag");

            bool flagRes = tram.TestFlag(0, 4);
            Debug.Assert(flagRes == false);

            //set a false flag true
            tram.SetFlag(0, 4, true);
            flagRes = tram.TestFlag(0, 4);
            Debug.Assert(flagRes == true);

            // true >> true
            tram.SetFlag(0, 4, true);
            flagRes = tram.TestFlag(0, 4);
            Debug.Assert(flagRes == true);

            // true >> false
            tram.SetFlag(0, 4, false);
            flagRes = tram.TestFlag(0, 4);
            Debug.Assert(flagRes == false);

            // false >> false
            tram.SetFlag(0, 4, false);
            flagRes = tram.TestFlag(0, 4);
            Debug.Assert(flagRes == false);

            Logger.Instance.writeLog("Test: All Ram Tests passed\n");
            Logger.Instance.closeTrace();
            tram.CLEAR();

        }

    }


    public class TestDecodeExecute
    {

        Memory RAM = new Memory();
        Register[] reg = new Register[16];
        CPU cpu;
        public void RunTests()
                {
                    //append
                    Logger.Instance.writeLog("Test: Starting Decode Execute unit tests");
            
                    Logger.Instance.closeTrace();
                    //0xe3a02030 mov r2, #48
                    //defines 16 registers, 0 - 15
                    for (int i = 0; i < 16; i++)
                    {
                       reg[i] = new Register();
                    }

                    cpu = new CPU(ref RAM, ref reg);
                    
                    //put the instruction into memory

                    Logger.Instance.writeLog("TEST: mov r2, #48 : 0xe3a02030");
                    this.runDataManCommand(0xe3a02030);

                    Debug.Assert(reg[2].ReadWord(0) == 48);
                    Logger.Instance.writeLog("TEST: Executed");


                    Logger.Instance.writeLog("TEST: mov r0, r3 : 0xe1a00003");
                    reg[3].WriteWord(0, 3);

                    this.runDataManCommand(0xe1a00003);
                    
                    Debug.Assert(reg[0].ReadWord(0) == 3);
                    Logger.Instance.writeLog("TEST: Executed");

                    Logger.Instance.writeLog("TEST: mov r0, r3 lsl #4 : 0xe1a00403");
                    reg[3].WriteWord(0, 3);
        
                    this.runDataManCommand(0xe1a00403);
                  
                    Debug.Assert(reg[0].ReadWord(0) == 0x300);
                    Logger.Instance.writeLog("TEST: Executed");



                    Logger.Instance.writeLog("TEST: All Decode/Execute Tests Passed");

                    Logger.Instance.closeTrace();
 
                }

        private void runDataManCommand(uint p)
        {
            RAM.WriteWord(0, p);

            //get the program counter to point at the test command
            reg[15].WriteWord(0, 0);

            //fetch, decode, execute commands here
            Memory rawInstruction = cpu.fetch();

            /// make sure we fetched the right hting
            Debug.Assert(rawInstruction.ReadWord(0) == p);

            Logger.Instance.writeLog("TEST: Fetched");


            //decode the uint!
            Instruction cookedInstruction = cpu.decode(rawInstruction);
            Debug.Assert(cookedInstruction is dataManipulation);
            Logger.Instance.writeLog("TEST: Decoded");

            //exeucte the decoded Command!!
            cpu.execute(cookedInstruction);

        }//runTests

    }//testDecodeExecute

}//namespace
