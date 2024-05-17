namespace server
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
            listen = new Button();
            listenStr = new Button();
            SuspendLayout();
            // 
            // ChatArea
            // 
            ChatArea.Location = new Point(24, 12);
            ChatArea.Multiline = true;
            ChatArea.Name = "ChatArea";
            ChatArea.Size = new Size(585, 412);
            ChatArea.TabIndex = 0;
            // 
            // MessageArea
            // 
            MessageArea.Location = new Point(24, 465);
            MessageArea.Multiline = true;
            MessageArea.Name = "MessageArea";
            MessageArea.Size = new Size(585, 67);
            MessageArea.TabIndex = 1;
            // 
            // send
            // 
            send.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            send.Location = new Point(645, 465);
            send.Name = "send";
            send.Size = new Size(119, 67);
            send.TabIndex = 2;
            send.Text = "send";
            send.UseVisualStyleBackColor = true;
            send.Click += send_Click;
            // 
            // listen
            // 
            listen.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            listen.Location = new Point(645, 12);
            listen.Name = "listen";
            listen.Size = new Size(119, 67);
            listen.TabIndex = 3;
            listen.Text = "listen";
            listen.UseVisualStyleBackColor = true;
            listen.Click += listen_Click;
            // 
            // listenStr
            // 
            listenStr.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            listenStr.Location = new Point(645, 113);
            listenStr.Name = "listenStr";
            listenStr.Size = new Size(128, 67);
            listenStr.TabIndex = 4;
            listenStr.Text = "listenStr";
            listenStr.UseVisualStyleBackColor = true;
            listenStr.Click += listenStr_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(806, 570);
            Controls.Add(listenStr);
            Controls.Add(listen);
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
        private Button listen;
        private Button listenStr;
    }
}
