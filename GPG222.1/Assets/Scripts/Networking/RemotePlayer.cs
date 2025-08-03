using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    [SerializeField] public string playerID;
    [SerializeField] public bool canDie = true;

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
    }

    public void SetScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }
}
