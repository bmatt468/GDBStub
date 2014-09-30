﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GDBStub
{
    
    class Handler
    {
        
        public void Listen(int portNo)
        {
            try
            {
                Console.WriteLine("Attempting to open port " + portNo);
                
                // Get the ip address of the local machine
                IPAddress[] ips = Dns.GetHostAddresses("localhost");
                IPAddress localhost = ips[0];
                //for daniel
                //localhost = ips[1];

                // create new socket on the specified port
                TcpListener t = new TcpListener(localhost, portNo);
                // start listener
                t.Start();
                Console.WriteLine("Server started with address {0}:{1}", localhost.ToString(), portNo.ToString());

                // create buffer for reading data
                byte[] buffer = new byte[4096];
                string data;

                // begin listening loop
                while (true)
                {
                    // the following lines accept an incoming connection
                    // (from the gdb client), and creates a stream to accept
                    // data passed to it. It also created an instance of 
                    // the computer class that will be used as the 
                    // simulator for the project
                    TcpClient client = t.AcceptTcpClient();
                    Console.WriteLine("Connection established");
                    NetworkStream ns = client.GetStream();
                    
                    int i = ns.Read(buffer, 0, buffer.Length);
                    while (i != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                        Console.WriteLine(String.Format("Received: {0}", data));

                        // Process the data sent by the client.
                        switch (data)
                        {                            
                            case "+":
                                //byte[] msg = System.Text.Encoding.ASCII.GetBytes("+");
                                //ns.Write(msg, 0, msg.Length);
                                break;

                            case "-":
                                //byte[] msg2 = System.Text.Encoding.ASCII.GetBytes("-");
                                //ns.Write(msg2, 0, msg2.Length);
                                break;

                            default:
                                string command;
                                string checksum;
                                ushort obtainedChecksum = 0;
                                
                                // get checksum
                                checksum = data.Substring(data.IndexOf('#') + 1);
                                data = data.Remove(data.IndexOf('#'));
                                
                                // get command
                                command = data.Substring(data.IndexOf("$") + 1);
                                

                                // obtain checksum
                                foreach (char c in command)
                                {
                                    obtainedChecksum += (ushort)Convert.ToInt16(c);                                   
                                }
                                obtainedChecksum %= 256;
                                
                                // send success packet if checksum matches
                                if (checksum == obtainedChecksum.ToString("x2"))
                                {
                                    byte[] msg3 = System.Text.Encoding.UTF8.GetBytes("+");
                                    ns.Write(msg3, 0, msg3.Length);
                                    Console.WriteLine(String.Format("Sent: {0}", System.Text.Encoding.UTF8.GetString(msg3)));
                                }                                 
                                // send failed packet if checksum doesn't match
                                else
                                {
                                    byte[] msg3 = System.Text.Encoding.UTF8.GetBytes("-");
                                    ns.Write(msg3, 0, msg3.Length);
                                    Console.WriteLine(String.Format("Sent: {0}", System.Text.Encoding.UTF8.GetString(msg3)));
                                    break;
                                }

                                // parse command
                                this.ParseCommand(command, ns);
                                
                                break;
                        }
                        

                        //data Processed
                        i = ns.Read(buffer, 0, buffer.Length);
                    }

                    client.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void ParseCommand(string cmd, NetworkStream ns)
        {            
            switch (cmd)
            {
                // initial phase of handshake
                // server responds with packetsize (in hex)
                // for testing purposes the respone is 0x79 (15 32-bit registers 
                // [requiring 8 bits ea.] + 1 extra bit)
                /*case "qSupported:qRelocInsn+":
                    this.Respond("PacketSize=79", ns);
                    break;*/

                // phase of handshake
                // client informs server about threads
                // server responds with acknowledgement (OK)
                /*case "Hg0":
                    this.Respond("OK", ns);
                    break;*/

                // phase of handshake
                // client asks why thread halted
                // server responds with reason
                // for testing purposes the respone S05 (TRAP Exception)
                /*case "?":
                    this.Respond("S05", ns);
                    break;*/

                // phase of handshake
                // client sets thread
                // server responds with OK
                /*case "Hc-1":
                    this.Respond("OK", ns);
                    break;*/

                // phase of handshake
                // client asks for thread ID
                // server responds with thread ID
                // for testing purposes the response is QC0 
                /*case "qC":
                    this.Respond("QC0", ns);
                    break;*/

                // phase of handshake
                // client asks if new thread was created
                // server responds with whether process is new or existing
                // for testing purposes the response is 0 (new process) 
                /*case "qAttached":
                    this.Respond("0", ns);
                    break;*/
                
                // phase of handshake
                // client asks for state of registers
                // server responds with register state
                // format for response is XX..., where XX is the byte representation of the register
                // for testing purposes the response is all 0s (empty registers)                 
                // computer.dumpRegisters();
                /*case "g":

                    // byte[] of the reg data. will be 64 bytes
                    //Computer.Instance.dumpRegisters();
                    this.Respond(byteArrayToString(Computer.Instance.dumpRegisters(), 64), ns);
                    break;*/
                

                // client asks for state of specific register
                // server responds with register state
                // format for response is XX, where XX... is the byte representation of the register
                // for testing purposes the response is all 0s (empty) 
                /*case "pf":

                    // byte[] of one register, will be 4 bytes
                    this.Respond(byteArrayToString(Computer.Instance.dumpRegister(15), 4), ns);
                    break;*/

                // client asks stub if there is a trace experiment running
                // server respondss with status
                // for test purposes the response it T0 (not running)
                /*case "qTStatus":
                    //returns true or false based ont he tracer.
                    //Computer.Instance.getTraceStatus();
                    this.Respond("T0", ns);
                    break;

                case "qTfV":
                    this.Respond("", ns);
                    break;*/
                    
                default:
                    // handle single character commands
                    char c = cmd[0];
                    switch (c)
                    {
                        // phase of handshake
                        // client asks for state of registers
                        // server responds with register state
                        // format for response is XX..., where XX is the byte representation of the register
                        // for testing purposes the response is all 0s (empty registers)                 
                        // computer.dumpRegisters();
                        case 'g':

                            // byte[] of the reg data. will be 64 bytes
                            //Computer.Instance.dumpRegisters();
                            this.Respond(byteArrayToString(Computer.Instance.dumpRegisters(), 64), ns);
                            break;
                        
                        case 'G':
                            // WRITE GENERAL MEMORY COMMAND
                            Console.WriteLine("General Mem");
                            Console.WriteLine(cmd);
                            break;

                        case 'H':
                            if (cmd == "Hc-1") 
                            { 
                                // phase of handshake
                                // client sets thread
                                // server responds with OK                
                                this.Respond("OK", ns);
                            }
                            else if (cmd == "Hg0") 
                            { 
                                // phase of handshake
                                // client informs server about threads
                                // server responds with acknowledgement (OK)                
                                this.Respond("OK", ns);                   
                            }
                            break;
                        
                        case 'k':
                            // kill client command
                            // done automatically but looking for clean method
                            Console.WriteLine("Kill");
                            break;

                        // client asks for the state of memory
                        // format of command is 'mAddr,length'
                        // NOTE** info will come in in HEX
                        // server responds with 32 bit respnse
                        case 'm':
                            // parse command
                            cmd = cmd.Remove(0, 1);
                            string[] addrAndLength = cmd.Split(',');

                            // get value 
                            uint addr = Convert.ToUInt32(addrAndLength[0], 16);
                            int length = Convert.ToInt32(addrAndLength[1], 16);

                            // returns a byte[] of the RAM from starting address for length bytes
                            // make response
                            this.Respond(byteArrayToString(Computer.Instance.dumpRAM(addr, length), length), ns);

                            break;

                        case 'M':
                            // WRITE AT MEMORY COMMAND
                            Console.WriteLine("Load at mem");
                            Console.WriteLine(cmd);
                            break;

                        case 'p':
                            // READ REGISTER COMMAND
                            // defaults to dump reg #15
                            int regval = int.Parse(cmd.Substring(1),System.Globalization.NumberStyles.HexNumber);                            
                            this.Respond(byteArrayToString(Computer.Instance.dumpRegister(15),4), ns);
                            Console.WriteLine("Print Register");
                            Console.WriteLine(cmd);
                            break;

                        case 'P':
                            // WRITE REGISTER COMMAND
                            // SYNTAX: n...=r...
                            // Computer.Instance.writeRegister(reg#, ammount);

                            Console.WriteLine("Write Register");
                            Console.WriteLine(cmd);
                            break;

                        case 'q':
                            // ask about first client side tracepoint variable                           
                            if (cmd == "qTfP")
                            {
                                this.Respond("", ns);
                            }
                            // ask about more tracepoint variables
                            else if (cmd == "qTsP")
                            {
                                this.Respond("", ns);
                            }
                            else if (cmd == "qAttached")
                            {
                                // phase of handshake
                                // client asks if new thread was created
                                // server responds with whether process is new or existing
                                // for testing purposes the response is 0 (new process)                 
                                this.Respond("0", ns);                   
                            }
                            else if (cmd == "qTStatus")
                            {
                                // client asks stub if there is a trace experiment running
                                // server respondss with status
                                // for test purposes the response it T0 (not running)               
                                //returns true or false based ont he tracer.
                                //Computer.Instance.getTraceStatus();
                                this.Respond("T0", ns);                   
                            }
                            else if (cmd == "qTfV") 
                            {
                                this.Respond("", ns);
                            }
                            else if (cmd == "qC")
                            {
                                // phase of handshake
                                // client asks for thread ID
                                // server responds with thread ID
                                // for testing purposes the response is QC0                
                                this.Respond("QC0", ns);                    
                            }
                            else if (cmd == "qSupported:qRelocInsn+")
                            {
                                // initial phase of handshake
                                // server responds with packetsize (in hex)
                                // for testing purposes the respone is 0x79 (15 32-bit registers 
                                // [requiring 8 bits ea.] + 1 extra bit)                                
                                this.Respond("PacketSize=79", ns);
                            }

                            break;

                        case 's':
                            // SINGLE STEP COMMAND
                            // Computer.Instance.Step();
                            Console.WriteLine("Step");
                            Console.WriteLine(cmd);
                            break;

                        case 'v':
                            if (cmd.StartsWith("vRun"))
                            {
                                // RUN COMMAND
                            }
                            break;
                        
                        case 'X':
                            // WRITE DATA COMMAND
                            // Computer.Instance.writeRAM(addr, byte[]);
                            Console.WriteLine("Write Data Command");
                            Console.WriteLine(cmd);
                            break;

                        case 'z':
                            // REMOVE BREAKPOINT COMMAND
                            Console.WriteLine("Remove Break point");
                            Console.WriteLine(cmd);
                            break;

                        case 'Z':
                            // SET BREAKPOINT COMMAND
                            // Computer.Instance.setBreakPoint(
                            Console.WriteLine("Set BreakPoint");
                            Console.WriteLine(cmd);
                            break;

                        // phase of handshake
                        // client asks why thread halted
                        // server responds with reason
                        // for testing purposes the respone S05 (TRAP Exception)
                        case '?':
                            this.Respond("S05", ns);
                            break;

                        default:
                            this.Respond("", ns);
                            break;
                    }                    
                    break;                   
            }
        }

        private string byteArrayToString(byte[] memory, int length)
        {
            string output = "";
            for (int i = 0; i < length; ++i)
            {
                output += memory[i].ToString().PadLeft(2, '0');
            }
            return output;
        }

        public void Respond(string response, NetworkStream ns)
        {
            //if (response != "")
            //{
                ushort chk = 0;
                foreach (char c in response)
                {
                    chk += (ushort)Convert.ToInt16(c);
                }
                chk %= 256;
                byte[] msg = System.Text.Encoding.UTF8.GetBytes("$" + response + "#" + chk.ToString("x2"));
                ns.Write(msg, 0, msg.Length);
                Console.WriteLine(String.Format("Sent: {0}", System.Text.Encoding.UTF8.GetString(msg)));
            //}
            /*else
            {
                byte[] msg = System.Text.Encoding.UTF8.GetBytes("");
                ns.Write(msg, 0, msg.Length);
                Console.WriteLine(String.Format("Sent: {0}", System.Text.Encoding.UTF8.GetString(msg)));
            }*/
        }
    }
}
