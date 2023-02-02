using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInfo : NetworkBehaviour
{
    private readonly int FIRE = 0;

    private readonly int EARTH = 1;

    private readonly int METAL = 2;

    private readonly int WATER = 3;

    private readonly int WOOD = 4;

    public NetworkVariable<ulong> selectedCardId = 
        new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> pickedCard = 
        new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> playerEarthWins = 
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> playerFireWins =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> playerWaterWins =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> playerWoodWins =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> playerMetalWins =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //public NetworkVariable<int>[] playerWins = new NetworkVariable<int>[5];
    private bool readyToPick = false;

    public override void OnNetworkSpawn()
    {
        //for (int i = 0; i < playerWins.Length; i++)
        //{
        //    playerWins[i] = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        //        NetworkVariableWritePermission.Owner);
        //}
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.Singleton.Shutdown();

        if (IsHost) NetworkManager.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        else SceneManager.LoadScene("Menu", LoadSceneMode.Single);
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

    public void IncrementPlayerWins(int type)
    {
        //playerWins[type].Value++;
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

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    for (int i = 0; i < playerWins.Length; i++)
        //    {
        //        NetworkLog.LogInfoServer(playerWins[i].Value.ToString());
        //    }
        //}
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
