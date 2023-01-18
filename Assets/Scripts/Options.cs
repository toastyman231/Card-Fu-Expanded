using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Options : MonoBehaviour
{
    public bool musicOn;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }
}
