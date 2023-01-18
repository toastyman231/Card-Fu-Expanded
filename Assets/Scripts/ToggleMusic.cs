using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleMusic : MonoBehaviour
{
    private void OnMouseDown()
    {
        Options gameOptions = GameObject.FindGameObjectWithTag("Options").GetComponent<Options>();
        gameOptions.musicOn = !gameOptions.musicOn;
        if (gameOptions.musicOn)
        {
            GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Checkbox3");
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Checkbox2");
        }
    }
}
