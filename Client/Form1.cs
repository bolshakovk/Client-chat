using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Client
{
    public partial class Form1 : Form
    {
        // Делегаты 
        private delegate void printer(string data);
        private delegate void cleaner();

        readonly printer Printer;
        readonly cleaner Cleaner;
        private Socket _serverSocket;
        private Thread _clientThread;
        private const string _serverHost = "localhost";
        private const int _serverPort = 9933;
        public Form1()
        {
            InitializeComponent();
            Printer = new printer(print);
            Cleaner = new cleaner(ClearChat);
            Connect();
            _clientThread = new Thread(Listner)
            {
                IsBackground = true
            };
            _clientThread.Start();
        }
        // Считывание даты с сервера
        private void Listner()
        {
            while (_serverSocket.Connected)
            {
                byte[] buffer = new byte[8196];
                int bytesRec = _serverSocket.Receive(buffer);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRec);
                if (data.Contains("#updatechat"))
                {
                    UpdateChat(data);
                    continue;
                }
            }
        }
        // Коннект к серверу
        private void Connect()
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(_serverHost);
                IPAddress ipAddress = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, _serverPort);
                _serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Connect(ipEndPoint);
            }
            catch { print("Сервер недоступен!"); }
        }
        // Отчистка текстбокса
        private void ClearChat()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Cleaner);
                return;
            }
            chatBox.Clear();
        }
        // Отрисовка сообщений в чате
        private void UpdateChat(string data)
        {
            //#updatechat&userName~data|username~data
            ClearChat();
            string[] messages = data.Split('&')[1].Split('|');
            int countMessages = messages.Length;
            if (countMessages <= 0) return;
            for (int i = 0; i < countMessages; i++)
            {
                try
                {
                    if (string.IsNullOrEmpty(messages[i])) continue;
                    print(String.Format("[{0}]:{1}.", messages[i].Split('~')[0], messages[i].Split('~')[1]));
                }
                catch { continue; }
            }
        }
        // Передача побайтовой сообщения на сервер
        private void Send(string data)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int bytesSent = _serverSocket.Send(buffer);
            }
            catch { print("Связь с сервером прервалась..."); }
        }
        // отрисовка в текстбоксе сообщения
        private void print(string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Printer, msg);
                return;
            }
            if (chatBox.Text.Length == 0)
                chatBox.AppendText(msg);
            else
                chatBox.AppendText(Environment.NewLine + msg);
        }

        // Отправка сообщения
        private void sendMessage()
        {
            try
            {
                string data = chat_msg.Text;
                if (string.IsNullOrEmpty(data)) return;
                Send("#newmsg&" + data);
                chat_msg.Text = string.Empty;
            }
            catch { MessageBox.Show("Ошибка при отправке сообщения!"); }
        }
        private void chatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                sendMessage();
        }

        // Отправка сообщения по клику на кнопку
        private void chat_send_Click(object sender, EventArgs e)
        {
            sendMessage();
        }
        // Кнопка войти в чат, позволяет печатать в чат, отправляет имя пользователя на сервер через сокет
        private void enterChat_Click(object sender, EventArgs e)
        {
            string Name = userName.Text;
            if (string.IsNullOrEmpty(Name)) return;
            Send("#setname&" + Name);
            chat_msg.Enabled = true;
            chat_send.Enabled = true;
            userName.Enabled = false;
            enterChat.Enabled = false;
        }


        private void chat_msg_TextChanged(object sender, EventArgs e)
        {

        }

        private void chatBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}

