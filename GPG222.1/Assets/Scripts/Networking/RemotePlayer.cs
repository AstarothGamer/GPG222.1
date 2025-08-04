using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public string playerID;

    public bool canDie = true;

    public string playerName;

    private Vector3 lastPosition;

    private Vector3 targetScale = Vector3.one;

    void Start()
    {

        lastPosition = transform.position;

    }

    void Update()
    {

        transform.position = Vector3.Lerp(transform.position, lastPosition, 10f * Time.deltaTime);

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 10f * Time.deltaTime);

    }

    public void SetPosition(Vector2 pos)
    {
        lastPosition = new Vector3(pos.x, pos.y, 0);
    }

    public void SetScale(Vector3 scale)
    {
        targetScale = scale;
    }


    public void SetCanDie(bool value)
    {

        canDie = value;

    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
