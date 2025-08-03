using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public string playerID;
    public string playerName;
    [SerializeField] public float speed = 5f;
    // float timer = 0;
    private Rigidbody rb;

    bool isSizing = false;
    bool sizeCoolDown = false;

    bool isSpeeding = false;
    bool speedCoolDown = false;

    bool isShieldUsed = false;
    bool canDie = true;

    PlayerData pd;

    void Start()
    {
        pd = FindObjectOfType<PlayerData>();
        playerID = pd.playerID;
        playerName = pd.playerName;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector2 direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        rb.velocity = direction.normalized * speed;

        if (Input.GetKeyDown(KeyCode.Space) && !isSizing && !sizeCoolDown)
        {
            StartCoroutine(SizeBoost());
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && !isSpeeding && !speedCoolDown)
        {
            StartCoroutine(SpeedBoost());
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isShieldUsed)
        {
            StartCoroutine(ActivateShield());
        }
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
            }
        }
        else if (other.GetComponent<RemotePlayer>())
        {
            Transform enemy = other.transform;
            Kill(enemy);
        }
    }



    IEnumerator SizeBoost()
    {
        isSizing = true;
        sizeCoolDown = true;
        Vector3 scale = transform.localScale;

        transform.localScale *= 1.5f;

        yield return new WaitForSeconds(5f);

        isSizing = false;
        transform.localScale = scale;

        yield return new WaitForSeconds(15f);

        sizeCoolDown = false;
    }

    IEnumerator SpeedBoost()
    {
        isSpeeding = true;
        speedCoolDown = true;

        speed *= 1.5f;

        yield return new WaitForSeconds(2f);

        isSpeeding = false;
        speed = 5f;

        yield return new WaitForSeconds(10f);

        speedCoolDown = false;
    }

    IEnumerator ActivateShield()
    {
        isShieldUsed = true;
        canDie = false;

        yield return new WaitForSeconds(1f);

        canDie = true;
    }

    public void Die()
    {
        if (canDie)
        {
            Destroy(gameObject);
        }
    }

    public void Kill(Transform enemy)
    {
        RemotePlayer rp = enemy.GetComponent<RemotePlayer>();
        if (rp == null) return;
        Debug.Log("Trying eat another player");

        bool enemyCanDie = rp.canDie;

        if (enemyCanDie && transform.localScale.magnitude > enemy.localScale.magnitude)
        {
            Debug.Log("Another player should die");
            Client.Instance.NotifyPlayerShouldDie(rp.playerID, rp.canDie);
        }
    }
}
