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
        SetupCard(value, type);
        source = GetComponent<AudioSource>();        
    }

    public void SetupCard(int value, Elements.Element element)
    {
        cardType.text = element.ToString();
        cardValue.text = value.ToString();

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
