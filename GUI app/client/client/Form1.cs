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
            try
            {
                StringBuilder directoryInfo = new StringBuilder();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
                {
                    directoryInfo.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                }

                string receivedData = directoryInfo.ToString();

                // Split the received data into lines
                string[] lines = receivedData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Process each line to create files and folders
                foreach (string line in lines)
                {
                    // Check if the line represents a file or directory
                    if (line.StartsWith("Files:") || line.StartsWith("Directories:"))
                    {
                        // Skip the header lines
                        continue;
                    }

                    // Check if the line represents a file or directory
                    if (line.EndsWith("/"))
                    {
                        // Directory
                        string directoryName = line.TrimEnd('/');
                        Directory.CreateDirectory(directoryName);
                    }
                    else
                    {
                        // File
                        string fileName = line;
                        // Create an empty file
                        File.Create(fileName).Close();
                    }
                }

                UpdateDirectoryScreen(receivedData);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving directory information: " + ex.Message);
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
