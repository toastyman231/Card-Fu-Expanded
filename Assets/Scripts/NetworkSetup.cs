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

    private void Awake()
    {
        /*serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });*/
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            //SceneManager.LoadScene("Game", LoadSceneMode.Single);
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        });
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            //SceneManager.LoadScene("Game", LoadSceneMode.Single);
        });
    }
}
