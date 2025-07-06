using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class ChatUI : MonoBehaviour
{


    [SerializeField] TMP_InputField ChatInput; 

    [SerializeField] Button SendButton;

    [SerializeField] TMP_Text ChatBox;



    public static ChatUI Instance; //Singleton access to this script

    void Awake()
    {

        Instance = this;              //Allows access to ChatUI.Instance from other scripts

    }

    void Start()
    {

        SendButton.onClick.AddListener(OnSendButtonClicked);



    }


    void OnSendButtonClicked()
    {

        string message = ChatInput.text.Trim();

        if (!string.IsNullOrEmpty(message))
        {
            ClientTest.Instance.SendMessage(message);      //Sending to server

            AppendMessage(message);   //Adding to local chat

            ChatInput.text = "";  


        }


    }

    public void AppendMessage(string message)
    {

        ChatBox.text += message + "\n";    // Each new message is added to the ChatBox on a new line


    }

}
