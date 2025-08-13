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
        Invoke(nameof(Respawn), respawnTime);
    }

    void Respawn()
    {
        Vector2 randomPos = new Vector2(
            Random.Range(respawnAreaMin.x, respawnAreaMax.x),
            Random.Range(respawnAreaMin.y, respawnAreaMax.y));

        transform.position = randomPos;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Consume();
        }
    }
}
