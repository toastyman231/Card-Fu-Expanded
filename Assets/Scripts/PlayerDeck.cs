using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public GameObject[] deck;

    private void Awake()
    {
        int slot = 0;
        deck = new GameObject[50];

        for(int i = 0; i < 10; i++)
        {
            GameObject nextECard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextECard.GetComponent<CardInfo>().value = i + 1;
            nextECard.GetComponent<CardInfo>().type = Elements.Element.EARTH;

            GameObject nextFCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextFCard.GetComponent<CardInfo>().value = i + 1;
            nextFCard.GetComponent<CardInfo>().type = Elements.Element.FIRE;

            GameObject nextMCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextMCard.GetComponent<CardInfo>().value = i + 1;
            nextMCard.GetComponent<CardInfo>().type = Elements.Element.METAL;

            GameObject nextWaCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextWaCard.GetComponent<CardInfo>().value = i + 1;
            nextWaCard.GetComponent<CardInfo>().type = Elements.Element.WATER;

            GameObject nextWoCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextWoCard.GetComponent<CardInfo>().value = i + 1;
            nextWoCard.GetComponent<CardInfo>().type = Elements.Element.WOOD;

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
}
