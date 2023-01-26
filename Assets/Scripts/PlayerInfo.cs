using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariable<bool> deckReady = new NetworkVariable<bool>(false);
    public NetworkVariable<NetworkUintArray> playerDeck = new NetworkVariable<NetworkUintArray>(new NetworkUintArray{deck = new uint[50]});

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsHost) NetworkManager.SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        else SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}

public struct NetworkUintArray : INetworkSerializable
{
    public uint[] deck;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        for (int i = 0; i < deck.Length; i++)
        {
            serializer.SerializeValue(ref deck[i]);
        }
    }

    public void SetDeck(uint[] newDeck)
    {
        deck = newDeck;
    }
}
