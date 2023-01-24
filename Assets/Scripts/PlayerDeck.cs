using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerDeck : NetworkBehaviour
{
    public GameObject[] deck;

    private static List<uint> _ids;

    private uint _nextId;

    private bool _nextIdReady;

    public override void OnNetworkSpawn()
    {
        _nextIdReady = false;
        deck = new GameObject[50];
        if (IsServer)
        {
            _ids = new List<uint>();
            NetworkLog.LogInfoServer("Server Ids setup ready");
        }
        else
        {
            _ids = new List<uint> { 0 };
            NetworkLog.LogInfoServer("Client Ids setup ready");
        }
    }

    public void InitializeDeck()
    {
        Debug.Log("Initializing deck!");
        NetworkLog.LogInfoServer("Initializing deck");
        int slot = 0;
        for (int i = 0; i < 10; i++)
        {
            GameObject nextECard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            Debug.Log("Instantiated card");
            nextECard.GetComponent<NetworkObject>().Spawn(true);
            nextECard.GetComponent<CardInfo>().value = i + 1;
            nextECard.GetComponent<CardInfo>().type = Elements.Element.EARTH;
            //Debug.Log("Requesting ID!");
            //RequestCardIdServerRpc(new ServerRpcParams());
            //while (!_nextIdReady) await Task.Delay(100);
            //Debug.Log("Found Id");
            //nextECard.GetComponent<CardInfo>().SetCardId(_nextId);
            //_nextIdReady = false;

            GameObject nextFCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextFCard.GetComponent<NetworkObject>().Spawn(true);
            nextFCard.GetComponent<CardInfo>().value = i + 1;
            nextFCard.GetComponent<CardInfo>().type = Elements.Element.FIRE;
            //RequestCardIdServerRpc(new ServerRpcParams());
            //while (!_nextIdReady) await Task.Delay(100);
            //nextFCard.GetComponent<CardInfo>().SetCardId(_nextId);
            //_nextIdReady = false;

            GameObject nextMCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextMCard.GetComponent<NetworkObject>().Spawn(true);
            nextMCard.GetComponent<CardInfo>().value = i + 1;
            nextMCard.GetComponent<CardInfo>().type = Elements.Element.METAL;
            //RequestCardIdServerRpc(new ServerRpcParams());
            //while (!_nextIdReady) await Task.Delay(100);
            //nextMCard.GetComponent<CardInfo>().SetCardId(_nextId);
            //_nextIdReady = false;

            GameObject nextWaCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextWaCard.GetComponent<NetworkObject>().Spawn(true);
            nextWaCard.GetComponent<CardInfo>().value = i + 1;
            nextWaCard.GetComponent<CardInfo>().type = Elements.Element.WATER;
            //RequestCardIdServerRpc(new ServerRpcParams());
            //while (!_nextIdReady) await Task.Delay(100);
            //nextWaCard.GetComponent<CardInfo>().SetCardId(_nextId);
            //_nextIdReady = false;

            GameObject nextWoCard = Instantiate(Resources.Load("Card"), transform.position, transform.rotation) as GameObject;
            nextWoCard.GetComponent<NetworkObject>().Spawn(true);
            nextWoCard.GetComponent<CardInfo>().value = i + 1;
            nextWoCard.GetComponent<CardInfo>().type = Elements.Element.WOOD;
            //RequestCardIdServerRpc(new ServerRpcParams());
            //while (!_nextIdReady) await Task.Delay(100);
            //nextWoCard.GetComponent<CardInfo>().SetCardId(_nextId);
            //_nextIdReady = false;

            AddCard(ref deck, nextECard, ref slot);
            AddCard(ref deck, nextFCard, ref slot);
            AddCard(ref deck, nextMCard, ref slot);
            AddCard(ref deck, nextWaCard, ref slot);
            AddCard(ref deck, nextWoCard, ref slot);
        }
    }

    public static GameObject GetCardById(uint id)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Card"))
        {
            if (go.GetComponent<CardInfo>().GetCardId() == id) return go;
        }

        return null;
    }

    public uint[] ReturnDeckArray()
    {
        uint[] res = new uint[50];

        for (int i = 0; i < 50; i++)
        {
            res[i] = deck[i].GetComponent<CardInfo>().GetCardId();
        }

        return res;
    }

    [ServerRpc]
    private void RequestCardIdServerRpc(ServerRpcParams serverParams)
    {
        NetworkLog.LogInfoServer("Requesting card ID");
        uint id = (uint)Random.Range(1f, 900f);

        while (!_ids.Contains(id))
        {
            id = (uint)Random.Range(1f, 900f);
        }

        _ids.Add(id);

        ReturnCardIdClientRpc(id, new ClientRpcParams{Send = new ClientRpcSendParams
        {
            TargetClientIds = new List<ulong>{serverParams.Receive.SenderClientId}
        }});
    }

    [ClientRpc]
    private void ReturnCardIdClientRpc(uint id, ClientRpcParams clientParams)
    {
        NetworkLog.LogInfoServer("Created card with id: " + id);
        _nextId = id;
        _nextIdReady = true;
    }

    private void AddCard(ref GameObject[] deck, GameObject card, ref int slot)
    {
        deck[slot] = card;
        ++slot;
    }

    public void ShuffleDeck()
    {
        NetworkLog.LogInfoServer("Shuffling deck");
        for (int i = 0; i < 600; i++)
        {
            int pos1 = Random.Range(0, 50);
            int pos2 = Random.Range(0, 50);
            (deck[pos1], deck[pos2]) = (deck[pos2], deck[pos1]);
        }
    }
}
