using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class FoodCounter : MonoBehaviour
{

    public static FoodCounter Instance { get; private set; }


    public TextMeshProUGUI counterText;

    private int foodCount = 0;

    void Awake()
    {
        Instance = this;

        UpdateText();



    }

    public void Increment()
    {
        foodCount++;

        UpdateText();


    }

    void UpdateText()
    {

        counterText.text = $"{foodCount}";

    }

}
