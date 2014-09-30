﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;


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

            Logger.Instance.writeLog("Test: Starting Simulator unit tests");

            Logger.Instance.writeLog("Test: Testing Hash of test1.exe");
            byte[] elfArray = File.ReadAllBytes("test1.exe");
            e.ReadHeader(elfArray);

            comp.writeElfToRam(e, elfArray, ref ram);

            string resultHash = ram.getHash();
            string hash = "3500a8bef72dfed358b25b61b7602cf1";
            Debug.Assert(hash.ToUpper() == resultHash);

            ram.CLEAR();

            Logger.Instance.writeLog("Test: Testing Hash of test2.exe");
            elfArray = File.ReadAllBytes("test2.exe");
            e.ReadHeader(elfArray);
            comp.writeElfToRam(e, elfArray, ref ram);
            resultHash = ram.getHash();
            hash = "0a81d8b63d44a192e5f9f52980f2792e";
            Debug.Assert(hash.ToUpper() == resultHash);

            ram.CLEAR();

            Logger.Instance.writeLog("Test: Testing Hash of test3.exe");
            elfArray = File.ReadAllBytes("test3.exe");
            e.ReadHeader(elfArray);
            comp.writeElfToRam(e, elfArray, ref ram);
            resultHash = ram.getHash();
            hash = "977159b662ac4e450ed62063fba27029";
            Debug.Assert(hash.ToUpper() == resultHash);
            Logger.Instance.writeLog("Test: All Hashes correct\n");

            Logger.Instance.toggleTrace();
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
            tram.SetFlag(0, 4, true);
            flagRes = tram.TestFlag(0, 4);
            Debug.Assert(flagRes == true);
            flagRes = tram.TestFlag(0, 3);
            Debug.Assert(flagRes == false);

            Logger.Instance.writeLog("Test: All Ram Tests passed\n");
            Logger.Instance.closeTrace();
            tram.CLEAR();

        }

    }
}
