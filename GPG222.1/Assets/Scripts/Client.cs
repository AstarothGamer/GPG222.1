using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour
{
    Socket socket;
    string ipAddress = "192.168.232.36";
    int port = 3000;
    bool connected;

    // Start is called before the first frame update
    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!connected)
            {
                try
                {
                    socket.Connect(ipAddress, port);
                    socket.Blocking = false;
                    connected = true;
                    Debug.LogError("Connected to the server at " + ipAddress + ":" + port);
                }
                catch
                {
                }
            }
        }


        if (!connected)
            return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            try
            {
                socket.Send(Encoding.ASCII.GetBytes("LOL is the worst game"));
                Debug.LogError("Message sent");
            }
            catch
            {
                
            }
        }
    }
}
