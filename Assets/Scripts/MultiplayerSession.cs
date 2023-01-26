using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerSession : NetworkBehaviour
{
    //private NetworkVariable<NetworkUintArray> playerDeck = new NetworkVariable<NetworkUintArray>(new NetworkUintArray { deck = new uint[50] });
    //private NetworkVariable<NetworkUintArray> opponentDeck = new NetworkVariable<NetworkUintArray>(new NetworkUintArray { deck = new uint[50] });
    PlayerDeck player1Deck;
    PlayerDeck player2Deck;
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
        player1Deck = GameObject.FindGameObjectWithTag("PlayerDraw").GetComponent<PlayerDeck>();
        player2Deck = GameObject.FindGameObjectWithTag("OpponentDraw").GetComponent<PlayerDeck>();
        //playerDeck.Value = new NetworkDeckArray();//GameObject.FindGameObjectWithTag("PlayerDraw").GetComponent<PlayerDeck>();
        //opponentDeck.Value = new NetworkDeckArray();//GameObject.FindGameObjectWithTag("OpponentDraw").GetComponent<PlayerDeck>();
        playerHand = new GameObject[5];
        opponentHand = new GameObject[5];
        source = GetComponent<AudioSource>();

        if (!IsHost) return;

        while (NetworkManager.ConnectedClientsIds.Count != 2) await Task.Delay(100);

        NetworkLog.LogInfoServer("2 Players reached!");

        await Task.Delay(100);

        player1Deck.InitializeDeck();
        player2Deck.InitializeDeck();
        ShuffleDecksClientRpc();

        StartDealingServerRpc(new ServerRpcParams());

        while (!(NetworkManager.ConnectedClients[0].PlayerObject.GetComponent<PlayerInfo>().pickedCard.Value &&
                 NetworkManager.ConnectedClients[1].PlayerObject.GetComponent<PlayerInfo>().pickedCard.Value))
            await Task.Delay(100);
        ResetPickedCardsClientRpc();

        GameLoopServerRpc();
    }

    [ServerRpc]
    private void GameLoopServerRpc()
    {
        DisableHandsClientRpc();

        PlayCardsClientRpc(NetworkManager.ConnectedClients[0].PlayerObject
            .GetComponent<PlayerInfo>().selectedCardId.Value, NetworkManager.ConnectedClients[1].PlayerObject
            .GetComponent<PlayerInfo>().selectedCardId.Value);


    }

    [ClientRpc]
    private void PlayCardsClientRpc(ulong cardId1, ulong cardId2)
    {
        ulong playerCardId = (cardId1 == NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerInfo>().selectedCardId.Value) ?
            cardId1 : cardId2;
        ulong opponentCardId = (cardId1 == NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerInfo>().selectedCardId.Value) ?
            cardId2 : cardId1;

        GameObject playerCard = PlayerDeck.GetCardById(playerCardId);
        GameObject opponentCard = PlayerDeck.GetCardById(opponentCardId);

        Vector2 cardPos = playerCard.transform.position;
        Vector2 destPos = GameObject.FindGameObjectWithTag("PlayerPlay").transform.position;
        playerCard.GetComponent<Animator>().SetTrigger("Unflip");
        StartCoroutine(MoveCard(playerCard, cardPos, destPos, new Vector2(0.25f, 0.25f), new Vector2(0.15f, 0.15f)));

        //Opponent
        Vector2 oppCardPos = opponentCard.transform.position;
        Vector2 oppDestPos = GameObject.FindGameObjectWithTag("OpponentPlay").transform.position;
        StartCoroutine(MoveCard(opponentCard, oppCardPos, oppDestPos, new Vector2(0.25f, 0.25f), new Vector2(0.15f, 0.15f)));

        cardsDealt -= 2;
        StartCoroutine(WaitAndFlip());

        playerCard.GetComponent<Animator>().SetTrigger("Flip");
        opponentCard.GetComponent<Animator>().SetTrigger("FlipU");

        StartCoroutine(WaitAndBoost());
        //selectionStage = false;
    }

    [ClientRpc]
    private void DisableHandsClientRpc()
    {
        foreach (GameObject card in playerHand)
        {
            card.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    [ClientRpc]
    private void ResetPickedCardsClientRpc()
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerInfo>().pickedCard.Value = false;
    }

    [ClientRpc]
    private void ShuffleDecksClientRpc()
    {
        player1Deck.ShuffleDeck();
        player2Deck.ShuffleDeck();
    }

    [ClientRpc]
    private void SetupDecksClientRpc(ClientRpcParams clientParams)
    {
        //NetworkLog.LogInfoServer("Switched decks!");
        PlayerDeck temp = player1Deck;

        player1Deck = player2Deck;
        player1Deck.transform.position = player2Deck.transform.position;
        player2Deck = temp;
        player2Deck.transform.position = temp.transform.position;
    }

    [ClientRpc]
    private void DealClientRpc()
    {
        //NetworkLog.LogInfoServer("Dealing! p1: " + player1Deck.name + ", p2: " + player2Deck.name);
        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(DealPlayer(i + 1, i));
            StartCoroutine(DealOpponent(i + 1, i));
        }

        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerInfo>().SetReadyToPick(true);
    }

    [ServerRpc]
    private void StartDealingServerRpc(ServerRpcParams serverParams)
    {
        //NetworkLog.LogInfoServer("Dealing! Sent by: " + serverParams.Receive.SenderClientId);
        SetupDecksClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });

        DealClientRpc();
    }

    [ServerRpc]
    private void RequestReshuffleServerRpc()
    {
        ShuffleDecksClientRpc();
    }

    IEnumerator DealPlayer(int waitTime, int handPos)
    {
        //NetworkLog.LogInfoServer("Dealing player!");
        yield return new WaitForSeconds(waitTime * 0.2f);

        playerHand[handPos] = player1Deck.deck[drawCount];
        player1Deck.deck[drawCount].transform.position = GameObject.FindGameObjectWithTag("PlayerDraw").transform.position;
        Vector2 cardPos = player1Deck.deck[drawCount].transform.position;
        Vector2 destPos = new Vector2(-3.6f + (handPos * 1.8f), -4.4f);
        player1Deck.deck[drawCount].GetComponent<Animator>().SetTrigger("Flip");
        StartCoroutine(MoveCard(player1Deck.deck[drawCount], cardPos, destPos, new Vector2(0.15f, 0.15f), new Vector2(0.25f, 0.25f), true));
        ++cardsDealt;
    }

    IEnumerator DealOpponent(int waitTime, int handPos)
    {
        //NetworkLog.LogInfoServer("Dealing opponent!");
        yield return new WaitForSeconds(waitTime * 0.2f);
        opponentHand[handPos] = player2Deck.deck[drawCount];
        player2Deck.deck[drawCount].transform.position = GameObject.FindGameObjectWithTag("OpponentDraw").transform.position;
        Vector2 cardPos = player2Deck.deck[drawCount].transform.position;
        Vector2 destPos = new Vector2(-3.6f + (handPos * 1.8f), 4.5f);
        StartCoroutine(MoveCard(player2Deck.deck[drawCount], cardPos, destPos, new Vector2(0.15f, 0.15f), new Vector2(0.25f, 0.25f)));
        ++cardsDealt;
        if (drawCount == 49)
        {
            drawCount = 0;
            RequestReshuffleServerRpc();
            //ShuffleDeck(playerDeck.Value);
            //ShuffleDeck(opponentDeck.Value);
        }
        else
        {
            ++drawCount;
        }
    }

    IEnumerator MoveCard(GameObject card, Vector2 startPos, Vector2 destPos, Vector2 startSize, Vector2 endSize, bool makePickable = false)
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
        if (makePickable) card.GetComponent<BoxCollider2D>().enabled = true;
        yield return null;
    }

    IEnumerator WaitAndFlip()
    {
        yield return new WaitForSeconds(1);

        //revealStage = true;
    }

    IEnumerator WaitAndBoost()
    {
        yield return new WaitForSeconds(1);

        //boostStage = true;
    }

    private void OnApplicationQuit()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
