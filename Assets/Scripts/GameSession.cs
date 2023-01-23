using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameSession : NetworkBehaviour
{
    //Class variables
    PlayerDeck playerDeck;
    PlayerDeck opponentDeck;
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

    
    private void Start()
    {
        playerDeck = GameObject.FindGameObjectWithTag("PlayerDraw").GetComponent<PlayerDeck>();
        opponentDeck = GameObject.FindGameObjectWithTag("OpponentDraw").GetComponent<PlayerDeck>();
        playerHand = new GameObject[5];
        opponentHand = new GameObject[5];
        source = GetComponent<AudioSource>();
        
        //1) Shuffle Deck
        ShuffleDeck(ref playerDeck);
        ShuffleDeck(ref opponentDeck);

        //2) Deal cards
        Debug.Log("Reached dealing");
        StartDealingServerRpc();
        //StartCoroutine(StartDealing());
    }
    private void Update()
    {
        //3) Initiate start
        if(!readyToStart && cardsDealt == 10)
        {
            readyToStart = true;
        }
        else if (readyToStart && !hasStarted)
        {
            //ADD START ANIMATION HERE

            foreach(GameObject card in playerHand)
            {
                card.GetComponent<BoxCollider2D>().enabled = true;
            }

            hasStarted = true;
            selectionStage = true;
        }


        if (hasStarted)
        {
            //4) Card select
            if (selectedCard != null && selectionStage)
            {
                //Player
                foreach (GameObject card in playerHand)
                {
                    card.GetComponent<BoxCollider2D>().enabled = false;
                }
                Vector2 cardPos = selectedCard.transform.position;
                Vector2 destPos = GameObject.FindGameObjectWithTag("PlayerPlay").transform.position;
                selectedCard.GetComponent<Animator>().SetTrigger("Unflip");
                StartCoroutine(MoveCard(selectedCard, cardPos, destPos, new Vector2(0.25f, 0.25f), new Vector2(0.15f, 0.15f)));

                //Opponent
                int cardSelected = Random.Range(0, 5);
                opponentCard = opponentHand[cardSelected];
                Vector2 oppCardPos = opponentCard.transform.position;
                Vector2 oppDestPos = GameObject.FindGameObjectWithTag("OpponentPlay").transform.position;
                StartCoroutine(MoveCard(opponentCard, oppCardPos, oppDestPos, new Vector2(0.25f, 0.25f), new Vector2(0.15f, 0.15f)));

                cardsDealt -= 2;
                StartCoroutine(WaitAndFlip());
                selectionStage = false;
            }

            if (revealStage)
            {
                selectionStage = false;

                //5) Reveal cards
                selectedCard.GetComponent<Animator>().SetTrigger("Flip");
                opponentCard.GetComponent<Animator>().SetTrigger("FlipU");

                StartCoroutine(WaitAndBoost());
                revealStage = false;
            }

            if (boostStage)
            {
                CardInfo playerInfo = selectedCard.GetComponent<CardInfo>();
                CardInfo oppInfo = opponentCard.GetComponent<CardInfo>();
                if (!checkBoost)
                {
                    //6) Add boosts
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

                    if (wasBoosted)
                    {
                        StartCoroutine(WaitAndRevealWinner());
                    }
                    else
                    {
                        winnerStage = true;
                    }

                    checkBoost = true;
                }

                if (winnerStage)
                {
                    boostStage = false;

                    //7) Determine winner
                    if (playerInfo.type == Elements.Element.FIRE  && oppInfo.type == Elements.Element.METAL ||
                        playerInfo.type == Elements.Element.EARTH && oppInfo.type == Elements.Element.WATER ||
                        playerInfo.type == Elements.Element.METAL && oppInfo.type == Elements.Element.WOOD  ||
                        playerInfo.type == Elements.Element.WATER && oppInfo.type == Elements.Element.FIRE  ||
                        playerInfo.type == Elements.Element.WOOD  && oppInfo.type == Elements.Element.EARTH)
                    {
                        playerWon = true;
                        winningType = playerInfo.type;
                    }
                    else if (oppInfo.type == Elements.Element.FIRE && playerInfo.type == Elements.Element.METAL ||
                            oppInfo.type == Elements.Element.EARTH && playerInfo.type == Elements.Element.WATER ||
                            oppInfo.type == Elements.Element.METAL && playerInfo.type == Elements.Element.WOOD  ||
                            oppInfo.type == Elements.Element.WATER && playerInfo.type == Elements.Element.FIRE  ||
                            oppInfo.type == Elements.Element.WOOD  && playerInfo.type == Elements.Element.EARTH)
                    {
                        playerWon = false;
                        winningType = oppInfo.type;
                    }
                    else if (playerInfo.value > oppInfo.value)
                    {
                        playerWon = true;
                        winningType = playerInfo.type;
                    }
                    else if (oppInfo.value > playerInfo.value)
                    {
                        playerWon = false;
                        winningType = oppInfo.type;
                    }
                    else
                    {
                        wasTie = true;
                    }
                    winnerStage = false;
                    StartCoroutine(WaitAndClear());
                }
            }

            //8) Clear board
            if (clearingStage)
            {
                if (playerWon)
                {
                    Elements.Element cardType = selectedCard.GetComponent<CardInfo>().type;
                    StartCoroutine(MoveToCenter(opponentCard));
                    switch (cardType)
                    {
                        case Elements.Element.EARTH:
                            opponentCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/EDeath");
                            opponentCard.GetComponent<Animator>().SetTrigger("EDeath");
                            ++playerEarthWins;
                            break;
                        case Elements.Element.FIRE:
                            opponentCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/FDeath");
                            opponentCard.GetComponent<Animator>().SetTrigger("FDeath");
                            ++playerFireWins;
                            break;
                        case Elements.Element.METAL:
                            opponentCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/MDeath");
                            opponentCard.GetComponent<Animator>().SetTrigger("MDeath");
                            ++playerMetalWins;
                            break;
                        case Elements.Element.WATER:
                            opponentCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/WaDeath");
                            opponentCard.GetComponent<Animator>().SetTrigger("WaDeath");
                            ++playerWaterWins;
                            break;
                        case Elements.Element.WOOD:
                            opponentCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/WoDeath");
                            opponentCard.GetComponent<Animator>().SetTrigger("WoDeath");
                            ++playerWoodWins;
                            break;
                    }
                }
                else if(!playerWon && !wasTie)
                {
                    Elements.Element cardType = opponentCard.GetComponent<CardInfo>().type;
                    StartCoroutine(MoveToCenter(selectedCard));
                    switch (cardType)
                    {
                        case Elements.Element.EARTH:
                            selectedCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/EDeath");
                            selectedCard.GetComponent<Animator>().SetTrigger("EDeath");
                            ++oppEarthWins;
                            break;
                        case Elements.Element.FIRE:
                            selectedCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/FDeath");
                            selectedCard.GetComponent<Animator>().SetTrigger("FDeath");
                            ++oppFireWins;
                            break;
                        case Elements.Element.METAL:
                            selectedCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/MDeath");
                            selectedCard.GetComponent<Animator>().SetTrigger("MDeath");
                            ++oppMetalWins;
                            break;
                        case Elements.Element.WATER:
                            selectedCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/WaDeath");
                            selectedCard.GetComponent<Animator>().SetTrigger("WaDeath");
                            ++oppWaterWins;
                            break;
                        case Elements.Element.WOOD:
                            selectedCard.GetComponent<CardInfo>().death = Resources.Load<AudioClip>("Sounds/WoDeath");
                            selectedCard.GetComponent<Animator>().SetTrigger("WoDeath");
                            ++oppWoodWins;
                            break;
                    }
                }

                if (!wasTie)
                {
                    StartCoroutine(WaitAndGivePoints());
                }
                else
                {
                    selectedCard.GetComponent<Animator>().SetTrigger("Fade");
                    opponentCard.GetComponent<Animator>().SetTrigger("Fade");
                    overallWinner = false;
                    restartStage = true;
                }
                
                clearingStage = false;
            }

            //9) Award play points
            if (pointsStage)
            {
                if (playerWon)
                {
                    Elements.Element cardType = selectedCard.GetComponent<CardInfo>().type;
                    switch (cardType)
                    {
                        case Elements.Element.EARTH:
                            GameObject.FindGameObjectWithTag("PEP" + playerEarthWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.FIRE:
                            GameObject.FindGameObjectWithTag("PFP" + playerFireWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.METAL:
                            GameObject.FindGameObjectWithTag("PMP" + playerMetalWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.WATER:
                            GameObject.FindGameObjectWithTag("PWaP" + playerWaterWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.WOOD:
                            GameObject.FindGameObjectWithTag("PWoP" + playerWoodWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                    }
                    source.clip = Resources.Load<AudioClip>("Sounds/CardPass3");
                    source.Play();
                    selectedCard.GetComponent<Animator>().SetTrigger("Fade");
                }
                else
                {
                    Elements.Element cardType = opponentCard.GetComponent<CardInfo>().type;
                    switch (cardType)
                    {
                        case Elements.Element.EARTH:
                            GameObject.FindGameObjectWithTag("OEP" + oppEarthWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.FIRE:
                            GameObject.FindGameObjectWithTag("OFP" + oppFireWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.METAL:
                            GameObject.FindGameObjectWithTag("OMP" + oppMetalWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.WATER:
                            GameObject.FindGameObjectWithTag("OWaP" + oppWaterWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                        case Elements.Element.WOOD:
                            GameObject.FindGameObjectWithTag("OWoP" + oppWoodWins.ToString()).GetComponent<SpriteRenderer>().enabled = true;
                            break;
                    }
                    source.clip = Resources.Load<AudioClip>("Sounds/CardPass3");
                    source.Play();
                    opponentCard.GetComponent<Animator>().SetTrigger("Fade");
                }
                StartCoroutine(WaitAndCheck());
                pointsStage = false;
            }

            //10) Check for round winner
            if (wincheckStage)
            {
                selectedCard.transform.position = GameObject.FindGameObjectWithTag("PlayerDraw").transform.position;
                opponentCard.transform.position = GameObject.FindGameObjectWithTag("OpponentDraw").transform.position;

                if ((playerEarthWins >= 1 && playerFireWins >= 1 && playerMetalWins >= 1 && playerWaterWins >= 1 && playerWoodWins >= 1) ||
                playerEarthWins == 3 || playerFireWins == 3 || playerMetalWins == 3 || playerWaterWins == 3 || playerWoodWins == 3)
                {
                    print("Player Wins!");
                    overallWinner = true;
                }
                else if ((oppEarthWins >= 1 && oppFireWins >= 1 && oppMetalWins >= 1 && oppWaterWins >= 1 && oppWoodWins >= 1) ||
                          oppEarthWins == 3 || oppFireWins == 3 || oppMetalWins == 3 || oppWaterWins == 3 || oppWoodWins == 3)
                {
                    print("Opponent Wins!");
                    overallWinner = true;
                }
                StartCoroutine(WaitAndRestart());
                wincheckStage = false;
            }

            if (!overallWinner && restartStage)
            {
                //12) Draw cards
                source.clip = Resources.Load<AudioClip>("Sounds/CardPass1");
                int playerReplacePos = -1;
                for (int i = 0; i < 5; i++)
                {
                    if (playerHand[i] == selectedCard)
                    {
                        playerReplacePos = i;
                    }
                }
                StartCoroutine(DealPlayer(1, playerReplacePos));
                int oppReplacePos = -1;
                for (int i = 0; i < 5; i++)
                {
                    if (opponentHand[i] == opponentCard)
                    {
                        oppReplacePos = i;
                    }
                }
                StartCoroutine(DealOpponent(1, oppReplacePos));

                //13) Restart
                Restart();
            }
            else if(overallWinner && restartStage)
            {
                //14) End game
                restartStage = false;
                AudioClip finish = Resources.Load<AudioClip>("Sounds/WinGong");
                GameObject.FindGameObjectWithTag("Music").GetComponent<AudioSource>().Stop();
                source.clip = finish;
                source.Play();
                GameObject endScreen = GameObject.FindGameObjectWithTag("End");
                if (playerWon)
                {
                    endScreen.transform.Find("Win-Loss").GetComponent<TMPro.TextMeshPro>().text = "You Win!";
                }
                else
                {
                    endScreen.transform.Find("Win-Loss").GetComponent<TMPro.TextMeshPro>().text = "You Lost...";
                }
                Vector3 screenPos = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, endScreen.transform.position.z);
                endScreen.transform.position = screenPos;
            }
        }
    }

    private void Restart()
    {
        readyToStart = false;
        hasStarted = false;
        selectionStage = false;
        revealStage = false;
        boostStage = false;
        checkBoost = false;
        winnerStage = false;
        clearingStage = false;
        pointsStage = false;
        wincheckStage = false;
        restartStage = false;
        playerWon = false;
        wasTie = false;
        wasBoosted = false;
        overallWinner = false;
        selectedCard = null;
        opponentCard = null;
    }

    [ServerRpc]
    private void StartDealingServerRpc()
    {
        Debug.Log("Dealing!");
        StartCoroutine(StartDealing());
    }

    IEnumerator StartDealing()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count == 2);
        //yield return new WaitForSeconds(2);
        Debug.Log("2 PLayers reached");

        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(DealPlayer(i + 1, i));
            StartCoroutine(DealOpponent(i + 1, i));
        }
    }

    private void ShuffleDeck(ref PlayerDeck deck)
    {
        for (int i = 0; i < 600; i++)
        {
            int pos1 = Random.Range(0, 50);
            int pos2 = Random.Range(0, 50);
            GameObject temp = deck.deck[pos1];
            deck.deck[pos1] = deck.deck[pos2];
            deck.deck[pos2] = temp;
        }
    }

    IEnumerator DealPlayer(int waitTime, int handPos)
    {
        yield return new WaitForSeconds(waitTime * 0.2f);
        playerHand[handPos] = playerDeck.deck[drawCount];
        playerDeck.deck[drawCount].transform.position = GameObject.FindGameObjectWithTag("PlayerDraw").transform.position;
        Vector2 cardPos = playerDeck.deck[drawCount].transform.position;
        Vector2 destPos = new Vector2(-3.6f + (handPos * 1.8f), -4.4f);
        playerDeck.deck[drawCount].GetComponent<Animator>().SetTrigger("Flip");
        StartCoroutine(MoveCard(playerDeck.deck[drawCount], cardPos, destPos, new Vector2(0.15f, 0.15f), new Vector2(0.25f, 0.25f)));
        ++cardsDealt;
    }
    IEnumerator DealOpponent(int waitTime, int handPos)
    {
        yield return new WaitForSeconds(waitTime * 0.2f);
        opponentHand[handPos] = opponentDeck.deck[drawCount];
        opponentDeck.deck[drawCount].transform.position = GameObject.FindGameObjectWithTag("OpponentDraw").transform.position;
        Vector2 cardPos = opponentDeck.deck[drawCount].transform.position;
        Vector2 destPos = new Vector2(-3.6f + (handPos * 1.8f), 4.5f);
        StartCoroutine(MoveCard(opponentDeck.deck[drawCount], cardPos, destPos, new Vector2(0.15f, 0.15f), new Vector2(0.25f, 0.25f)));
        ++cardsDealt;
        if (drawCount == 49)
        {
            drawCount = 0;
            ShuffleDeck(ref playerDeck);
            ShuffleDeck(ref opponentDeck);
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
        while(lerpTime < 1.1f)
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

        revealStage = true;
    }

    IEnumerator WaitAndBoost()
    {
        yield return new WaitForSeconds(1);

        boostStage = true;
    }

    IEnumerator WaitAndRevealWinner()
    {
        yield return new WaitForSeconds(0.5f);

        winnerStage = true;
    }

    IEnumerator WaitAndClear()
    {
        yield return new WaitForSeconds(0.5f);

        clearingStage = true;
    }

    IEnumerator WaitAndGivePoints()
    {
        yield return new WaitForSeconds(2);

        pointsStage = true;
    }

    IEnumerator WaitAndCheck()
    {
        yield return new WaitForSeconds(1);

        wincheckStage = true;
    }

    IEnumerator WaitAndRestart()
    {
        yield return new WaitForSeconds(0.5f);

        restartStage = true;
    }
}
