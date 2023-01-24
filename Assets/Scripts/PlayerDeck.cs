using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerDeck : NetworkBehaviour
{
    public GameObject[] deck;

    public override void OnNetworkSpawn()
    {
        deck = new GameObject[50];
    }

    [ClientRpc]
    private void SetCardValuesClientRpc(int val, Elements.Element element)
    {
        CardInfo.recentCard.SetupCard(val, element);
    }

    public void InitializeDeck()
    {
        NetworkLog.LogInfoServer("Initializing deck");
        int slot = 0;
        for (int i = 0; i < 10; i++)
        {
            GameObject nextECard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextECard.GetComponent<NetworkObject>().Spawn(true);
            SetCardValuesClientRpc(i + 1, Elements.Element.EARTH);

            GameObject nextFCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextFCard.GetComponent<NetworkObject>().Spawn(true);
            SetCardValuesClientRpc(i + 1, Elements.Element.FIRE);

            GameObject nextMCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextMCard.GetComponent<NetworkObject>().Spawn(true);
            SetCardValuesClientRpc(i + 1, Elements.Element.METAL);

            GameObject nextWaCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextWaCard.GetComponent<NetworkObject>().Spawn(true);
            SetCardValuesClientRpc(i + 1, Elements.Element.WATER);

            GameObject nextWoCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextWoCard.GetComponent<NetworkObject>().Spawn(true);
            SetCardValuesClientRpc(i + 1, Elements.Element.WOOD);

            AddCard(ref deck, nextECard, ref slot);
            AddCard(ref deck, nextFCard,ref  slot);
            AddCard(ref deck, nextMCard, ref slot);
            AddCard(ref deck, nextWaCard, ref slot);
            AddCard(ref deck, nextWoCard, ref slot);
        }
    }

    private void AddCard(ref GameObject[] deck, GameObject card, ref int slot)
    {
        deck[slot] = card;
        ++slot;
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < 600; i++)
        {
            int pos1 = Random.Range(0, 50);
            int pos2 = Random.Range(0, 50);
            GameObject temp = deck[pos1];
            deck[pos1] = deck[pos2];
            deck[pos2] = temp;
        }
    }
}
