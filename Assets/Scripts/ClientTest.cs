using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;



public class ClientTest : MonoBehaviour
{

    Socket socket;   //TCP socket for communication with the server

    int port = 6666;

    bool connected = false;


    public static ClientTest Instance { get; private set; }      //Singleton access to the client


    void Awake()           //Implements the Singleton pattern
    {
        if (Instance == null)
        {

            Instance = this;

            DontDestroyOnLoad(gameObject);

        }
        else
        {

            Destroy(gameObject);

            return;
        }
    }


    void Start()      //Creates a TCP socket
    {

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


    }


    void Update()
    {

        if (!connected)
        {

            return;

        }


        try
        {

            if (socket.Available > 0)
            {

                byte[] buffer = new byte[socket.Available];   //Reads data into byte[]

                socket.Receive(buffer);

                string message = Encoding.UTF8.GetString(buffer);  //Decodes from bytes to string

                Debug.Log("Received: " + message);

                ChatUI.Instance.AppendMessage(message);

            }



        }
        catch 
        { 
        }



    }




    public void ConnectToServer(string ipAddress)        //Trying to connect to ip 6666
    {

        if (connected)
        {
            return;

        }


        try
        {


            socket.Connect(ipAddress, port);        //Trying to connect by IP and port


            socket.Blocking = false;                 //Makes the socket non-blocking

            connected = true;

            Debug.Log("Connected to server!");


        }
        catch (SocketException e)
        {

            Debug.LogError("Connection failed: " + e.Message);


        }




    }


    public void SendMessage(string message)
    {
        if (!connected || string.IsNullOrEmpty(message))   //Checks that it is connected
        {
            return;

        }

        byte[] buffer = Encoding.UTF8.GetBytes(message);  //Encodes a string into a byte[]

        socket.Send(buffer);              //Sends data to the server


    }


}
