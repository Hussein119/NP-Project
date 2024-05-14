namespace client
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ChatArea = new TextBox();
            MessageArea = new TextBox();
            send = new Button();
            connect = new Button();
            directoryScreen = new TextBox();
            Directories = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            SuspendLayout();
            // 
            // ChatArea
            // 
            ChatArea.Location = new Point(12, 12);
            ChatArea.Multiline = true;
            ChatArea.Name = "ChatArea";
            ChatArea.Size = new Size(608, 329);
            ChatArea.TabIndex = 0;
            // 
            // MessageArea
            // 
            MessageArea.Location = new Point(12, 482);
            MessageArea.Multiline = true;
            MessageArea.Name = "MessageArea";
            MessageArea.Size = new Size(586, 56);
            MessageArea.TabIndex = 1;
            // 
            // send
            // 
            send.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            send.Location = new Point(733, 482);
            send.Name = "send";
            send.Size = new Size(123, 56);
            send.TabIndex = 2;
            send.Text = "send";
            send.UseVisualStyleBackColor = true;
            send.Click += send_Click;
            // 
            // connect
            // 
            connect.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            connect.Location = new Point(733, 12);
            connect.Name = "connect";
            connect.Size = new Size(123, 77);
            connect.TabIndex = 3;
            connect.Text = "connect";
            connect.UseVisualStyleBackColor = true;
            connect.Click += connect_Click;
            // 
            // directoryScreen
            // 
            directoryScreen.Location = new Point(651, 133);
            directoryScreen.Multiline = true;
            directoryScreen.Name = "directoryScreen";
            directoryScreen.Size = new Size(312, 335);
            directoryScreen.TabIndex = 4;
            // 
            // Directories
            // 
            Directories.AutoSize = true;
            Directories.Font = new Font("Segoe UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Directories.Location = new Point(733, 92);
            Directories.Name = "Directories";
            Directories.Size = new Size(131, 31);
            Directories.TabIndex = 5;
            Directories.Text = "Directories";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 359);
            label1.Name = "label1";
            label1.Size = new Size(622, 20);
            label1.TabIndex = 6;
            label1.Text = "You are by default in a real time chat and you can run any of this commands in the chat:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 388);
            label2.Name = "label2";
            label2.Size = new Size(446, 20);
            label2.TabIndex = 7;
            label2.Text = "1- if you want to get a file from the server enter: request file: PATH";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 408);
            label3.Name = "label3";
            label3.Size = new Size(478, 20);
            label3.TabIndex = 8;
            label3.Text = "2- if you want to get a video from the server enter: request video: PATH";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 428);
            label4.Name = "label4";
            label4.Size = new Size(488, 20);
            label4.TabIndex = 9;
            label4.Text = "3- if you want to get a image from the server enter: request image: PATH";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 448);
            label5.Name = "label5";
            label5.Size = new Size(505, 20);
            label5.TabIndex = 10;
            label5.Text = "4- if you want to get a image from the server enter: request directory: PATH";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Lime;
            ClientSize = new Size(1022, 550);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(Directories);
            Controls.Add(directoryScreen);
            Controls.Add(connect);
            Controls.Add(send);
            Controls.Add(MessageArea);
            Controls.Add(ChatArea);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox ChatArea;
        private TextBox MessageArea;
        private Button send;
        private Button connect;
        private TextBox directoryScreen;
        private Label Directories;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
    }
}
