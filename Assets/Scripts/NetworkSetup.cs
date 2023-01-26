using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkSetup : MonoBehaviour
{
    //[SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private GameObject netManagerPrefab;

    private Options gameOptions;

    private void Awake()
    {
        if (GameObject.FindGameObjectWithTag("NetworkManager") == null)
        {
            Instantiate(netManagerPrefab, transform.position, Quaternion.identity);
        }

        gameOptions = GameObject.FindGameObjectWithTag("Options").GetComponent<Options>();
        /*serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });*/
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            //SceneManager.LoadScene("Game", LoadSceneMode.Single);
            gameOptions.multiplayer = true;
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        });
        clientBtn.onClick.AddListener(() =>
        {
            //if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 1) return;
            // TODO: Remember to limit players to 2
            gameOptions.multiplayer = true;
            NetworkManager.Singleton.StartClient();
            //SceneManager.LoadScene("Game", LoadSceneMode.Single);
        });
    }
}
