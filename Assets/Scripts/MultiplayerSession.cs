using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerSession : NetworkBehaviour
{
    private NetworkVariable<NetworkUintArray> playerDeck = new NetworkVariable<NetworkUintArray>(new NetworkUintArray { deck = new uint[50] });
    private NetworkVariable<NetworkUintArray> opponentDeck = new NetworkVariable<NetworkUintArray>(new NetworkUintArray { deck = new uint[50] });
    GameObject[] playerHand;
    GameObject[] opponentHand;
    public GameObject selectedCard;
    GameObject opponentCard;
    Elements.Element winningType;
    AudioSource source;
    bool readyToStart = false;
    bool hasStarted = false;
    bool selectionStage = false;
    bool revealStage = false;
    bool boostStage = false;
    bool checkBoost = false;
    bool winnerStage = false;
    bool clearingStage = false;
    bool pointsStage = false;
    bool wincheckStage = false;
    bool restartStage = false;
    bool playerWon = false;
    bool wasTie = false;
    bool wasBoosted = false;
    bool overallWinner = false;
    int drawCount = 0;
    int cardsDealt = 0;
    int playerFireWins = 0;
    int playerEarthWins = 0;
    int playerMetalWins = 0;
    int playerWaterWins = 0;
    int playerWoodWins = 0;
    int oppFireWins = 0;
    int oppEarthWins = 0;
    int oppMetalWins = 0;
    int oppWaterWins = 0;
    int oppWoodWins = 0;

    public override async void OnNetworkSpawn()
    {
        NetworkLog.LogInfoServer("Started multiplayer!");
        //playerDeck.Value = new NetworkDeckArray();//GameObject.FindGameObjectWithTag("PlayerDraw").GetComponent<PlayerDeck>();
        //opponentDeck.Value = new NetworkDeckArray();//GameObject.FindGameObjectWithTag("OpponentDraw").GetComponent<PlayerDeck>();
        playerHand = new GameObject[5];
        opponentHand = new GameObject[5];
        source = GetComponent<AudioSource>();

        while (NetworkManager.ConnectedClientsIds.Count != 2) await Task.Delay(100);

        NetworkLog.LogInfoServer("2 Players reached!");

        PlayerDeck pDeck = GameObject.FindGameObjectWithTag("PlayerDraw").GetComponent<PlayerDeck>();
        pDeck.InitializeDeck(); //TODO: This line makes host freeze when client joins
        /*pDeck.ShuffleDeck();

        playerDeck.Value.SetDeck(pDeck.ReturnDeckArray());
        NetworkManager.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<PlayerInfo>().playerDeck = playerDeck;

        // PROBLEM IS SOMEWHERE BETWEEN THE LOG AND HERE
        ulong opponentId = 0;
        foreach (ulong playerId in NetworkManager.ConnectedClientsIds)
        {
            if (OwnerClientId == playerId) continue;

            opponentId = playerId;
        }

        while (!NetworkManager.ConnectedClients[opponentId].PlayerObject.GetComponent<PlayerInfo>().deckReady.Value)
            await Task.Delay(100);
        opponentDeck = NetworkManager.ConnectedClients[opponentId].PlayerObject.GetComponent<PlayerInfo>().playerDeck;

        StartDealingServerRpc(new ServerRpcParams());*/
    }

    [ServerRpc]
    private void StartDealingServerRpc(ServerRpcParams serverParams)
    {
        NetworkLog.LogInfoServer("Dealing! Sent by: " + serverParams.Receive.SenderClientId);

        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(DealPlayer(i + 1, i));
            StartCoroutine(DealOpponent(i + 1, i));
        }
    }

    IEnumerator DealPlayer(int waitTime, int handPos)
    {
        yield return new WaitForSeconds(waitTime * 0.2f);
        playerHand[handPos] = PlayerDeck.GetCardById(playerDeck.Value.deck[drawCount]);
        PlayerDeck.GetCardById(playerDeck.Value.deck[drawCount]).transform.position = GameObject.FindGameObjectWithTag("PlayerDraw").transform.position;
        Vector2 cardPos = PlayerDeck.GetCardById(playerDeck.Value.deck[drawCount]).transform.position;
        Vector2 destPos = new Vector2(-3.6f + (handPos * 1.8f), -4.4f);
        PlayerDeck.GetCardById(playerDeck.Value.deck[drawCount]).GetComponent<Animator>().SetTrigger("Flip");
        StartCoroutine(MoveCard(PlayerDeck.GetCardById(playerDeck.Value.deck[drawCount]), cardPos, destPos, new Vector2(0.15f, 0.15f), new Vector2(0.25f, 0.25f)));
        ++cardsDealt;
    }
    IEnumerator DealOpponent(int waitTime, int handPos)
    {
        yield return new WaitForSeconds(waitTime * 0.2f);
        opponentHand[handPos] = PlayerDeck.GetCardById(opponentDeck.Value.deck[drawCount]);
        PlayerDeck.GetCardById(opponentDeck.Value.deck[drawCount]).transform.position = GameObject.FindGameObjectWithTag("OpponentDraw").transform.position;
        Vector2 cardPos = PlayerDeck.GetCardById(opponentDeck.Value.deck[drawCount]).transform.position;
        Vector2 destPos = new Vector2(-3.6f + (handPos * 1.8f), 4.5f);
        StartCoroutine(MoveCard(PlayerDeck.GetCardById(opponentDeck.Value.deck[drawCount]), cardPos, destPos, new Vector2(0.15f, 0.15f), new Vector2(0.25f, 0.25f)));
        ++cardsDealt;
        if (drawCount == 49)
        {
            drawCount = 0;
            //ShuffleDeck(playerDeck.Value);
            //ShuffleDeck(opponentDeck.Value);
            // TODO: bring this back
        }
        else
        {
            ++drawCount;
        }
    }

    IEnumerator MoveCard(GameObject card, Vector2 startPos, Vector2 destPos, Vector2 startSize, Vector2 endSize)
    {
        float lerpTime = 0;
        source.Play();
        card.transform.Find("DeathCard").GetComponent<SpriteRenderer>().sortingOrder = 1;
        card.GetComponent<SpriteRenderer>().sortingOrder = 2;
        card.transform.Find("Number").GetComponent<TMPro.TextMeshPro>().sortingOrder = 3;
        card.transform.Find("Type").GetComponent<TMPro.TextMeshPro>().sortingOrder = 3;
        card.transform.Find("Symbol").GetComponent<SpriteRenderer>().sortingOrder = 3;
        while (lerpTime < 1.1f)
        {
            card.transform.position = Vector2.Lerp(startPos, destPos, lerpTime);
            card.transform.localScale = Vector2.Lerp(startSize, endSize, lerpTime);
            lerpTime += Time.deltaTime * 3;

            yield return null;
        }
        card.transform.Find("DeathCard").GetComponent<SpriteRenderer>().sortingOrder = 3;
        card.GetComponent<SpriteRenderer>().sortingOrder = 4;
        card.transform.Find("Number").GetComponent<TMPro.TextMeshPro>().sortingOrder = 5;
        card.transform.Find("Type").GetComponent<TMPro.TextMeshPro>().sortingOrder = 5;
        card.transform.Find("Symbol").GetComponent<SpriteRenderer>().sortingOrder = 5;
        card.transform.position = destPos;
        yield return null;
    }
}
