using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string playerID;
    public string playerName;
    public float speed = 5f;
    private Rigidbody rb;

    PlayerData pd;

    void Start()
    {
        pd = FindObjectOfType<PlayerData>();
        playerID = pd.playerID;
        playerName = pd.playerName;
        rb = GetComponent<Rigidbody>();
        
        GetComponent<Renderer>().material.color = pd.playerColor;
    }

    void Update()
    {
        Vector2 direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        rb.velocity = direction.normalized * speed;
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
    }
    
}
