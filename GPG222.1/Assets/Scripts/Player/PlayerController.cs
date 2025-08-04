using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public string playerID;
    public string playerName;
    //public float speed = 5f;
    public float baseSpeed = 5f;
    public float speedMultiplier = 2f;
    public float boostDuration = 3f;
    private float speed;
    private bool isBoosted = false;

    private Rigidbody rb;



    PlayerData pd;

    public bool canDie = true;

    void Awake()
    {
        pd = FindObjectOfType<PlayerData>();
        playerID = pd.playerID;
        playerName = pd.playerName;
        rb = GetComponent<Rigidbody>();
        
        GetComponent<Renderer>().material.color = pd.playerColor;
        speed = baseSpeed;
    }

    void Update()
    {
        Vector2 direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        rb.velocity = direction.normalized * speed;
    }

    public void ApplySpeedBoost()
    {
        if (!isBoosted)
        {

            StartCoroutine(BoostRoutine());

        }


    }

    private System.Collections.IEnumerator BoostRoutine()
    {
        isBoosted = true;

        speed = baseSpeed * speedMultiplier;

        transform.localScale *= 1.1f;

        yield return new WaitForSeconds(boostDuration);

        speed = baseSpeed;

        transform.localScale /= 1.1f;

        isBoosted = false;


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            FoodController food = other.GetComponent<FoodController>();
            if (food != null)
            {
                Client.Instance.NotifyFoodEaten(food.foodID);
                transform.localScale *= 1.01f;

                // FoodCounter.Instance?.Increment();

                FoodSound sound = other.GetComponent<FoodSound>();

                if (sound != null)
                {

                    sound.Play();

                }
                FoodCounter.Instance?.Increment();
            }
        }

        if (other.CompareTag("SpeedBoost"))
        {

            SpeedBoostController boost = other.GetComponent<SpeedBoostController>();

            if (boost != null)
            {


                Client.Instance.NotifyBoostCollected(boost.boostID);


            }


        }

        if (other.GetComponent<RemotePlayer>())
        {
            Transform enemy = other.transform;
            Kill(enemy);
        }
    }

    public void Die()
    {
        Debug.Log("Player has died.");
        Client.Instance.Disconnection();
        SceneManager.LoadScene("MainMenu");
    }
    
    public void Kill(Transform enemy)
    {
        RemotePlayer rp = enemy.GetComponent<RemotePlayer>();
        if (rp == null) return;
        Debug.Log("Trying eat another player");

        if (rp.canDie && transform.localScale.magnitude > enemy.localScale.magnitude)
        {
            Debug.Log("Another player should die");
            rp.Die();
            Client.Instance.NotifyPlayerShouldDie(rp.playerID, rp.canDie);
        }
    }
}
