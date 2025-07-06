using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;

namespace Multiplayer
{
    public class Client : MonoBehaviour
    {
        Socket _socket;
        private const string IPAddress = "127.0.0.1"; 
        private const int Port = 3000;
        bool _connected = false;
        bool _tryConnect = false;

        [SerializeField] private TextMeshProUGUI chatText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_InputField authorField;

        private Thread _receiveThread;
        private bool _running = false;
        private readonly object _queueLock = new();
        private readonly System.Collections.Generic.Queue<string> _messageQueue = new();

        void Start()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Debug.LogError("Client Started");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                print("W");
            }

            if (Input.GetKeyDown(KeyCode.W) && !_connected && !_tryConnect)
            {
                _tryConnect = true;
                Debug.LogError("Trying to connect");

                try
                {
                    _socket.Connect(IPAddress, Port);
                    _socket.Blocking = false;
                    _connected = true;
                    Debug.LogError("Connected to server");
                    StartReceiving();
                }
                catch (SocketException e)
                {
                    Debug.LogError("Connection failed: " + e.Message);
                }
                finally
                {
                    _tryConnect = false;
                }
            }

            if (_connected && inputField != null && Input.GetKeyDown(KeyCode.Return))
            {
                string author = authorField != null ? authorField.text : "Anonymous User";
                string message = inputField.text;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    SendChatMessage(author, message);
                    inputField.text = "";
                }
            }

            lock (_queueLock)
            {
                while (_messageQueue.Count > 0)
                {
                    string msg = _messageQueue.Dequeue();
                    var split = msg.IndexOf(':');
                    if (split > 0)
                    {
                        string author = msg.Substring(0, split);
                        string text = msg.Substring(split + 1);
                        UpdateTextChat(author, text);
                    }
                    else
                    {
                        UpdateTextChat("Server", msg);
                    }
                }
            }
        }

        void OnApplicationQuit()
        {
            _running = false;
            if (_receiveThread != null && _receiveThread.IsAlive)
                _receiveThread.Join();
            if (_socket != null && _socket.Connected)
                _socket.Close();
        }

        private void UpdateTextChat(string author, string message)
        {
            if (chatText != null)
            {
                chatText.text += $"<b>{author}:</b> {message}\n";
                var lines = chatText.text.Split('\n');
                if (lines.Length > 50)
                {
                    chatText.text = string.Join("\n", lines, lines.Length - 50, 50);
                }
            }
            else
            {
                Debug.LogError("Chat text UI element is not assigned");
            }
        }

        private void SendChatMessage(string author, string message)
        {
            UpdateTextChat(author, message);
            if (!_connected) return;
            string fullMsg = $"{author}:{message}";
            try
            {
                _socket.Send(Encoding.UTF8.GetBytes(fullMsg));
                Debug.LogError("Message sent: " + fullMsg);
            }
            catch (SocketException e)
            {
                Debug.LogError("Send failed: " + e.Message);
            }
        }

        private void StartReceiving()
        {
            _running = true;
            _receiveThread = new Thread(ReceiveLoop);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }

        private void ReceiveLoop()
        {
            while (_running && _socket != null && _socket.Connected)
            {
                try
                {
                    if (_socket.Available > 0)
                    {
                        byte[] buffer = new byte[_socket.Available];
                        int received = _socket.Receive(buffer);
                        if (received > 0)
                        {
                            string msg = Encoding.UTF8.GetString(buffer, 0, received);
                            lock (_queueLock)
                            {
                                _messageQueue.Enqueue(msg);
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                }
                Thread.Sleep(10);
            }
        }
    }
}
