using UnityEngine;
using System.Collections;



public class RemotePlayer : MonoBehaviour
{
    public string playerID;

    public bool canDie = true;

    public string playerName;

    private Vector3 lastPosition;

    private Vector3 targetScale = Vector3.one;


    private PlayerName nameTag;


    public float invulnerabilityDuration = 2f;

    //private Renderer[] blinkRenderers;

    //private void Awake()
    //{

    //    blinkRenderers = GetBlinkRenderers();

    //}


    private void Start()
    {

       // lastPosition = transform.position;

        nameTag = GetComponent<PlayerName>() ?? gameObject.AddComponent<PlayerName>();

        var displayName = string.IsNullOrEmpty(playerName) ? gameObject.name : playerName;

        nameTag.SetText(displayName);

        StartCoroutine(InvulnerabilityRoutine(invulnerabilityDuration));

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

    public void SetColor(Color color)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    //private Renderer[] GetBlinkRenderers()
    //{

    //    var all = GetComponentsInChildren<Renderer>(true);

    //    var list = new System.Collections.Generic.List<Renderer>();

    //    foreach (var r in all)
    //    {

    //        if (r == null)
    //        {

    //            continue;

    //        }

    //        var n = r.gameObject.name;

    //        if (n != null && n.Contains("NameTag"))
    //        {

    //            continue;

    //        }

    //        list.Add(r);

    //    }

    //    return list.ToArray();
    //}

    private IEnumerator InvulnerabilityRoutine(float duration)
    {

        canDie = false;

        yield return new WaitForSeconds(duration);

        canDie = true;

        //float end = Time.time + duration;

        //bool on = true;

        //float interval = 0.15f;

        //while (Time.time < end)
        //{

        //    on = !on;

        //    foreach (var r in blinkRenderers)
        //    {

        //        if (r != null)
        //        {

        //            r.enabled = on;

        //        }

        //    }

        //    yield return new WaitForSeconds(interval);
        //}

        //foreach (var r in blinkRenderers)
        //{

        //    if (r != null)
        //    {

        //        r.enabled = true;

        //    }



        //}


    }







}
