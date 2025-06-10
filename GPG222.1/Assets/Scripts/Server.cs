using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using UnityEngine;

public class Server : MonoBehaviour
{
    Socket serverSocket;
    string ipAddress = "";
    int port = 3000;

    Socket clientSocket;

    void Start()
    {
        

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
        serverSocket.Listen(10);
        serverSocket.Blocking = false;


    }

    void Update()
    {
        try
        {
            clientSocket = serverSocket.Accept();
            Debug.LogError("Client connected: " + clientSocket.RemoteEndPoint.ToString());
        }
        catch
        {
            print("Trying to connect");
        }

        if (clientSocket == null)
            return;
            
        try
        {
            if (clientSocket[i].Available > 0)
            {
                byte[] buffer = new byte[clientSocket[i].Available];
                clientSocket.Receive(buffer);
                Debug.LogError(Encoding.ASCII.GetString(buffer));
            }
        }
        catch
        {

        }
    }
}
