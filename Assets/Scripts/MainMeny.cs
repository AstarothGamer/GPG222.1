using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;





public class MainMeny : MonoBehaviour
{


    [SerializeField] TMP_InputField IPInput;

    [SerializeField] TMP_InputField UsernameInput;

    [SerializeField] Button ConnectionButton;

    [SerializeField] TMP_Text Text;


    void Start()
    {

        ConnectionButton.onClick.AddListener(ConnectToServer);  //When the player presses the Connect button, the ConnectToServer method is called


    }


    void ConnectToServer()
    {


        string ip = IPInput.text.Trim();      //Gets IP and name from fields

        string username = UsernameInput.text.Trim();  //Checks that they are not empty


        if (string.IsNullOrEmpty(ip))             // Is emty, so shows message and exits
        {

            Text.text = "Input IP Adress of server";

            return;

        }

        if (string.IsNullOrEmpty(username))   // same
        {
            Text.text = "Input Name of User";
            return;
        }


        Text.text = "Connection.......";


        ClientTest.Instance.ConnectToServer(ip);       //Calls a client connection

        Text.text = "Connected";


    }







}
