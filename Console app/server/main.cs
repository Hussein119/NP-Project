using System;
using System.Net;
using System.IO;
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
                switch (message)
                {
                    case "request dirs":
                        SendMessage("Enter the path you want: ");
                        byte[] bytesForDir = new byte[1024];
                        int bytesRecForDir = clientSocket.Receive(bytesForDir);
                        string directoryPath = Encoding.ASCII.GetString(bytesForDir, 0, bytesRecForDir);
                        SendDirectoryListing(directoryPath);
                        break;
                    case "request file":
                        SendMessage("Enter the path of a file you want: ");
                        byte[] bytesForFile = new byte[1024];
                        int bytesRecForFile = clientSocket.Receive(bytesForFile);
                        string filePath = Encoding.ASCII.GetString(bytesForFile, 0, bytesRecForFile);
                        SendMessage("receive file");
                        SendFile(filePath);
                        break;
                    case "request video":
                        SendMessage("Enter the path of a file you want: ");
                        byte[] bytesForVid = new byte[1024];
                        int bytesRecForVid = clientSocket.Receive(bytesForVid);
                        string directoryVideoPath = Encoding.ASCII.GetString(bytesForVid, 0, bytesRecForVid);
                        SendMessage("receive video");
                        SendVideo(directoryVideoPath);
                        break;
                    case "request image":
                        SendMessage("Enter the path of a file you want: ");
                        byte[] bytesForImg = new byte[1024];
                        int bytesRecForImg = clientSocket.Receive(bytesForImg);
                        string directoryImagePath = Encoding.ASCII.GetString(bytesForImg, 0, bytesRecForImg);
                        SendMessage("receive image");
                        SendImage(directoryImagePath);
                        break;
                    default:
                        Console.WriteLine("Received from client: " + message);
                        break;
                }
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Client disconnected.");
        }
    }

    public static void SendDirectoryListing(string directoryPath)
    {
        try
        {
            string[] files = Directory.GetFiles(directoryPath);
            string[] directories = Directory.GetDirectories(directoryPath);

            StringBuilder responseBuilder = new StringBuilder();

            responseBuilder.AppendLine("Files:");
            foreach (string file in files)
            {
                responseBuilder.AppendLine(Path.GetFileName(file));
            }

            responseBuilder.AppendLine("Directories:");
            foreach (string dir in directories)
            {
                responseBuilder.AppendLine(Path.GetFileName(dir) + "/");
            }

            string response = responseBuilder.ToString();
            byte[] msg = Encoding.ASCII.GetBytes(response);
            clientSocket.Send(msg);
        }
        catch (Exception ex)
        {
            string errorMessage = "Error: " + ex.Message;
            byte[] errorMsg = Encoding.ASCII.GetBytes(errorMessage);
            clientSocket.Send(errorMsg);
        }
    }

    public static void SendFile(string filePath)
    {
        try
        {
            // Check if the file exists
            if (File.Exists(filePath))
            {
                // // Read the contents of the file
                // byte[] fileData = File.ReadAllBytes(filePath);

                // // Send the file data to the client
                // clientSocket.Send(fileData);

                // Console.WriteLine("File sent successfully.");

                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        clientSocket.Send(buffer, 0, bytesRead, SocketFlags.None);
                    }
                }
            }
            else
            {
                SendMessage("File not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending file: " + ex.Message);
        }
    }

    public static void SendVideo(string videoFilePath)
    {
        try
        {
            using (FileStream fs = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    clientSocket.Send(buffer, 0, bytesRead, SocketFlags.None);
                }
            }
            Console.WriteLine("Video streaming completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error streaming video: " + ex.Message);
        }
    }

    public static void SendImage(string imageFilePath)
    {
        try
        {
            using (FileStream fs = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    clientSocket.Send(buffer, 0, bytesRead, SocketFlags.None);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending image: " + ex.Message);
        }
    }

    public static void SendMessage(string message)
    {
        byte[] msg = Encoding.ASCII.GetBytes(message);
        clientSocket.Send(msg);
    }
}