using System.Collections;
using UnityEngine;

namespace ChatSystem
{
    public class HideChat : MonoBehaviour
    {
        [SerializeField] private GameObject chatHeader;
        [SerializeField] private GameObject moveLocation;
        [SerializeField] private GameObject chatButtonImage;

        [SerializeField] private float animationDuration = 0.5f;

        private Vector3 _initialPosition;
        private bool _isChatVisible = true;
        private int _initialRotation = 0;

        private void Start()
        {
            _initialPosition = chatHeader.transform.position;
        }

        public void ToggleChat()
        {
            if (_isChatVisible)
            {
                StartCoroutine(RotateButtonImage180Degrees());
                StartCoroutine(MoveChat());
            }
            else
            {
                StartCoroutine(RotateButtonImage180Degrees());
                StartCoroutine(MoveChatBack());
            }
        }

        private IEnumerator MoveChat()
        {
            Vector3 targetPosition = moveLocation.transform.position;
            Vector3 startPosition = chatHeader.transform.position;
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / animationDuration);
                chatHeader.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            chatHeader.transform.position = targetPosition;
            _isChatVisible = !_isChatVisible;
        }

        private IEnumerator MoveChatBack()
        {
            Vector3 targetPosition = _initialPosition;
            Vector3 startPosition = chatHeader.transform.position;
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / animationDuration);
                chatHeader.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            chatHeader.transform.position = targetPosition;
            _isChatVisible = !_isChatVisible;
        }

        private IEnumerator RotateButtonImage180Degrees()
        {

            float elapsedTime = 0f;
            Quaternion startRotation = chatButtonImage.transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(0, 0, _initialRotation);

            while (elapsedTime < animationDuration)
            {
                chatButtonImage.transform.rotation =
                    Quaternion.Lerp(startRotation, targetRotation, elapsedTime / animationDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            chatButtonImage.transform.rotation = targetRotation;
            if (_initialRotation == 0)
            {
                _initialRotation = 180;
            }
            else
            {
                _initialRotation = 0;
            }
        }
    }
}
