using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace client
{
    public partial class Form1 : Form
    {
        Socket server;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;

        public Form1()
        {
            InitializeComponent();
        }

        private async void connect_Click(object sender, EventArgs e)
        {
            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6060);
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                connect.Enabled = false;

                await Task.Run(() => server.Connect(ipep));
                ns = new NetworkStream(server);
                sw = new StreamWriter(ns);
                sr = new StreamReader(ns, Encoding.UTF8);

                ReceiveMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to server: " + ex.Message);
                connect.Enabled = true;
            }
        }

        private async void ReceiveMessages()
        {
            try
            {
                string tmp;
                while ((tmp = await sr.ReadLineAsync()) != null)
                {
                    if (tmp.StartsWith("receive file"))
                    {
                        string message = tmp.Substring("receive file".Length).Trim();
                        // Extract the file type if it exists
                        string fileType = "";
                        if (message.StartsWith("(") && message.Contains(")"))
                        {
                            int startIndex = message.IndexOf("(") + 1;
                            int endIndex = message.IndexOf(")");
                            fileType = message.Substring(startIndex, endIndex - startIndex).ToLower().Trim();
                        }
                        ReceiveFile(fileType);
                    }
                    else if (tmp.StartsWith("receive directory"))
                    {
                        ReceiveDirectory();
                    }
                    else if (tmp.StartsWith("receive download directory"))
                    {
                        ReceiveDownloadDirectory();
                    }
                    else
                    {
                        tmp = "Server: " + tmp;
                        UpdateChat(tmp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving message from server: " + ex.Message);
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

        private void UpdateDirectoryScreen(string message)
        {
            if (directoryScreen.InvokeRequired)
            {
                directoryScreen.Invoke((MethodInvoker)(() =>
                {
                    directoryScreen.Text += message + Environment.NewLine;
                }));
            }
            else
            {
                directoryScreen.Text += message + Environment.NewLine;
            }
        }

        private void SendMessage(string message)
        {
            try
            {
                if (sw != null)
                {
                    sw.WriteLine(message);
                    sw.Flush();
                }
                else
                {
                    MessageBox.Show("StreamWriter is not initialized.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending message to server: " + ex.Message);
            }
        }

        private void send_Click(object sender, EventArgs e)
        {
            string message = MessageArea.Text;
            if (!string.IsNullOrEmpty(message))
            {
                SendMessage(message);
                message = "Me (client): " + message;
                UpdateChat(message);
                MessageArea.Text = "";
            }
        }

        private void DecompressFile(string compressedFilePath, string decompressedFilePath)
        {
            using (FileStream compressedFileStream = new FileStream(compressedFilePath, FileMode.Open, FileAccess.Read))
            {
                using (FileStream decompressedFileStream = new FileStream(decompressedFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (GZipStream decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }
                }
            }
        }

        private void ReceiveFile(string type)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                string compressedFilePath = "received_file." + type + ".gz";
                string decompressedFilePath = "received_file." + type;

                using (FileStream fs = new FileStream(compressedFilePath, FileMode.Create, FileAccess.Write))
                {
                    while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                        if (!ns.DataAvailable)
                        {
                            // No more data available, assume end of file
                            break;
                        }
                    }
                }

                // Decompress the received file
                DecompressFile(compressedFilePath, decompressedFilePath);

                UpdateChat("File received and decompressed: " + decompressedFilePath);
                MessageBox.Show("File received and decompressed");

                // Clean up the compressed file after decompressing
                if (File.Exists(compressedFilePath))
                {
                    File.Delete(compressedFilePath);
                }

                // if the file is an image show the image in pictureBox1
                if (type == "jpg" || type == "jpeg" || type == "png")
                {
                    using (FileStream imageStream = new FileStream(decompressedFilePath, FileMode.Open, FileAccess.Read))
                    {
                        Image image = Image.FromStream(imageStream);
                        pictureBox1.Invoke((MethodInvoker)delegate
                        {
                            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                            pictureBox1.Image = image;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving the file: " + ex.Message);
            }
        }

        private void ReceiveDirectory()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = ns.Read(buffer, 0, buffer.Length);
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                UpdateDirectoryScreen(receivedData);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving directory information: " + ex.Message);
            }
        }
        private void ReceiveDownloadDirectory()
        {
            string targetPath = "D:\\Collage\\8th\\Network Programming\\Project\\GUI app\\client\\client\\bin\\Debug\\net8.0-windows";
            
            try
            {
                // Receive the number of files
                byte[] fileCountBytes = new byte[4];
                int receivedBytes = server.Receive(fileCountBytes);
                if (receivedBytes == 0)
                {
                    throw new Exception("No data received for file count.");
                }
                int fileCount = BitConverter.ToInt32(fileCountBytes, 0);

                for (int i = 0; i < fileCount; i++)
                {
                    // Receive relative path length
                    byte[] relativePathLengthBytes = new byte[4];
                    receivedBytes = server.Receive(relativePathLengthBytes);
                    if (receivedBytes == 0)
                    {
                        throw new Exception("No data received for relative path length.");
                    }
                    int relativePathLength = BitConverter.ToInt32(relativePathLengthBytes, 0);

                    // Receive relative path
                    byte[] relativePathBytes = new byte[relativePathLength];
                    receivedBytes = server.Receive(relativePathBytes);
                    if (receivedBytes == 0)
                    {
                        throw new Exception("No data received for relative path.");
                    }
                    string relativePath = Encoding.UTF8.GetString(relativePathBytes);

                    // Sanitize the relative path
                    relativePath = relativePath.Replace("/", "\\");
                    relativePath = relativePath.TrimEnd('\\');

                    // Ensure the relative path does not contain invalid characters
                    foreach (char invalidChar in Path.GetInvalidPathChars())
                    {
                        relativePath = relativePath.Replace(invalidChar, '_');
                    }

                    // Receive file size
                    byte[] fileSizeBytes = new byte[4];
                    receivedBytes = server.Receive(fileSizeBytes);
                    if (receivedBytes == 0)
                    {
                        throw new Exception("No data received for file size.");
                    }
                    int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

                    // Receive file content
                    byte[] fileBytes = new byte[fileSize];
                    int totalReceived = 0;
                    while (totalReceived < fileSize)
                    {
                        receivedBytes = server.Receive(fileBytes, totalReceived, fileSize - totalReceived, SocketFlags.None);
                        if (receivedBytes == 0)
                        {
                            throw new Exception("No data received for file content.");
                        }
                        totalReceived += receivedBytes;
                    }

                    // Recreate the directory structure
                    string fullPath = Path.Combine(targetPath, relativePath);
                    string directory = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Write the file content
                    File.WriteAllBytes(fullPath, fileBytes);
                }

                UpdateChat("Files successfully received: open this client\\client\\bin\\Debug\\net8.0-windows to see it");
                MessageBox.Show("Directory and files successfully received");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving directory files from server: " + ex.Message);
            }
        }


        private async void playStream_Click(object sender, EventArgs e)
        {
            try
            {
                // Connect to the server
                TcpClient client = new TcpClient();
                await client.ConnectAsync("127.0.0.1", 1234); // Connect to the server's IP address and port

                playStream.Enabled = false;

                // Receive the video stream from the server
                NetworkStream ns = client.GetStream();
                byte[] buffer = new byte[4096]; // Buffer to hold incoming data

                // Continuously read data from the stream
                while (client.Connected)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int bytesRead;
                        while ((bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, bytesRead);
                            if (IsEndOfFrame(buffer, bytesRead))
                            {
                                break;
                            }
                        }

                        // Convert the byte array to an image and display it
                        ms.Seek(0, SeekOrigin.Begin);
                        Image image = Image.FromStream(ms);
                        pictureBox1.Invoke((MethodInvoker)delegate
                        {
                            pictureBox1.Image = image;
                        });
                    }
                }

                // Close the connection with the server
                client.Close();
            }
            catch (IOException ex)
            {
                // Log the exception details
                Console.WriteLine("IOException while playing stream: " + ex.ToString());
            }
            catch (SocketException ex)
            {
                // Log the exception details
                Console.WriteLine("SocketException while playing stream: " + ex.ToString());
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine("Error playing stream: " + ex.ToString());
            }
        }


        private bool IsEndOfFrame(byte[] buffer, int bytesRead)
        {
            // Check for end of frame marker or condition
            return bytesRead < buffer.Length;
        }


    }
}
