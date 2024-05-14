using System;
using System.IO;
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
                        ReceiveFile();
                    }
                    else if (tmp.StartsWith("receive directory"))
                    {
                        ReceiveDirectory();
                    }
                    else if (tmp.StartsWith("receive download directory"))
                    {
                        ReceiveDownloadDirectory();
                    }
                    else if (tmp.StartsWith("receive video"))
                    {
                        ReceiveVideo();
                    }
                    else if (tmp.StartsWith("receive image"))
                    {
                        ReceiveImage();
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

        private void ReceiveFile()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                using (FileStream fs = new FileStream("received_file.txt", FileMode.Create, FileAccess.Write))
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




        private void ReceiveVideo()
        {
            try
            {
                // Read the video size from the server
                int videoSize = int.Parse(sr.ReadLine());

                // Read the video content into a byte array
                byte[] videoData = new byte[videoSize];
                int bytesRead = ns.Read(videoData, 0, videoSize);

                string videoFilePath = "received_video.mp4";
                File.WriteAllBytes(videoFilePath, videoData);

                // Log the successful video reception
                UpdateChat("Video receivedL open client\\client\\bin\\Debug\\net8.0-windows to see it");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving video from server: " + ex.Message);
            }
        }

        private void ReceiveImage()
        {
            try
            {
                // Read the image size from the server
                int imageSize = int.Parse(sr.ReadLine());

                // Read the image content into a byte array
                byte[] imageData = new byte[imageSize];
                int bytesRead = ns.Read(imageData, 0, imageSize);

                string imageFilePath = "received_image.png";
                File.WriteAllBytes(imageFilePath, imageData);

                // Log the successful image reception
                UpdateChat("Image received: open this client\\client\\bin\\Debug\\net8.0-windows to see it");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving image from server: " + ex.Message);
            }
        }
    }
}
