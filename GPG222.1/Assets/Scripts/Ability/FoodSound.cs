using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSound : MonoBehaviour
{


    public AudioClip clip;


    

    public void Play()
    {

        if (clip != null)
        {

            AudioSource.PlayClipAtPoint(clip, transform.position);

            Debug.Log("Sound played");

        }
            

    }
}
