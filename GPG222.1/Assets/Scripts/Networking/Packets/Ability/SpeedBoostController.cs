using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class SpeedBoostController : MonoBehaviour
{

    public int boostID;

    public float respawnTime = 5f;

    public Vector2 areaMinimal = new(-10f, -10f);

    public Vector2 areaMaximal = new(10f, 10f);



    public void Collect()
    {
        gameObject.SetActive(false);

        Invoke(nameof(Respawn), respawnTime);



    }


    void Respawn()
    {

        Vector2 randomPos = new(Random.Range(areaMinimal.x, areaMaximal.x), Random.Range(areaMinimal.y, areaMaximal.y));

        transform.position = randomPos;

        gameObject.SetActive(true);
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {

            // Client.Instance.NotifyBoostCollected(boostID);

            Collect();

        }
    }
}
