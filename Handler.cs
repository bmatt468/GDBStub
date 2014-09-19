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
    }
}
