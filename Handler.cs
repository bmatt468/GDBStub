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
                    // data passed to it
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
                                if (checksum == obtainedChecksum.ToString("x"))
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
            ushort chk = 0;
            switch (cmd)
            {
                // initial phase of handshake
                // server responds with packetsize (in hex)
                // for testing purposes the respone is 119 (35 32-bit registers)
                case "qSupported:qRelocInsn+":
                    foreach (char c in "PacketSize=119")
                    {
                        chk += (ushort)Convert.ToInt16(c);
                    }
                    chk %= 256;
                    byte[] msg = System.Text.Encoding.UTF8.GetBytes("$PacketSize=119#" + chk.ToString("x"));
                    ns.Write(msg, 0, msg.Length);
                    Console.WriteLine(String.Format("Sent: {0}", System.Text.Encoding.UTF8.GetString(msg)));
                    break;

                // phase of handshake
                // server responds with packetsize (in hex)
                // for testing purposes the respone is 119 (35 32-bit registers)
                case "Hg0":
                    string response = "OK";
                    foreach (char c in response)
                    {
                        chk += (ushort)Convert.ToInt16(c);
                    }
                    chk %= 256;
                    byte[] msg2 = System.Text.Encoding.UTF8.GetBytes("$" + response + "#" + chk.ToString("x"));
                    ns.Write(msg2, 0, msg2.Length);
                    Console.WriteLine(String.Format("Sent: {0}", System.Text.Encoding.UTF8.GetString(msg2)));
                    break;
                
                default:
                    byte[] msg335 = System.Text.Encoding.ASCII.GetBytes("");
                    ns.Write(msg335, 0, msg335.Length);
                    Console.WriteLine(String.Format("Sent: {0}", System.Text.Encoding.UTF8.GetString(msg335)));
                    break;
            }
        }        
    }
}
