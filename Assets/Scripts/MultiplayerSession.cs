using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerSession : NetworkBehaviour
{
    private readonly int FIRE = 0;

    private readonly int EARTH = 1;

    private readonly int METAL = 2;

    private readonly int WATER = 3;

    private readonly int WOOD = 4;

    //private NetworkVariable<NetworkUintArray> playerDeck = new NetworkVariable<NetworkUintArray>(new NetworkUintArray { deck = new uint[50] });
    //private NetworkVariable<NetworkUintArray> opponentDeck = new NetworkVariable<NetworkUintArray>(new NetworkUintArray { deck = new uint[50] });
    PlayerDeck player1Deck;
    PlayerDeck player2Deck;
    GameObject[] playerHand;
    GameObject[] opponentHand;
    public GameObject selectedCard;
    GameObject opponentCard;
    Elements.Element winningType;
    private Elements.Element losingType;
    AudioSource source;
    private ulong roundWinnerId;
    private List<ulong> roundResults;
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
    //int playerFireWins = 0;
    //int playerEarthWins = 0;
    //int playerMetalWins = 0;
    //int playerWaterWins = 0;
    //int playerWoodWins = 0;
    //int oppFireWins = 0;
    //int oppEarthWins = 0;
    //int oppMetalWins = 0;
    //int oppWaterWins = 0;
    //int oppWoodWins = 0;

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

        roundResults = new List<ulong>();

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

        roundResults.Clear();
        PlayCardsClientRpc(NetworkManager.ConnectedClients[0].PlayerObject
            .GetComponent<PlayerInfo>().selectedCardId.Value, NetworkManager.ConnectedClients[1].PlayerObject
            .GetComponent<PlayerInfo>().selectedCardId.Value);

        WaitForResults();
    }

    private async void WaitForResults()
    {
        while (roundResults.Count < 2) await Task.Delay(100);

        await Task.Delay(1000);

        if (roundResults[0] != roundResults[1])
        {
            // logic for if the clients disagree
            NetworkLog.LogWarningServer("Clients disagree on winner!");
        }
        else
        {
            // Show results to clients
            ulong loserCardId = NetworkManager.ConnectedClients[roundResults[0]].PlayerObject.GetComponent<PlayerInfo>()
                .selectedCardId.Value;
            ulong winnerCardId = NetworkManager.ConnectedClients[roundWinnerId].PlayerObject.GetComponent<PlayerInfo>()
                .selectedCardId.Value;
            DisplayResultsClientRpc(roundResults[0], winnerCardId, loserCardId);
        }
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

        StartCoroutine(FlipCards(1f, playerCard, opponentCard));

        StartCoroutine(BoostCards(1f, playerCard, opponentCard));
        //selectionStage = false;
        StartCoroutine(CheckWinner(0.5f, playerCard, opponentCard));
    }

    private IEnumerator FlipCards(float waitTime, GameObject playerCard, GameObject opponentCard)
    {
        yield return new WaitForSeconds(waitTime);

        playerCard.GetComponent<Animator>().SetTrigger("Flip");
        opponentCard.GetComponent<Animator>().SetTrigger("FlipU");
    }

    private IEnumerator BoostCards(float waitTime, GameObject playerCard, GameObject opponentCard)
    {
        yield return new WaitForSeconds(waitTime);

        CardInfo playerInfo = playerCard.GetComponent<CardInfo>();
        CardInfo oppInfo = opponentCard.GetComponent<CardInfo>();

        if (playerInfo.type == Elements.Element.FIRE && oppInfo.type == Elements.Element.EARTH ||
            playerInfo.type == Elements.Element.EARTH && oppInfo.type == Elements.Element.METAL ||
            playerInfo.type == Elements.Element.METAL && oppInfo.type == Elements.Element.WATER ||
            playerInfo.type == Elements.Element.WATER && oppInfo.type == Elements.Element.WOOD ||
            playerInfo.type == Elements.Element.WOOD && oppInfo.type == Elements.Element.FIRE)
        {
            oppInfo.value += playerInfo.value / 2;
            oppInfo.cardValue.text = oppInfo.value.ToString();
            oppInfo.cardValue.color = new Color(0, 0.5f, 0, 1);
            source.clip = Resources.Load<AudioClip>("Sounds/Boost");
            source.Play();
            wasBoosted = true;
        }
        else if (oppInfo.type == Elements.Element.FIRE && playerInfo.type == Elements.Element.EARTH ||
                 oppInfo.type == Elements.Element.EARTH && playerInfo.type == Elements.Element.METAL ||
                 oppInfo.type == Elements.Element.METAL && playerInfo.type == Elements.Element.WATER ||
                 oppInfo.type == Elements.Element.WATER && playerInfo.type == Elements.Element.WOOD ||
                 oppInfo.type == Elements.Element.WOOD && playerInfo.type == Elements.Element.FIRE)
        {
            playerInfo.value += oppInfo.value / 2;
            playerInfo.cardValue.text = playerInfo.value.ToString();
            playerInfo.cardValue.color = new Color(0, 0.5f, 0, 1);
            source.clip = Resources.Load<AudioClip>("Sounds/Boost");
            source.Play();
            wasBoosted = true;
        }
        else wasBoosted = false;
    }

    private IEnumerator CheckWinner(float waitTime, GameObject playerCard, GameObject opponentCard)
    {
        yield return new WaitForSeconds(waitTime);

        CardInfo playerInfo = playerCard.GetComponent<CardInfo>();
        CardInfo oppInfo = opponentCard.GetComponent<CardInfo>();

        if (playerInfo.type == Elements.Element.FIRE && oppInfo.type == Elements.Element.METAL ||
                        playerInfo.type == Elements.Element.EARTH && oppInfo.type == Elements.Element.WATER ||
                        playerInfo.type == Elements.Element.METAL && oppInfo.type == Elements.Element.WOOD ||
                        playerInfo.type == Elements.Element.WATER && oppInfo.type == Elements.Element.FIRE ||
                        playerInfo.type == Elements.Element.WOOD && oppInfo.type == Elements.Element.EARTH)
        {
            // Tell server player1 won
            //playerWon = true;
            roundWinnerId = NetworkManager.LocalClientId;
            winningType = playerInfo.type;
            losingType = oppInfo.type;
        }
        else if (oppInfo.type == Elements.Element.FIRE && playerInfo.type == Elements.Element.METAL ||
                oppInfo.type == Elements.Element.EARTH && playerInfo.type == Elements.Element.WATER ||
                oppInfo.type == Elements.Element.METAL && playerInfo.type == Elements.Element.WOOD ||
                oppInfo.type == Elements.Element.WATER && playerInfo.type == Elements.Element.FIRE ||
                oppInfo.type == Elements.Element.WOOD && playerInfo.type == Elements.Element.EARTH)
        {
            // Tell server player2 won
            //playerWon = false;
            ulong playerId = NetworkManager.LocalClientId;
            roundWinnerId = (ulong) (playerId == (ulong)0 ? 1 : 0);
            winningType = oppInfo.type;
            losingType = playerInfo.type;
        }
        else if (playerInfo.value > oppInfo.value)
        {
            // Tell server player1 won
            //playerWon = true;
            roundWinnerId = NetworkManager.LocalClientId;
            winningType = playerInfo.type;
            losingType = oppInfo.type;
        }
        else if (oppInfo.value > playerInfo.value)
        {
            // Tell server player2 won
            //playerWon = false;
            ulong playerId = NetworkManager.LocalClientId;
            roundWinnerId = (ulong)(playerId == (ulong)0 ? 1 : 0);
            winningType = oppInfo.type;
            losingType = playerInfo.type;
        }
        else
        {
            // Tell server about tie
            //wasTie = true;
            roundWinnerId = (ulong)3;
        }

        SendRoundResultsServerRpc(roundWinnerId);
    }

    private IEnumerator GivePoints(float waitTime, Elements.Element winningType, GameObject winningCard)
    {
        yield return new WaitForSeconds(waitTime);

        Elements.Element cardType = winningType;
        if (roundWinnerId == NetworkManager.LocalClientId)
        {
            PlayerInfo playerInfo = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerInfo>();
            switch (cardType)
            {
                case Elements.Element.EARTH:
                    GameObject.FindGameObjectWithTag("PEP" + playerInfo.playerWins[EARTH].Value.ToString())
                        .GetComponent<SpriteRenderer>().enabled = true;
                    break;
                case Elements.Element.FIRE:
                    GameObject.FindGameObjectWithTag("PFP" + playerInfo.playerWins[FIRE].Value.ToString())
                        .GetComponent<SpriteRenderer>().enabled = true;
                    break;
                case Elements.Element.METAL:
                    GameObject.FindGameObjectWithTag("PMP" + playerInfo.playerWins[METAL].Value.ToString())
                        .GetComponent<SpriteRenderer>().enabled = true;
                    break;
                case Elements.Element.WATER:
                    GameObject.FindGameObjectWithTag("PWaP" + playerInfo.playerWins[WATER].Value.ToString())
                        .GetComponent<SpriteRenderer>().enabled = true;
                    break;
                case Elements.Element.WOOD:
                    GameObject.FindGameObjectWithTag("PWoP" + playerInfo.playerWins[WOOD].Value.ToString())
                        .GetComponent<SpriteRenderer>().enabled = true;
                    break;
            }
        }
        else
        {
            GetOpponentWinsServerRpc((int)winningType, new ServerRpcParams());
        }

        source.clip = Resources.Load<AudioClip>("Sounds/CardPass3");
        source.Play();
        winningCard.GetComponent<Animator>().SetTrigger("Fade");
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetOpponentWinsServerRpc(int type, ServerRpcParams serverParams)
    {
        ulong opponentId = (ulong) ((serverParams.Receive.SenderClientId == (ulong)0) ? 1 : 0);
        int winsCount = NetworkManager.ConnectedClients[opponentId].PlayerObject.GetComponent<PlayerInfo>()
            .playerWins[type].Value;
        UpdateOpponentWinsClientRpc(winsCount, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new List<ulong>{serverParams.Receive.SenderClientId}
            }
        });
    }

    [ClientRpc]
    private void UpdateOpponentWinsClientRpc(int count, ClientRpcParams clientParams)
    {
        Elements.Element cardType = winningType;
        switch (cardType)
        {
            case Elements.Element.EARTH:
                GameObject.FindGameObjectWithTag("OEP" + count).GetComponent<SpriteRenderer>().enabled = true;
                break;
            case Elements.Element.FIRE:
                GameObject.FindGameObjectWithTag("OFP" + count).GetComponent<SpriteRenderer>().enabled = true;
                break;
            case Elements.Element.METAL:
                GameObject.FindGameObjectWithTag("OMP" + count).GetComponent<SpriteRenderer>().enabled = true;
                break;
            case Elements.Element.WATER:
                GameObject.FindGameObjectWithTag("OWaP" + count).GetComponent<SpriteRenderer>().enabled = true;
                break;
            case Elements.Element.WOOD:
                GameObject.FindGameObjectWithTag("OWoP" + count).GetComponent<SpriteRenderer>().enabled = true;
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendRoundResultsServerRpc(ulong result)
    {
        roundResults.Add(result);
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncrementWinsServerRpc(int type, ServerRpcParams serverParams)
    {
        PlayerInfo playerInfo = NetworkManager.ConnectedClients[serverParams.Receive.SenderClientId].PlayerObject
            .GetComponent<PlayerInfo>();
        playerInfo.playerWins[type].Value++;
        NetworkLog.LogInfoServer("Current wins: " + playerInfo.playerWins[type].Value);
    }

    [ClientRpc]
    private void DisplayResultsClientRpc(ulong winnerId, ulong winnerCardId, ulong loserCardId)
    {
        GameObject losingCard = PlayerDeck.GetCardById(loserCardId);
        GameObject winningCard = PlayerDeck.GetCardById(winnerCardId);

        if (winnerId != 3)
        {
            Elements.Element cardType = winningType;
            StartCoroutine(MoveToCenter(losingCard));
            switch (cardType)
            {
                case Elements.Element.EARTH:
                    losingCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/EDeath");
                    losingCard.GetComponent<Animator>().SetTrigger("EDeath");
                    if (winnerId == NetworkManager.LocalClientId) IncrementWinsServerRpc(EARTH, new ServerRpcParams());
                    //++playerEarthWins;
                    break;
                case Elements.Element.FIRE:
                    losingCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/FDeath");
                    losingCard.GetComponent<Animator>().SetTrigger("FDeath");
                    if (winnerId == NetworkManager.LocalClientId) IncrementWinsServerRpc(FIRE, new ServerRpcParams());
                    //++playerFireWins;
                    break;
                case Elements.Element.METAL:
                    losingCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/MDeath");
                    losingCard.GetComponent<Animator>().SetTrigger("MDeath");
                    if (winnerId == NetworkManager.LocalClientId) IncrementWinsServerRpc(METAL, new ServerRpcParams());
                    //++playerMetalWins;
                    break;
                case Elements.Element.WATER:
                    losingCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/WaDeath");
                    losingCard.GetComponent<Animator>().SetTrigger("WaDeath");
                    if (winnerId == NetworkManager.LocalClientId) IncrementWinsServerRpc(WATER, new ServerRpcParams());
                    //++playerWaterWins;
                    break;
                case Elements.Element.WOOD:
                    losingCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/WoDeath");
                    losingCard.GetComponent<Animator>().SetTrigger("WoDeath");
                    if (winnerId == NetworkManager.LocalClientId) IncrementWinsServerRpc(WOOD, new ServerRpcParams());
                    //++playerWoodWins;
                    break;
            }
        }
        else
        {
            winningCard.GetComponent<Animator>().SetTrigger("Fade");
            losingCard.GetComponent<Animator>().SetTrigger("Fade");
            // TODO: Restart to next round
        }

        StartCoroutine(GivePoints(2f, winningCard.GetComponent<CardInfo>().type, winningCard));
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

    IEnumerator MoveToCenter(GameObject card)
    {
        float lerpTime = 0;
        card.transform.Find("DeathCard").GetComponent<SpriteRenderer>().sortingOrder = 6;
        card.GetComponent<SpriteRenderer>().sortingOrder = 7;
        card.transform.Find("Number").GetComponent<TMPro.TextMeshPro>().sortingOrder = 8;
        card.transform.Find("Type").GetComponent<TMPro.TextMeshPro>().sortingOrder = 8;
        card.transform.Find("Symbol").GetComponent<SpriteRenderer>().sortingOrder = 8;
        Vector2 startPos = card.transform.position;
        Vector2 destPos = new Vector2(-0.3f, 0);
        Vector2 startSize = card.transform.localScale;
        Vector2 destSize = new Vector2(0.25f, 0.25f);
        Quaternion startRot = card.transform.rotation;
        Quaternion destRot = Quaternion.identity;
        while (lerpTime < 1.1f)
        {
            card.transform.position = Vector2.Lerp(startPos, destPos, lerpTime);
            card.transform.localScale = Vector2.Lerp(startSize, destSize, lerpTime);
            card.transform.rotation = Quaternion.Lerp(startRot, destRot, lerpTime);
            lerpTime += Time.deltaTime * 3;

            yield return null;
        }

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
