using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OpenCvSharp;


namespace server
{
    public partial class Form1 : Form
    {
        Socket server;

        private Dictionary<Socket, int> clientIds = new Dictionary<Socket, int>();
        private int nextClientId = 1;

        List<StreamWriter> clientWriters = new List<StreamWriter>();

        public Form1()
        {
            InitializeComponent();
        }

        private async void listen_Click(object sender, EventArgs e)
        {
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6060);
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(ipep);
                server.Listen(10);

                listen.Enabled = false;

                // Accept incoming connections in a separate thread
                Thread acceptThread = new Thread(() =>
                {
                    while (true)
                    {
                        Socket client = server.Accept();
                        lock (clientIds)
                        {
                            clientIds[client] = nextClientId++;
                        }

                        // Start a new thread to handle client communication
                        Thread receiveThread = new Thread(() => ReceiveMessages(client));
                        receiveThread.Start();
                    }
                });
                acceptThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting server: " + ex.Message);
                listen.Enabled = true;
            }
        }

        private void HandleClient(Socket client)
        {
            try
            {
                // Initialize network stream and writers for the client
                NetworkStream ns = new NetworkStream(client);
                StreamWriter sw = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };
                clientWriters.Add(sw);

                // Start a separate thread to handle client messages
                Thread receiveThread = new Thread(() => ReceiveMessages(client));
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error handling client: " + ex.Message);
            }
        }

        private async void ReceiveMessages(Socket client)
        {
            try
            {
                NetworkStream ns = new NetworkStream(client);
                StreamReader sr = new StreamReader(ns, Encoding.UTF8);
                string tmp;
                int clientId;
                lock (clientIds)
                {
                    clientId = clientIds[client];
                }
                while ((tmp = await sr.ReadLineAsync()) != null)
                {
                    if (tmp.StartsWith("request file: "))
                    {
                        string filePath = tmp.Substring("request file: ".Length).Trim();
                        string fileType = Path.GetExtension(filePath).TrimStart('.');

                        string message = $"receive file ({fileType})";
                        SendMessageToClient(client, message);

                        SendFile(client, filePath);
                    }
                    else if (tmp.StartsWith("request directory: "))
                    {
                        string directoryPath = tmp.Substring("request directory: ".Length).Trim();

                        string message = "receive directory";
                        SendMessageToClient(client, message);

                        SendDirectoryData(client, directoryPath);
                    }
                    else if (tmp.StartsWith("download directory: "))
                    {
                        string directoryPath = tmp.Substring("download directory: ".Length).Trim();

                        string message = "receive download directory";
                        SendMessageToClient(client, message);

                        SendDirectory(client, directoryPath);
                    }
                    else
                    {
                        tmp = $"Client {clientId}: " + tmp;
                        UpdateChat(tmp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving message from client: " + ex.Message);
            }
        }

        private void SendMessageToClient(Socket client, string message)
        {
            try
            {
                NetworkStream ns = new NetworkStream(client);
                StreamWriter sw = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };
                sw.WriteLine(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending message to client: " + ex.Message);
            }
        }

        private void CompressFile(string filePath, string compressedFilePath)
        {
            using (FileStream originalFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (FileStream compressedFileStream = new FileStream(compressedFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        private void SendFile(Socket client, string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // compress the file before send it 
                    string compressedFilePath = filePath + ".gz";
                    CompressFile(filePath, compressedFilePath);

                    // send the compressed file
                    byte[] fileBytes = File.ReadAllBytes(compressedFilePath);
                    NetworkStream ns = new NetworkStream(client);
                    ns.Write(fileBytes, 0, fileBytes.Length);

                    UpdateChat("File sent: " + compressedFilePath);

                    // Clean up the compressed file after sending
                    if (File.Exists(compressedFilePath))
                    {
                        File.Delete(compressedFilePath);
                    }
                }
                else
                {
                    SendMessageToClient(client, "File not found: " + filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending file to client: " + ex.Message);
            }
        }

        private void SendDirectoryData(Socket client, string directoryPath)
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
                client.Send(msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending directory files to client: " + ex.Message);
            }
        }

        private void SendDirectory(Socket client, string directoryPath)
        {
            try
            {
                // Get all files and directories
                string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

                // Send the number of files
                byte[] fileCountBytes = BitConverter.GetBytes(files.Length);
                client.Send(fileCountBytes);

                // Send each file
                foreach (string file in files)
                {
                    // Send relative path
                    string relativePath = Path.GetRelativePath(directoryPath, file);
                    byte[] relativePathBytes = Encoding.UTF8.GetBytes(relativePath);
                    byte[] relativePathLengthBytes = BitConverter.GetBytes(relativePathBytes.Length);
                    client.Send(relativePathLengthBytes);
                    client.Send(relativePathBytes);

                    // Send file size
                    byte[] fileBytes = File.ReadAllBytes(file);
                    byte[] fileSizeBytes = BitConverter.GetBytes(fileBytes.Length);
                    client.Send(fileSizeBytes);

                    // Send file content
                    client.Send(fileBytes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending directory files to client: " + ex.Message);
            }
        }


        private void UpdateChat(string message)
        {
            if (ChatArea.InvokeRequired)
            {
                ChatArea.Invoke((MethodInvoker)(() =>
                {
                    ChatArea.Text += message + Environment.NewLine;
                }));
            }
            else
            {
                ChatArea.Text += message + Environment.NewLine;
            }
        }

        private void SendMessage(string message)
        {
            try
            {
                foreach (var client in clientIds.Keys)
                {
                    SendMessageToClient(client, message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending message to clients: " + ex.Message);
            }
        }

        private void send_Click(object sender, EventArgs e)
        {
            string message = MessageArea.Text;
            if (!string.IsNullOrEmpty(message))
            {
                SendMessage(message);
                UpdateChat("Me (server): " + message);
                MessageArea.Text = "";
            }
        }

        private Socket _server;
        private VideoCapture _capture;
        private readonly List<Socket> _clients = new List<Socket>();

        private void StartVStream()
        {
            // Establish server socket
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 1234); // Listen on any available IP address
                _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _server.Bind(ipep);
                _server.Listen(10);

                listenStr.Enabled = false;

                // Accept client connections asynchronously
                Task.Run(async () =>
                {
                    while (true)
                    {
                        Socket client = await _server.AcceptAsync();
                        _clients.Add(client); // Add client to the list
                        Task.Run(() => HandleVClient(client));
                    }
                });

                // Initialize webcam capture
                _capture = new VideoCapture(0);
                if (!_capture.IsOpened())
                {
                    MessageBox.Show("Failed to open webcam.");
                    return;
                }

                // Start capturing frames and sending them to clients
                Task.Run(CaptureAndSendFrames);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting server or VLC stream: " + ex.Message);
            }
        }

        private async Task CaptureAndSendFrames()
        {
            try
            {
                while (_server.IsBound)
                {
                    Mat frame = new Mat();
                    _capture.Read(frame); // Read a frame from the webcam

                    if (!frame.Empty())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            // Encode frame as JPEG
                            byte[] imageData = frame.ImEncode(".jpg", new ImageEncodingParam(ImwriteFlags.JpegQuality, 100)); // Use 100 as the quality value

                            foreach (Socket client in _clients.ToList())
                            {
                                if (client.Connected)
                                {
                                    await client.SendAsync(imageData, SocketFlags.None); // Send frame to each connected client
                                }
                                else
                                {
                                    _clients.Remove(client); // Remove disconnected client
                                    client.Dispose(); // Dispose the socket
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error capturing and sending frames: " + ex.Message);
            }
        }

        private async void HandleVClient(Socket client)
        {
            try
            {
                // Read client request
                NetworkStream ns = new NetworkStream(client);

                // Continuously capture frames and send them to the client
                while (true)
                {
                    using (Mat frame = new Mat())
                    {
                        if (_capture !=  null) 
                            _capture.Read(frame); // Read a frame from the webcam

                        if (!frame.Empty())
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                // Encode frame as JPEG
                                byte[] imageData = frame.ImEncode(".jpg", new ImageEncodingParam(ImwriteFlags.JpegQuality, 100)); // Use 100 as the quality value

                                // Send frame to the client
                                await ns.WriteAsync(imageData, 0, imageData.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error handling client: " + ex.Message);
            }
            finally
            {
                // Close the connection and release resources
                client.Close();
            }
        }

        private void listenStr_Click(object sender, EventArgs e)
        {
            StartVStream();
        }
    }
}
