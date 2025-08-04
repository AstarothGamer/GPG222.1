using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ChatSystem
{
    public class TextBoxManager : MonoBehaviour
    {
        public static TextBoxManager Instance { get; private set; }

        private TextMeshProUGUI _textBox;
        private Queue<string> _textQueue = new();
    
        [SerializeField] int maxLines = 8;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            _textBox = GetComponent<TextMeshProUGUI>();
            for (int i = 0; i < maxLines; i++)
            {
                _textQueue.Enqueue("");
            }
            _textBox.text = string.Join("\n", _textQueue.ToArray());
            // StartCoroutine(TestTextUpdate());
        }
    
        // private IEnumerator TestTextUpdate()
        // {
        //     while (true)
        //     {
        //         UpdateText("Test message at " + Time.time);
        //         yield return new WaitForSeconds(0.2f);
        //     }
        // }
        public void UpdateText(string text)
        {
            _textQueue.Enqueue(text);
            if (_textQueue.Count > maxLines)
            {
                _textQueue.Dequeue();
            }
        
            _textBox.text = string.Join("\n", _textQueue.ToArray());
        }
    }
}
