using TMPro;
using UnityEngine;

namespace ChatSystem
{
    public class ChatFocusHandler : MonoBehaviour
    {
        [SerializeField] private TMP_InputField chatInputField;
        private PlayerController playerController;
    
        private void Start()
        {
            chatInputField = GameObject.Find("ChatInputField")?.GetComponent<TMP_InputField>();
            playerController = FindObjectOfType<PlayerController>();
            if (chatInputField == null || playerController == null)
            {
                Debug.LogError("ChatInputField or PlayerController not found in the scene.");
                return;
            }
            chatInputField.onSelect.AddListener(OnChatFocus);
            chatInputField.onDeselect.AddListener(OnChatUnfocus);
        }

        private void OnChatFocus(string _)
        {
            playerController.enabled = false;
        }

        private void OnChatUnfocus(string _)
        {
            playerController.enabled = true;
        }

        private void Update()
        {
            if (playerController.enabled == false && Input.GetKeyDown(KeyCode.Return))
            {
                Client.Instance.AddChat();
            }
        }
    }
}