using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkTest : NetworkBehaviour
{
    [SerializeField] private Transform cardPrefab;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if(Input.GetKeyDown(KeyCode.T))
        {
            SpawnCardServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnCardServerRpc()
    {
        Transform card = Instantiate(cardPrefab);
        card.GetComponent<NetworkObject>().Spawn(true);
        Debug.Log(OwnerClientId + ": Spawned card!");
    }
}
