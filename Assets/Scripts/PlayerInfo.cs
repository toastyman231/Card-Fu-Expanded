using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariable<ulong> selectedCardId = 
        new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> pickedCard = 
        new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<int>[] playerWins = new NetworkVariable<int>[5];
    private bool readyToPick = false;

    public override void OnNetworkSpawn()
    {
        for (int i = 0; i < playerWins.Length; i++)
        {
            playerWins[i] = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.Singleton.Shutdown();

        if (IsHost) NetworkManager.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        else SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            for (int i = 0; i < playerWins.Length; i++)
            {
                NetworkLog.LogInfoServer(playerWins[i].Value.ToString());
            }
        }
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
