using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class JoinCodeText : NetworkBehaviour
{
    [SerializeField] private TextMeshPro joinCode;
    private Options gameOptions;

    private void Start()
    {
        gameOptions = GameObject.FindGameObjectWithTag("Options").GetComponent<Options>();

        joinCode.text = "Join Code: " + gameOptions.joinCode;
    }
}
