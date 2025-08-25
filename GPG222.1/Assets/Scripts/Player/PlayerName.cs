using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;




public class PlayerName : MonoBehaviour
{

    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, 0f);

    [SerializeField] private float fontSize = 2.2f;

    private Camera cam;

    private TextMeshPro tmp;

    private void Awake()
    {

        cam = Camera.main;

        var gObj = new GameObject("NameTagTMP");

        gObj.transform.SetParent(transform, false);

        gObj.transform.localPosition = offset;

        tmp = gObj.AddComponent<TextMeshPro>();

        tmp.alignment = TextAlignmentOptions.Center;

        tmp.fontSize = fontSize;

        tmp.enableWordWrapping = false;

        tmp.outlineWidth = 0.3f;

        tmp.outlineColor = Color.black;

        tmp.text = "";


    }


    public void SetText(string text)
    {

        if (tmp != null)
        {

            tmp.text = text;

        }


    }

    private void LateUpdate()
    {

        if (tmp == null)
        {

            return;

        }
        if (cam == null)
        {

            cam = Camera.main;

        }
        if (cam == null) 
        {

            return;

        }



        //var toCam = cam.transform.position - tmp.transform.position;

        Vector3 dir = tmp.transform.position - cam.transform.position;

        tmp.transform.rotation = Quaternion.LookRotation(dir, cam.transform.up);

    }












}
