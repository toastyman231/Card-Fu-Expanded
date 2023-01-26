using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Pick : NetworkBehaviour
{
    private Options gameOptions;
    private PlayerInfo playerInfo;

    private void Start()
    {
        gameOptions = GameObject.FindGameObjectWithTag("Options").GetComponent<Options>();
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerInfo>();
    }

    private void OnMouseDown()
    {
        if (gameOptions.multiplayer)
        {
            if (playerInfo.ReadyToPick())
            {
                playerInfo.selectedCardId.Value = GetComponent<NetworkObject>().NetworkObjectId;
                playerInfo.pickedCard.Value = true;
            }
        }
        else
        {
            GameObject.FindGameObjectWithTag("Game").GetComponent<GameSession>().selectedCard = gameObject;
        }
    }
}
