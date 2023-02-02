using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    AudioSource source;
    Options gameOptions;
    public bool isMusic;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        gameOptions = GameObject.FindGameObjectWithTag("Options").GetComponent<Options>();
    }

    // Update is called once per frame
    void Update()
    {
        gameOptions = GameObject.FindGameObjectWithTag("Options").GetComponent<Options>();
        if (!gameOptions.musicOn)
        {
            source.enabled = false;
        }
        else
        {
            source.enabled = true;
        }

        if (isMusic)
        {
            if (!source.isPlaying && source.clip == Resources.Load<AudioClip>("Sounds/Card Fu Battle theme intro"))
            {
                source.clip = Resources.Load<AudioClip>("Sounds/Card Fu Battle theme");
                source.Play();
                source.loop = true;
            }
        }
    }
}
