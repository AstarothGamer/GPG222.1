using System;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button connectButton;
    [SerializeField] TMP_InputField IPAddressField;
    [SerializeField] TMP_InputField NameField;
    // Start is called before the first frame update
    void Start()
    {
        Client.Instance.ConnectedToServerEvent += OnConnectedToServer;
    }

    public void ConnectToServer()
    {
        if (String.IsNullOrWhiteSpace(IPAddressField.text))
        {
            Debug.LogError("Wrong IP address or null.");
            return;
        }
        else
        {
            IPAddress.TryParse(IPAddressField.text, out IPAddress address);
            if (address == null)
            {
                Debug.LogError("Wrong IP address format");
                return;
            }
        }

        if (String.IsNullOrWhiteSpace(NameField.text))
        {
            Debug.LogError("Name field is empty.");
            return;
        }

        Client.Instance.ConnectToServer(IPAddressField.text, NameField.text);
    }

    public void OnConnectedToServer()
    {
        SceneManager.LoadScene("GameScene");
    }
}
