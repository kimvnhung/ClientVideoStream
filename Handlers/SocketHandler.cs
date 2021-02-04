using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientVideoStream.Handlers
{
    class SocketHandler
    {
        internal static Socket ConnectSocket(string server,int port)
        {
            Socket s = null;
            IPHostEntry hostEntry = null;

            // Get host related information.
            hostEntry = Dns.GetHostEntry(server);

            // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            // an exception that occurs when the host IP Address is not compatible with the address family
            // (typical in the IPv6 case).
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket =
                    new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if (tempSocket.Connected)
                {
                    s = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return s;
        }

        internal static bool SendData(string server,int port,byte[] data)
        {
            string request = "GET / HTTP/1.1\r\nHost: " + server +
            "\r\nConnection: Close\r\n\r\n";
            Byte[] bytesSent = Encoding.ASCII.GetBytes(request);
            //Byte[] bytesReceived = new Byte[256];
            //string page = "";

            // Create a socket connection with the specified server and port.
            using (Socket s = ConnectSocket(server, port))
            {

                if (s == null)
                {
                    Console.WriteLine("Connection failed");
                    return false;
                }
                    
                // Send request to the server.
                s.Send(bytesSent, bytesSent.Length, 0);
                return true;
            }
        }

        internal static byte[] ReceiveData(string server,int port)
        {
            Byte[] bytesReceived = new Byte[256];
            string page = "";

            // Create a socket connection with the specified server and port.
            using (Socket s = ConnectSocket(server, port))
            {

                if (s == null)
                {
                    Console.WriteLine("Connection failed");
                    return null;
                }

                // Receive the server home page content.
                int bytes = 0;
                page = "Default HTML page on " + server + ":\r\n";

                // The following will block until the page is transmitted.
                bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
            }

            return bytesReceived;
        }
    }
}
