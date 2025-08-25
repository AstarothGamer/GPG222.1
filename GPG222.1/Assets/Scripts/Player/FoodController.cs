using UnityEngine;

public class FoodController : MonoBehaviour
{
    public int foodID;
    public float respawnTime = 5f;
    public Vector2 respawnAreaMin = new Vector2(-10f, -10f);
    public Vector2 respawnAreaMax = new Vector2(10f, 10f);

    void Start()
    {
        Client.Instance.RegisterFood(this);
    }


    public void Consume()
    {
        gameObject.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Consume();
        }
    }
}
