using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariable<ulong> selectedCardId = new NetworkVariable<ulong>();
    public NetworkVariable<bool> pickedCard = new NetworkVariable<bool>();
    private bool readyToPick = false;

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.Singleton.Shutdown();

        if (IsHost) NetworkManager.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        else SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void SetReadyToPick(bool ready)
    {
        readyToPick = ready;
    }

    public bool ReadyToPick()
    {
        return readyToPick;
    }
}
