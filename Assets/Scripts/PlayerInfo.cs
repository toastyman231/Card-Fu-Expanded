using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariable<ulong> selectedCardId = new NetworkVariable<ulong>();

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.Singleton.Shutdown();

        if (IsHost) NetworkManager.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        else SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
