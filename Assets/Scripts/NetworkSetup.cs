using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class NetworkSetup : MonoBehaviour
{
    //[SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private GameObject netManagerPrefab;

    private Options gameOptions;

    private async void Awake()
    {
        if (GameObject.FindGameObjectWithTag("NetworkManager") == null)
        {
            Instantiate(netManagerPrefab, transform.position, Quaternion.identity);
        }

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        gameOptions = GameObject.FindGameObjectWithTag("Options").GetComponent<Options>();

        hostBtn.onClick.AddListener(async () =>
        {
            hostBtn.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            hostBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Creating Game...";
            hostBtn.interactable = false;
            if (!(await CreateRelay()))
            {
                hostBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Couldn't Create Game";
                await Task.Delay(2500);
                hostBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Host";
                hostBtn.interactable = true;
            }
        });
        clientBtn.onClick.AddListener(async () =>
        {
            clientBtn.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            clientBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Joining Game...";
            clientBtn.interactable = false;
            TMP_InputField inputField = GetComponentInChildren<TMP_InputField>();
            if (! (await JoinRelay(inputField.text)))
            {
                clientBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Couldn't Join Game";
                await Task.Delay(2500);
                clientBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Join";
                clientBtn.interactable = true;
            }
        });
    }

    private async Task<bool> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            gameOptions.joinCode = joinCode;
            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            gameOptions.multiplayer = true;
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    private async Task<bool> JoinRelay(string joinCode)
    {
        if (String.IsNullOrEmpty(joinCode) || String.IsNullOrWhiteSpace(joinCode))
        {
            return false;
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            gameOptions.multiplayer = true;
            return NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }
}
