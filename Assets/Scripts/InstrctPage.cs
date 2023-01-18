using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstrctPage : MonoBehaviour
{
    public GameObject nextPage;
    public GameObject previousPage;
    public GameObject mainMenu;
    public bool isNextButton;
    public bool toMain;

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
        if (toMain)
        {
            Vector3 nextPos = mainMenu.transform.position;
            Camera.main.transform.position = new Vector3(nextPos.x, nextPos.y, Camera.main.transform.position.z);
        }
        else if (isNextButton)
        {
            Vector3 nextPos = nextPage.transform.position;
            Camera.main.transform.position = new Vector3(nextPos.x, nextPos.y, Camera.main.transform.position.z);
        }
        else
        {
            Vector3 nextPos = previousPage.transform.position;
            Camera.main.transform.position = new Vector3(nextPos.x, nextPos.y, Camera.main.transform.position.z);
        }
    }
}
