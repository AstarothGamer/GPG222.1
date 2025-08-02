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
    [SerializeField] TMP_InputField RColorField;
    [SerializeField] TMP_InputField GColorField;
    [SerializeField] TMP_InputField BColorField;
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
        
        if (!float.TryParse(RColorField.text, out float r) || !float.TryParse(GColorField.text, out float g) || !float.TryParse(BColorField.text, out float b))
        {
            Debug.LogError("Color fields must be numbers.");
            return;
        }
        
        if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
        {
            Debug.LogError("Color values must be between 0 and 255.");
            return;
        }

        Client.Instance.ConnectToServer(IPAddressField.text, NameField.text, new Color(r / 255f, g / 255f, b / 255f, 1f));
        connectButton.interactable = false;
        IPAddressField.interactable = false;
        NameField.interactable = false;
        RColorField.interactable = false;
        GColorField.interactable = false;
        BColorField.interactable = false;
        Debug.Log($"Connecting to server at {IPAddressField.text}");
    }

    public void OnConnectedToServer()
    {
        SceneManager.LoadScene("GameScene");
    }
}
