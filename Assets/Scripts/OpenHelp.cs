using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenHelp : MonoBehaviour
{
    bool isOpen = false;

    private void OnMouseDown()
    {
        if (isOpen)
        {
            GetComponent<TMPro.TextMeshPro>().color = new Color(0, 0, 0, 0.49f);
            GameObject sheet1 = GameObject.FindGameObjectWithTag("HS1");
            GameObject sheet2 = GameObject.FindGameObjectWithTag("HS2");
            if (sheet1 != null)
            {
                Destroy(sheet1);
            }
            else
            {
                Destroy(sheet2);
            }
            isOpen = false;
        }
        else
        {
            GetComponent<TMPro.TextMeshPro>().color = Color.black;
            Instantiate(Resources.Load<GameObject>("HelpSheet1"));
            isOpen = true;
        }
        
    }
}
