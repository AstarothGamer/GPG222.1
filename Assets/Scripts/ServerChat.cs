using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class ServerChat : MonoBehaviour
{


    [SerializeField] TMP_Text ChatBox;

    public static ServerChat Instance;      //Singleton access to call AppendMessage from any script


    void Awake()
    {
        Instance = this;           //Sets global access  
    }

    public void AppendMessage(string message)
    {

        ChatBox.text += message + "\n";          //Adds a new message


    }


}

