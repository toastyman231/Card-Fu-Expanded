using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpMove : MonoBehaviour
{
    public bool isNextButton;

    private void OnMouseOver()
    {
        GetComponent<TMPro.TextMeshPro>().color = new Color(0.75f, 0, 0);
    }

    private void OnMouseExit()
    {
        GetComponent<TMPro.TextMeshPro>().color = Color.black;
    }

    private void OnMouseDown()
    {
        if (isNextButton)
        {
            Instantiate(Resources.Load<GameObject>("HelpSheet2"));
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Instantiate(Resources.Load<GameObject>("HelpSheet1"));
            Destroy(transform.parent.gameObject);
        }
    }
}
