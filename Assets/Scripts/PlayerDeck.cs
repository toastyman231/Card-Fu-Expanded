using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerDeck : NetworkBehaviour
{
    public GameObject[] deck;
    public int slot;

    public override void OnNetworkSpawn()
    {
        deck = new GameObject[50];
        slot = 0;
    }

    [ClientRpc]
    private void SetCardValuesClientRpc(int val, Elements.Element element)
    {
        CardInfo.recentCard.SetupCard(val, element);
    }

    public void InitializeDeck()
    {
        //NetworkLog.LogInfoServer("Initializing deck");
        //int slot = 0;
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

            AddCard(nextECard);
            AddCard(nextFCard);
            AddCard(nextMCard);
            AddCard(nextWaCard);
            AddCard(nextWoCard);
        }
    }

    public void InitializeDeckSingleplayer()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject nextECard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextECard.GetComponent<CardInfo>().SetupCard(i + 1, Elements.Element.EARTH);

            GameObject nextFCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextFCard.GetComponent<CardInfo>().SetupCard(i + 1, Elements.Element.FIRE);

            GameObject nextMCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextMCard.GetComponent<CardInfo>().SetupCard(i + 1, Elements.Element.METAL);

            GameObject nextWaCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextWaCard.GetComponent<CardInfo>().SetupCard(i + 1, Elements.Element.WATER);

            GameObject nextWoCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextWoCard.GetComponent<CardInfo>().SetupCard(i + 1, Elements.Element.WOOD);

            AddCard(nextECard);
            AddCard(nextFCard);
            AddCard(nextMCard);
            AddCard(nextWaCard);
            AddCard(nextWoCard);
        }
    }

    public void AddCard(GameObject card)
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
            (deck[pos1], deck[pos2]) = (deck[pos2], deck[pos1]);
        }
    }

    public bool IsFull()
    {
        return slot == deck.Length;
    }

    public static GameObject GetCardById(ulong cardId)
    {
        foreach (GameObject card in GameObject.FindGameObjectsWithTag("Card"))
        {
            if (card.GetComponent<NetworkObject>().NetworkObjectId == cardId)
            {
                return card;
            }
        }

        return null;
    }
}
