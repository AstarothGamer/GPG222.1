using UnityEngine;

public class RestartServer : MonoBehaviour
{
    public Server serverScript;

    public void Restart()
    {
        serverScript = GetComponent<Server>();
        Destroy(serverScript);
        serverScript = gameObject.AddComponent<Server>();
    }
}