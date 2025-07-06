using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Multiplayer
{
    public class Server : MonoBehaviour
    {
        private Socket _serverSocket;
        private readonly List<Socket> _clientSockets = new();
        private const int Port = 3000;

        void Start()
        {
            Debug.LogError("Start Server");
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Blocking = false;
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _serverSocket.Listen(10);
            _serverSocket.Blocking = false;

            Debug.LogError("Server started on port " + Port);
        }

        void Update()
        {
            if (_serverSocket.Poll(0, SelectMode.SelectRead))
            {
                Socket newClient = _serverSocket.Accept();
                newClient.Blocking = false;
                _clientSockets.Add(newClient);
                Debug.LogError("Client connected: " + newClient.RemoteEndPoint);
            }

            for (int i = 0; i < _clientSockets.Count; i++)
            {
                Socket client = _clientSockets[i];
                if (client.Available > 0)
                {
                    byte[] buffer = new byte[client.Available];
                    int received = client.Receive(buffer);
                    if (received > 0)
                    {
                        string msg = Encoding.UTF8.GetString(buffer, 0, received);
                        Debug.LogError("Received from client: " + msg);
                        BroadcastMessage(msg, client);
                    }
                }
            }
        }

        private void BroadcastMessage(string message, Socket excludeClient = null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (var client in _clientSockets)
            {
                if (client != excludeClient && client.Connected)
                {
                    try
                    {
                        client.Send(data);
                    }
                    catch (SocketException)
                    {
                    }
                }
            }
        }

        void OnApplicationQuit()
        {
            foreach (var client in _clientSockets)
            {
                client.Close();
            }

            _serverSocket.Close();
        }
    }
}
