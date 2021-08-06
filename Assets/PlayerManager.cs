using UnityEngine.UI;
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System;

public class PlayerManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetName))]
    public string playerName;
    [SyncVar(hook = nameof(SetChar))]
    public int charIndex;
    public GameObject cardStackPrefab;
    internal CardStackHandler inHandCardStack;
    private Button endTurnBtn;
    internal PotManager potManager;
    [SyncVar]
    public bool isDating;
    [SyncVar]
    public bool isHost;

    private void SetChar(int oldIndex, int newIndex)
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Characters/Asset " + newIndex);
    }
    public void SetName(string oldPlayerName, string newPlayerName)
    {
        gameObject.GetComponentInChildren<Text>().text = newPlayerName;
        gameObject.name = newPlayerName + netId;
    }
    void Start()
    {
        potManager = GetComponent<PotManager>();
        endTurnBtn = GameObject.FindGameObjectWithTag("EndTurnButton").GetComponent<Button>();
        if (isLocalPlayer)
            GameManager.singleton.CmdNotifyServer(netId);
        if (isServer && isHost)
            makeHost(connectionToClient);
    }
    [TargetRpc]
    internal void makeHost(NetworkConnection connection)
    {
        GameManager.singleton.startRoundBtn.gameObject.SetActive(true);
    }
    [TargetRpc]
    internal void newHand(NetworkConnection connection, List<string> newList, bool isPerk)
    {
        if (inHandCardStack == null && isLocalPlayer)
        {
            inHandCardStack = (Instantiate(cardStackPrefab)).GetComponent<CardStackHandler>();
            inHandCardStack.createNewCards(newList, isPerk);
        }
    }
    internal void changeTurn(bool isTurn)
    {
        transform.GetChild(2).gameObject.SetActive(isTurn);
    }

    internal void changeLocalTurn(bool isTurn)
    {
        if (!isDating)
        {
            if (!isTurn)
            {
                potManager.isZoomed = false;
                Destroy(inHandCardStack?.gameObject);
            }
            else
                inHandCardStack?.showCards(isTurn);
            endTurnBtn.interactable = isTurn;
        }
    }

    [TargetRpc]
    internal void allCardsPlayed(NetworkConnection target)
    {
        inHandCardStack.showCards(false);
    }
    [TargetRpc]
    internal void enableEndTurnButton(NetworkConnection target, bool interactable)
    {
        endTurnBtn.interactable = interactable;
    }

    [TargetRpc]
    internal void OnRoundEnded(NetworkConnection connectionToClient, string nextGameRound)
    {
        GameManager gm = GameManager.singleton;
        gm.startRoundBtn.interactable = true;
        gm.startRoundBtn.GetComponentInChildren<Text>().text = "Start " + nextGameRound;
    }
}
