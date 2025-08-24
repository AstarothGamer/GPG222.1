using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public string playerID;

    public bool canDie = true;

    public string playerName;

    private Vector3 lastPosition;

    private Vector3 targetScale = Vector3.one;


    private PlayerName nameTag;


    private void Start()
    {

       // lastPosition = transform.position;

        nameTag = GetComponent<PlayerName>() ?? gameObject.AddComponent<PlayerName>();

        var displayName = string.IsNullOrEmpty(playerName) ? gameObject.name : playerName;

        nameTag.SetText(displayName);


    }

    public void ApplyName(string name)
    {
        playerName = name;

        if (nameTag == null)
        {

            nameTag = GetComponent<PlayerName>() ?? gameObject.AddComponent<PlayerName>();

        }

        nameTag.SetText(playerName);

        gameObject.name = playerName;

    }


    //void Update()
    //{

    //    transform.position = Vector3.Lerp(transform.position, lastPosition, 10f * Time.deltaTime);

    //    transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 10f * Time.deltaTime);

    //}

    public void SetPosition(Vector3 pos)
    {
        // lastPosition = new Vector3(pos.x, pos.y, 0);

        lastPosition = pos;

        transform.position = pos;

    }

    public void SetScale(Vector3 scale)
    {
        targetScale = scale;

        transform.localScale = scale;
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
