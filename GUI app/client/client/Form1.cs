using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private void ReceiveFile(string type)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                using (FileStream fs = new FileStream("received_file." + type, FileMode.Create, FileAccess.Write))
                {
                    while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                        if (ns.DataAvailable == false)
                        {
                            // No more data available, assume end of file
                            break;
                        }
                    }
                }
                UpdateChat("File received: open this client\\client\\bin\\Debug\\net8.0-windows to see it");
                MessageBox.Show("File received");
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

                // Receive the video file from the server
                NetworkStream ns = client.GetStream();
                byte[] videoBytes = new byte[1024]; // Buffer to hold video bytes
                int bytesRead;
                using (MemoryStream ms = new MemoryStream())
                {
                    while ((bytesRead = await ns.ReadAsync(videoBytes, 0, videoBytes.Length)) > 0)
                    {
                        ms.Write(videoBytes, 0, bytesRead);
                    }
                    ms.Seek(0, SeekOrigin.Begin); // Reset memory stream position

                    // Save video bytes to a temporary file
                    string tempFilePath = Path.GetTempFileName();
                    File.WriteAllBytes(tempFilePath, ms.ToArray());

                    // Launch VLC to play the video from the temporary file
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe",
                        Arguments = tempFilePath, // Provide the temporary file path to VLC
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                }

                // Close the connection with the server
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing stream: " + ex.Message);
            }
        }



    }
}
