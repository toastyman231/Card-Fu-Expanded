using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        GetComponentInChildren<TMPro.TextMeshProUGUI>().color = new Color(0.75f, 0, 0, 1);
        GetComponent<AudioSource>().Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;

        GetComponentInChildren<TMPro.TextMeshProUGUI>().color = Color.black;
    }
}
