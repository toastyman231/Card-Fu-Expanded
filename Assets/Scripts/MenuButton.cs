using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MenuButton : NetworkBehaviour
{
    public GameObject[] symbols;
    public Canvas multiplayerCanvas;
    public bool isStart;
    public bool isMultiplayer;
    public bool isRestart;
    public bool isInstructions;
    public bool isOptions;
    public bool isCredits;
    public bool isQuit;

    private Options options;

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

    [ServerRpc(RequireOwnership = false)]
    private void RestartGameServerRpc()
    {
        NetworkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    [ServerRpc(RequireOwnership = false)]
    private void QuitGameServerRpc()
    {
        NetworkManager.Shutdown();
    }

    private void OnMouseDown()
    {
        options = GameObject.FindGameObjectWithTag("Options").GetComponent<Options>();

        if (isStart)
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
        else if (isMultiplayer)
        {
            Vector2 newCamPos = GameObject.FindGameObjectWithTag("Multiplayer").transform.position;
            Camera.main.transform.position = new Vector3(newCamPos.x, newCamPos.y, Camera.main.transform.position.z);
            multiplayerCanvas.GetComponent<Canvas>().enabled = true;
        }
        else if (isRestart)
        {
            if (options.multiplayer && IsHost) NetworkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            else if (options.multiplayer) RestartGameServerRpc();
            else SceneManager.LoadScene("Game", LoadSceneMode.Single);
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
            if (options.multiplayer)
            {
                QuitGameServerRpc();
            }
            else
            {
                Debug.Log("Quitting!");
                GameObject[] buttons = GameObject.FindGameObjectsWithTag("MenuItem");
                foreach (GameObject button in buttons)
                {
                    button.GetComponent<Collider2D>().enabled = false;
                }
                GetComponent<AudioSource>().clip = Resources.Load<AudioClip>("Sounds/FDeath");
                GetComponent<AudioSource>().Play();
                GameObject.FindGameObjectWithTag("Main").GetComponent<Animator>().SetTrigger("Quit");
            }
        }
    }
}
