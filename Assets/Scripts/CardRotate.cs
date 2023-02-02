using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardRotate : MonoBehaviour
{
    [SerializeField] private Sprite cardFront;
    [SerializeField] private Sprite cardBack;
    [SerializeField] private float rotationSpeed;

    private Image _cardImage;

    void Start()
    {
        _cardImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed);

        if ((transform.eulerAngles.y >= 268f && transform.eulerAngles.y <= 271f) || 
            (transform.eulerAngles.y >= 88f && transform.eulerAngles.y <= 91f))
        {
            _cardImage.sprite = (_cardImage.sprite == cardFront) ? cardBack : cardFront;
        }
    }
}
