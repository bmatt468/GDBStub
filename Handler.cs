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


        Computer comp;
        public void Listen()
        {

            /*tons of gdb logic
             * 
             * 
             * 
             * 
             * 
             * 
             */
            //you can do what you want with this was using it to test my 
            //thread logic.
            comp = new Computer();
            if (Option.Instance.getFile() != "")
            {
                //specified through command line, load the file
                comp.readELF(Option.Instance.getFile(), Option.Instance.getMemSize());
            }
            Console.WriteLine("\n");
            Console.Write("Please input a command: ");
            string input = Console.ReadLine();

            while (input != "")
            {
                string result = comp.command(input);
                Console.WriteLine(result);
                Console.Write("\nPlease input a command: ");
                //Run, Step, Stop/Break, and Reset
                input = Console.ReadLine();
            }

        }

        public void Listen2(int portNo)
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
