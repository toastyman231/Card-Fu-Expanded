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

    public NetworkVariable<int> playerEarthWins = 
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> playerFireWins =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> playerWaterWins =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> playerWoodWins =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> playerMetalWins =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    //public NetworkVariable<int>[] playerWins = new NetworkVariable<int>[5];
    private bool readyToPick = false;

    [SerializeField] private GameObject joinCodeText;

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsHost)
        {
            CheckIfMaxPlayersReachedServerRpc(new ServerRpcParams());
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.Singleton.Shutdown();

        if (IsHost) NetworkManager.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        else SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    [ServerRpc]
    private void CheckIfMaxPlayersReachedServerRpc(ServerRpcParams serverParams)
    {
        if (NetworkManager.ConnectedClientsIds.Count > 2)
        {
            NetworkManager.DisconnectClient(serverParams.Receive.SenderClientId);
        }
    }

    public int GetWins(int type)
    {
        switch (type)
        {
            case 0:
                return playerFireWins.Value;
            case 1:
                return playerEarthWins.Value;
            case 2:
                return playerMetalWins.Value;
            case 3:
                return playerWaterWins.Value;
            case 4:
                return playerWoodWins.Value;
            default:
                return 0;
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void IncrementPlayerWinsServerRpc(int type)
    {
        NetworkLog.LogInfoServer(IsClient.ToString());
        switch (type)
        {
            case 0:
                playerFireWins.Value++;
                break;
            case 1:
                playerEarthWins.Value++;
                break;
            case 2:
                playerMetalWins.Value++;
                break;
            case 3:
                playerWaterWins.Value++;
                break;
            case 4:
                playerWoodWins.Value++;
                break;
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
