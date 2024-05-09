using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class BidirectionalServer
{
    private static Socket clientSocket;

    public static void Main()
    {
        StartServer();
    }

    public static void StartServer()
    {
        // Establish the local endpoint for the socket.
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 9050);

        // Create a TCP/IP socket.
        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            // Bind the socket to the local endpoint and listen for incoming connections.
            listener.Bind(localEndPoint);
            listener.Listen(10);

            Console.WriteLine("Server started, waiting for a connection...");

            // Accept incoming connections in a separate thread
            Thread acceptThread = new Thread(() =>
            {
                while (true)
                {
                    clientSocket = listener.Accept();
                    Console.WriteLine("Connected to client: " + clientSocket.RemoteEndPoint);

                    // Start a new thread to handle client communication
                    Thread receiveThread = new Thread(ReceiveMessage);
                    receiveThread.Start();
                }
            });
            acceptThread.Start();

            // Main thread for sending messages
            while (true)
            {
                string message = Console.ReadLine();
                SendMessage(message);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static void ReceiveMessage()
    {
        try
        {
            while (true)
            {
                byte[] bytes = new byte[1024];
                int bytesRec = clientSocket.Receive(bytes);
                string message = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine("Received from client: " + message);
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Client disconnected.");
        }
    }

    public static void SendMessage(string message)
    {
        byte[] msg = Encoding.ASCII.GetBytes(message);
        clientSocket.Send(msg);
    }
}
