using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    private void Awake()
    {
        Options gameOptions = GameObject.FindGameObjectWithTag("Options").GetComponent<Options>();

        if (gameOptions.multiplayer)
        {
            gameObject.AddComponent<MultiplayerSession>();
        }
        else
        {
            gameObject.AddComponent<GameSession>();
        }
    }
}
