using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public string playerID;
    public string playerName;
    public Color playerColor;

    void Awake()
    {
        if (string.IsNullOrEmpty(playerID))
        {

            playerID = SystemInfo.deviceUniqueIdentifier;

        }
    }
}
