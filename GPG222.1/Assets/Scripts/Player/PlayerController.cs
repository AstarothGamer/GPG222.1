using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public string playerID;
    public string playerName;
    public float baseSpeed = 5f;
    public float speedMultiplier = 2f;
    public float boostDuration = 3f;

    private float speed;
    private bool isBoosted = false;

    private Rigidbody rb;
    private PlayerData pd;

    public bool canDie = true;

    private PlayerName nameTag;

    private Vector3 inputDir = Vector3.zero;

    public float invulnerabilityDuration = 2f;

    //private Renderer[] blinkRenderers;


    void Awake()
    {
        //pd = FindObjectOfType<PlayerData>();
        //playerID = pd.playerID;
        //playerName = pd.playerName;
        //rb = GetComponent<Rigidbody>();
        //speed = baseSpeed;

        rb = GetComponent<Rigidbody>();

        speed = baseSpeed;

        pd = GetComponent<PlayerData>();

        if (pd == null && Client.Instance != null)
        {

            pd = Client.Instance.pd;

        }

        rb.useGravity = false;

        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.interpolation = RigidbodyInterpolation.Interpolate;

       // blinkRenderers = GetBlinkRenderers();

    }

    private void Start()
    {
        if (pd != null)
        {

            if (string.IsNullOrEmpty(pd.playerID))
            {

                pd.playerID = SystemInfo.deviceUniqueIdentifier;

            }

            playerID = pd.playerID;

            if (!string.IsNullOrEmpty(pd.playerName))
            {

                playerName = pd.playerName;

            }

        }
        if (string.IsNullOrEmpty(playerName))
        {

            playerName = gameObject.name;



        }
            
        nameTag = GetComponent<PlayerName>() ?? gameObject.AddComponent<PlayerName>();

        nameTag.SetText(playerName);

        StartCoroutine(InvulnerabilityRoutine(invulnerabilityDuration));

    }






    private void Update()
    {
        //Vector2 direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //rb.velocity = direction.normalized * speed;

        float h = Input.GetAxisRaw("Horizontal");

        float v = Input.GetAxisRaw("Vertical");

        inputDir = new Vector3(h, v, 0f).normalized;

        //Vector3 direction = new Vector3(h, 0f, v).normalized;

        //rb.velocity = direction * speed;

    }

    private void FixedUpdate()
    {

        if (inputDir.sqrMagnitude > 0f)
        {

            Vector3 next = rb.position + inputDir * speed * Time.fixedDeltaTime;

            rb.MovePosition(next);

        }
        else
        {

            rb.velocity = Vector3.zero;

        }


    }


    //public void ApplySpeedBoost()
    //{
    //    if (!isBoosted)
    //    {
    //        StartCoroutine(BoostRoutine());
    //    }
    //}

    //private System.Collections.IEnumerator BoostRoutine()
    //{
    //    isBoosted = true;
    //    speed = baseSpeed * speedMultiplier;
    //    transform.localScale *= 1.1f;

    //    yield return new WaitForSeconds(boostDuration);

    //    speed = baseSpeed;
    //    transform.localScale /= 1.1f;
    //    isBoosted = false;
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {

            var food = other.GetComponent<FoodController>();



           // FoodController food = other.GetComponent<FoodController>();
            if (food != null)
            {
                Client.Instance?.NotifyFoodEaten(food.foodID);
                transform.localScale *= 1.01f;

                FoodSound sound = other.GetComponent<FoodSound>();
                if (sound != null) sound.Play();

                FoodCounter.Instance?.Increment();
            }
        }

        if (other.CompareTag("SpeedBoost"))
        {

            var boost = other.GetComponent<SpeedBoostController>();

           // SpeedBoostController boost = other.GetComponent<SpeedBoostController>();
            if (boost != null)
            {
                Client.Instance?.NotifyBoostCollected(boost.boostID);
            }
        }

        //if (other.GetComponent<RemotePlayer>())
        //{
        //    Transform enemy = other.transform;
        //    Kill(enemy);
        //}

        if (other.CompareTag("RemotePlayer"))
        {

            Kill(other.transform);

        }

    }

    public void ApplySpeedBoost()
    {

        if (!isBoosted)
        {

            StartCoroutine(BoostRoutine());

        }



    }

    private IEnumerator BoostRoutine()
    {

        isBoosted = true;

        float oldSpeed = speed;

        speed = baseSpeed * speedMultiplier;

        yield return new WaitForSeconds(boostDuration);

        speed = oldSpeed;

        isBoosted = false;


    }


    public void Die()
    {
        Debug.Log("Player has died.");
        //Client.Instance?.Disconnection();
        //SceneManager.LoadScene("MainMenu");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Kill(Transform enemy)
    {
        RemotePlayer rp = enemy.GetComponent<RemotePlayer>();
        if (rp == null || !rp.canDie) return;

        if (transform.localScale.magnitude > enemy.localScale.magnitude)
        {
            Debug.Log("Trying to eat another player");
            Client.Instance?.NotifyPlayerShouldDie(rp.playerID, rp.canDie);
            rp.Die(); // Удаление после отправки пакета
        }
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

        //}
        //foreach (var r in blinkRenderers)
        //{

        //    if (r != null)
        //    {

        //        r.enabled = on;

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
