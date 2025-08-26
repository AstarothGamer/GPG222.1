using System;
using System.Collections.Generic;
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
    
    [SerializeField] TMP_Dropdown myDropdown;
    
    List<Vector3Int> colorsFromDropDown = new List<Vector3Int>()
    {
        new Vector3Int(255, 0, 0), // Red
        new Vector3Int(0, 255, 0), // Green
        new Vector3Int(0, 0, 255), // Blue
        new Vector3Int(255, 255, 0), // Yellow
        new Vector3Int(255, 165, 0), // Orange
        new Vector3Int(128, 0, 128), // Purple
        new Vector3Int(0, 255, 255), // Cyan
        new Vector3Int(255, 192, 203) // Pink
    };

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

        // Get color from dropdown
        if (myDropdown.value < 0 || myDropdown.value >= colorsFromDropDown.Count)
        {
            Debug.LogError("Invalid color selection.");
            return;
        }
        Vector3Int selectedColor = colorsFromDropDown[myDropdown.value];
        float r = selectedColor.x;
        float g = selectedColor.y;
        float b = selectedColor.z;

        Client.Instance.ConnectToServer(IPAddressField.text, NameField.text, new Color(r / 255f, g / 255f, b / 255f, 1f));
        connectButton.interactable = false;
        IPAddressField.interactable = false;
        NameField.interactable = false;
        myDropdown.interactable = false;
        Debug.Log($"Connecting to server at {IPAddressField.text}");
    }

    public void OnConnectedToServer()
    {
        SceneManager.LoadScene("GameScene");
    }
}
