using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;


public class ServerTest : MonoBehaviour
{

    Socket serverSocket;


    int port = 6666;

    List<Socket> clientsSocket;



    void Start()           //A list of clients is created
    {
        clientsSocket = new List<Socket>();

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  //TCP socket is opened

        serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));      //Binds to any IP on port 6666

        serverSocket.Listen(3);          //  Listens to a maximum of 3 connections

        serverSocket.Blocking = false;  //the server throws an exception if there is nothing


    }



    void Update()
    {
        try
        {

            clientsSocket.Add(serverSocket.Accept());            //Trying to accept a new client / If the client is connected, it is added to clientsSocket

        }
        catch
        {
        }

        try
        {
            for (int i = 0; i < clientsSocket.Count; i++)
            {
                if (clientsSocket[i].Available > 0)           //If the client has data,  read it into the buffer /  Then forward this data to all other clients.
                {

                    byte[] buffer = new byte[clientsSocket[i].Available];

                    clientsSocket[i].Receive(buffer);



                    string message = Encoding.UTF8.GetString(buffer);  //Converts received bytes from the client into a text string

                    if (ServerChat.Instance != null)        //Checking if a link to ServerChatUI exists
                    {
                        ServerChat.Instance.AppendMessage(message);       //Shows a message in the chat window on the server scene
                    }



                    for (int j = 0; j < clientsSocket.Count; j++)
                    {

                        if (j != i)
                        {

                            clientsSocket[j].Send(buffer);

                        }

                    }

                }

            }

        }
        catch
        {
        }
    }



}

