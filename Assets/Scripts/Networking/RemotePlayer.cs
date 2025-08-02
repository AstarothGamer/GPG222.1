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

    public void SetColor(Color color)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
}
