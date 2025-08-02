using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public string playerID;

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
    }

    public void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }
}
