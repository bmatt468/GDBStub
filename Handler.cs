using System;
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
                localhost = ips[1];

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
                                
                                // send failed packet if checksum doesn't match
                                if (checksum != obtainedChecksum.ToString("x2"))
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
                    // Computer.Instance.writeRAM(uint addr, byte[] x ) 

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
                    System.Environment.Exit(0);
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
                    char[] ca = {',', ':'};
                    string[] sa = cmd.Substring(1).Split(ca);
                    byte[] ba = new byte[Convert.ToInt32(sa[1])];
                    string bigLongUselessString = sa[2];
                    // build byte array
                    for (int i = 0; i < ba.Length; ++i)
                    {
                        string toBeHexified = bigLongUselessString.Substring(0, 2);
                        byte b = Convert.ToByte(toBeHexified, 16);
                        ba[i] = b;
                        bigLongUselessString = bigLongUselessString.Remove(0, 2);
                    }               
                        // WRITE TO RAM
                        Computer.Instance.writeRAM((uint)Convert.ToInt32(sa[0]), ba);
                    //check the log.txt
                        // Print it out, Prove it worked! and it does
                        Logger.Instance.writeLog(
                            Computer.Instance.getRAM().getAtAddress(
                                (uint)Convert.ToInt32(sa[0]) - (uint)Convert.ToInt32(sa[1]), 10));

                        //tell the gdb server we got the instruction and read it.
                        this.Respond("OK", ns);
                    break;

                case 'p':
                    // READ REGISTER COMMAND
                    // defaults to dump reg #15
                    int regval = int.Parse(cmd.Substring(1),System.Globalization.NumberStyles.HexNumber); 
                    //change 15 to the register specified       
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
                    this.Respond("", ns);
                    break;

                case 'z':
                    // REMOVE BREAKPOINT COMMAND
                    // public void removeBreakPoint(uint addr)
                    Console.WriteLine("Remove Break point");
                    Console.WriteLine(cmd);
                    break;

                case 'Z':
                    // SET BREAKPOINT COMMAND
                    // public void setBreakPoint(uint addr, ushort immed = 0)
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
                byte[] msg = System.Text.Encoding.UTF8.GetBytes("+$" + response + "#" + chk.ToString("x2"));
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
