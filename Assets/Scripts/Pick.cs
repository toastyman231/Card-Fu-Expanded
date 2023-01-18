using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pick : MonoBehaviour
{
    private void OnMouseDown()
    {
        GameObject.FindGameObjectWithTag("Game").GetComponent<GameSession>().selectedCard = gameObject;
    }
}
