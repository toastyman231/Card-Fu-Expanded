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
        playerInfo = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerInfo>();//NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerInfo>();
    }

    private void OnMouseDown()
    {
        if (gameOptions.multiplayer)
        {
            if (playerInfo.ReadyToPick())
            {
                playerInfo.selectedCardId.Value = GetComponent<NetworkObject>().NetworkObjectId;
                playerInfo.pickedCard.Value = true;
                NetworkLog.LogInfoServer("Picked card: " + PlayerDeck.GetCardById(playerInfo.selectedCardId.Value).GetComponent<CardInfo>().ToString());
            } else
            {
                NetworkLog.LogInfoServer("Not ready to pick!");
            }
        }
        else
        {
            //NetworkLog.LogInfoServer("Singeplayer picking");
            GameObject.FindGameObjectWithTag("Game").GetComponent<GameSession>().selectedCard = gameObject;
        }
    }
}
