using UnityEngine;

namespace ChatSystem
{
    public class ChatManager : MonoBehaviour
    {
        public static ChatManager Instance { get; private set; }
    
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public void CallUpdateText()
        {
            Client.Instance.AddChat();
        }
    }
}
