using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace server
{
    public partial class Form1 : Form
    {
        Socket server;
        Socket client;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;

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

                client = await Task.Run(() => server.Accept());
                ns = new NetworkStream(client);
                sr = new StreamReader(ns, Encoding.UTF8);
                sw = new StreamWriter(ns, Encoding.UTF8);

                ReceiveMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting server: " + ex.Message);
                listen.Enabled = true;
            }
        }

        private async void ReceiveMessages()
        {
            try
            {
                string tmp;
                while ((tmp = await sr.ReadLineAsync()) != null)
                {
                    if (tmp.StartsWith("request file: "))
                    {
                        string filePath = tmp.Substring("request file: ".Length).Trim();

                        string message = "receive file";
                        SendMessage(message);

                        SendFile(filePath);
                    }
                    else if (tmp.StartsWith("request directory: "))
                    {
                        string directoryPath = tmp.Substring("request directory: ".Length).Trim();

                        string message = "receive directory";
                        SendMessage(message);

                        SendDirectoryData(directoryPath);
                    }
                    else if (tmp.StartsWith("download directory: "))
                    {
                        string directoryPath = tmp.Substring("download directory: ".Length).Trim();

                        string message = "receive download directory";
                        SendMessage(message);

                        SendDirectory(directoryPath);
                    }
                    else if (tmp.StartsWith("request video: "))
                    {
                        string videoPath = tmp.Substring("request video: ".Length).Trim();

                        string message = "receive video";
                        SendMessage(message);

                        SendVideo(videoPath);
                    }
                    else if (tmp.StartsWith("request image: "))
                    {
                        string imagePath = tmp.Substring("request image: ".Length).Trim();

                        string message = "receive image";
                        SendMessage(message);

                        SendImage(imagePath);
                    }
                    else
                    {
                        tmp = "Client: " + tmp;
                        UpdateChat(tmp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving message from client: " + ex.Message);
            }
        }

        private void SendFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    ns.Write(fileBytes, 0, fileBytes.Length);

                    UpdateChat("File sent: " + filePath);
                }
                else
                {
                    sw.WriteLine("File not found: " + filePath);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending file to client: " + ex.Message);
            }
        }

        private void SendDirectoryData(string directoryPath)
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
        private void SendDirectory(string directoryPath)
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


        private void SendVideo(string videoPath)
        {
            try
            {
                // Check if the video file exists
                if (File.Exists(videoPath))
                {
                    // Read the video file content into a byte array
                    byte[] videoBytes = File.ReadAllBytes(videoPath);

                    // Send the video size to the client
                    sw.WriteLine(videoBytes.Length);
                    sw.Flush();

                    // Send the video content to the client
                    ns.Write(videoBytes, 0, videoBytes.Length);

                    // Log the successful video transmission
                    UpdateChat("Video sent: " + videoPath);
                }
                else
                {
                    // Notify the client that the video file does not exist
                    sw.WriteLine("Video file not found: " + videoPath);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending video to client: " + ex.Message);
            }
        }

        private void SendImage(string imagePath)
        {
            try
            {
                // Check if the image file exists
                if (File.Exists(imagePath))
                {
                    // Read the image file content into a byte array
                    byte[] imageBytes = File.ReadAllBytes(imagePath);

                    // Send the image size to the client
                    sw.WriteLine(imageBytes.Length);
                    sw.Flush();

                    // Send the image content to the client
                    ns.Write(imageBytes, 0, imageBytes.Length);

                    // Log the successful image transmission
                    UpdateChat("Image sent: " + imagePath);
                }
                else
                {
                    // Notify the client that the image file does not exist
                    sw.WriteLine("Image file not found: " + imagePath);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending image to client: " + ex.Message);
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
                MessageBox.Show("Error sending message to client: " + ex.Message);
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
    }
}
