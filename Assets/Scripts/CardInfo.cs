using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CardInfo : NetworkBehaviour
{
    public static CardInfo recentCard;

    public int value;
    public Elements.Element type;
    public AudioClip death;
    AudioSource source;

    public TMPro.TextMeshPro cardType;
    public TMPro.TextMeshPro cardValue;
    public SpriteRenderer typeImage;

    public Sprite earthSymbol;
    public Sprite fireSymbol;
    public Sprite metalSymbol;
    public Sprite waterSymbol;
    public Sprite woodSymbol;

    public override void OnNetworkSpawn()
    {
        recentCard = this;
        source = GetComponent<AudioSource>();
        if (IsHost) return;

        PlayerDeck player1Deck = GameObject.FindGameObjectWithTag("PlayerDraw").GetComponent<PlayerDeck>();
        if (!player1Deck.IsFull())
        {
            NetworkLog.LogInfoServer("Adding to p1 deck");
            player1Deck.AddCard(gameObject);
        }
        else
        {
            NetworkLog.LogInfoServer("P1 full, adding to p2");
            PlayerDeck player2Deck = GameObject.FindGameObjectWithTag("OpponentDraw").GetComponent<PlayerDeck>();
            if (!player2Deck.IsFull())
            {
                player2Deck.AddCard(gameObject);
            }
            else
            {
                NetworkLog.LogInfoServer("P2 full");
            }
        }
    }

    public override string ToString()
    {
        return type + " " + value;
    }

    public void SetupCard(int val, Elements.Element element)
    {
        value = val;
        type = element;
        cardType.text = element.ToString();
        cardValue.text = val.ToString();

        switch (element)
        {
            case Elements.Element.EARTH:
                typeImage.sprite = earthSymbol;
                break;
            case Elements.Element.FIRE:
                typeImage.sprite = fireSymbol;
                break;
            case Elements.Element.METAL:
                typeImage.sprite = metalSymbol;
                break;
            case Elements.Element.WATER:
                typeImage.sprite = waterSymbol;
                break;
            case Elements.Element.WOOD:
                typeImage.sprite = woodSymbol;
                break;
        }
    }

    public void PlayDeath()
    {
        source.clip = death;
        source.Play();
    }
}
