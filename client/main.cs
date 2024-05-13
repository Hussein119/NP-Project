using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

class BidirectionalClient
{
    private static Socket sender;

    public static void Main()
    {
        StartClient();
    }

    public static void StartClient()
    {
        Console.WriteLine("You are by default in a real time chat and you can run any of this commands in the chat:\n 1- if you want to get a file from the server enter: request file\n 2- if you want to get a video from the server enter: request video\n 3- if you want to get a image from the server enter: request image\n");

        try
        {
            // Connect to the server
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 9050);

            sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);

            Console.WriteLine("Connected to server: " + sender.RemoteEndPoint);

            // Start a new thread for receiving messages
            Thread receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();

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
                int bytesRec = sender.Receive(bytes);
                string message = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (message == "receive video")
                {
                    ReceiveVideo();
                }
                if (message == "receive file")
                {
                    ReceiveFile();
                }
                if (message == "receive image")
                {
                    ReceiveImage();
                }
                Console.WriteLine("Received from server: " + message);
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Server disconnected.");
        }
    }
    public static void ReceiveVideo()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = sender.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
            {
                using (FileStream fs = new FileStream("received_video.mp4", FileMode.Append, FileAccess.Write))
                {
                    fs.Write(buffer, 0, bytesRead);
                }
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Server disconnected.");
        }
    }
    public static void ReceiveFile()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = sender.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
            {
                using (FileStream fs = new FileStream("received_file.txt", FileMode.Append, FileAccess.Write))
                {
                    fs.Write(buffer, 0, bytesRead);
                }
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Server disconnected.");
        }
    }
    public static void ReceiveImage()
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = sender.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
            {
                using (FileStream fs = new FileStream("received_image.png", FileMode.Append, FileAccess.Write))
                {
                    fs.Write(buffer, 0, bytesRead);
                }
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Server disconnected.");
        }
    }
    public static void SendMessage(string message)
    {
        byte[] msg = Encoding.ASCII.GetBytes(message);
        sender.Send(msg);
    }
}