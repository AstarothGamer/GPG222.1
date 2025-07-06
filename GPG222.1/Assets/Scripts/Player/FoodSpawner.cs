// using UnityEngine;

// public class FoodSpawner : MonoBehaviour
// {
//     public GameObject foodPrefab;
//     public int foodCount = 50;
//     public Vector2 spawnMin = new Vector2(-10f, -10f);
//     public Vector2 spawnMax = new Vector2(10f, 10f);

//     private static int foodIDCounter = 0;

//     void Start()
//     {
//         for (int i = 0; i < foodCount; i++)
//         {
//             Vector2 randomPos = new Vector2(
//                 Random.Range(spawnMin.x, spawnMax.x),
//                 Random.Range(spawnMin.y, spawnMax.y));

//             GameObject food = Instantiate(foodPrefab, randomPos, Quaternion.identity);
//             FoodController foodCtrl = food.GetComponent<FoodController>();

//             foodCtrl.foodID = foodIDCounter++;
//             Client.Instance.RegisterFood(foodCtrl);
//         }
//     }
// }
