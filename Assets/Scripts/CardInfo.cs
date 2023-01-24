using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfo : MonoBehaviour
{
    private uint _cardId;

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

    private void Start()
    {
        cardType.text = type.ToString();
        cardValue.text = value.ToString();
        source = GetComponent<AudioSource>();

        switch (type)
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

    public uint GetCardId()
    {
        return _cardId;
    }

    public void SetCardId(uint newId)
    {
        _cardId = newId;
    }
}
