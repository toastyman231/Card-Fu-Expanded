using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuButton : MonoBehaviour
{
    public GameObject[] symbols;
    public bool isStart;
    public bool isRestart;
    public bool isInstructions;
    public bool isOptions;
    public bool isCredits;
    public bool isQuit;


    private void OnMouseEnter()
    {
        symbols[0].GetComponent<SpriteRenderer>().enabled = true;
        symbols[1].GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<TMPro.TextMeshPro>().color = new Color(0.75f, 0, 0, 1);
        GetComponent<AudioSource>().Play();
    }

    private void OnMouseExit()
    {
        symbols[0].GetComponent<SpriteRenderer>().enabled = false;
        symbols[1].GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<TMPro.TextMeshPro>().color = Color.black;
    }

    private void OnMouseDown()
    {
        if (isStart)
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
        else if (isRestart)
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
        else if (isInstructions)
        {
            Vector2 newCamPos = GameObject.FindGameObjectWithTag("Instructions").transform.position;
            Camera.main.transform.position = new Vector3(newCamPos.x, newCamPos.y, Camera.main.transform.position.z);
        }
        else if (isOptions)
        {
            Vector2 newCamPos = GameObject.FindGameObjectWithTag("Options").transform.position;
            Camera.main.transform.position = new Vector3(newCamPos.x, newCamPos.y, Camera.main.transform.position.z);
        }
        else if (isCredits)
        {
            Vector2 newCamPos = GameObject.FindGameObjectWithTag("Credits").transform.position;
            Camera.main.transform.position = new Vector3(newCamPos.x, newCamPos.y, Camera.main.transform.position.z);
        }
        else if (isQuit)
        {
            GameObject[] buttons = GameObject.FindGameObjectsWithTag("MenuItem");
            foreach(GameObject button in buttons)
            {
                button.GetComponent<Collider2D>().enabled = false;
            }
            GetComponent<AudioSource>().clip = Resources.Load<AudioClip>("Sounds/FDeath");
            GetComponent<AudioSource>().Play();
            GameObject.FindGameObjectWithTag("Main").GetComponent<Animator>().SetTrigger("Quit");
        }
    }
}
