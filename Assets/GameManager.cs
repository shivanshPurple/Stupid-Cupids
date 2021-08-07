using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public SyncList<uint> players = new SyncList<uint>();
    public List<PlayerManager> playerManagers = new List<PlayerManager>();
    public static GameManager singleton;
    public Button startRoundBtn;
    internal GameRound round;
    internal int cardsPlayedByCurrentPlayer = 0;
    [SyncVar(hook = nameof(hookGameEvent))]
    private string currRoundName;
    [SyncVar(hook = nameof(changeTurn))]
    public uint turnId = 99;
    [SyncVar(hook = nameof(changeDater))]
    public uint datingId = 99;

    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else if (singleton != this)
            Destroy(gameObject);
    }
    void Start()
    {
        players.Callback += playersListCallback;
        GameObject[] roomPlayers = GameObject.FindGameObjectsWithTag("RoomPlayer");
        foreach (GameObject rp in roomPlayers)
            Destroy(rp);
    }
    void playersListCallback(SyncList<uint>.Operation op, int index, uint oldItem, uint newItem)
    {
        List<GameObject> temp = new List<GameObject>();
        playerManagers.Clear();
        foreach (uint p in players)
        {
            playerManagers.Add(NetworkIdentity.spawned[p].GetComponent<PlayerManager>());
            temp.Add(NetworkIdentity.spawned[p].gameObject);
        }
        utils.arrangeObjectsOnWidth(temp, -6.5f);
    }
    [Command(requiresAuthority = false)]
    public void CmdNotifyServer(uint id)
    {
        players.Add(id);
    }
    private void hookGameEvent(string oldEvent, string newEvent)
    {
        StartCoroutine(OnNewRound(newEvent));
    }
    IEnumerator OnNewRound(string newEvent)
    {
        BoardManager.singleton.showBoard(newEvent);
        turnId = 99;
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => BoardManager.singleton.mode == BoardManager.playMode.blank);
        if (NetworkClient.localPlayer.gameObject.GetComponent<PlayerManager>().isHost)
            CmdEndTurn();
    }

    [Command(requiresAuthority = false)]
    public void CmdStartRound()
    {
        round = GameRound.GetNext(round);
        currRoundName = round.name;
        if (round != GameRound.Dating)
        {
            // sets the next dater according to the previous one
            if (round == GameRound.Normal)
            {
                int nextIndex = getNextIndex(datingId);
                if (nextIndex == 99)
                    nextIndex = getNextIndex(99);
                datingId = players[nextIndex];
            }
            // distributes cards to the cupids
            List<int> set = new List<int>();
            set = utils.getSetOfRandomNumbers(round.availableCards.Length, round.inHandLimit * players.Count);
            for (int i = 0; i < playerManagers.Count; i++)
            {
                if (players[i] != datingId)
                {
                    List<string> tempStrings = new List<string>();
                    List<int> thisPlayerCardIndexes = set.GetRange(i * round.inHandLimit, round.inHandLimit);
                    foreach (int index in thisPlayerCardIndexes)
                        tempStrings.Add(round.availableCards[index]);
                    playerManagers[i].newHand(playerManagers[i].connectionToClient, tempStrings, round == GameRound.Normal);
                }
            }
        }
    }

    private void changeDater(uint oldDater, uint newDater)
    {
        playerManagers[getPlayerIndex(datingId)].isDating = true;
        if (players.Contains(oldDater))
            playerManagers[getPlayerIndex(oldDater)].isDating = false;
        utils.arrangeObjectsOnWidth(new List<GameObject> { playerManagers[getPlayerIndex(newDater)].gameObject }, 6.5f);
        List<GameObject> temp = new List<GameObject>();
        foreach (PlayerManager p in playerManagers)
            if (!p.isDating)
                temp.Add(p.gameObject);
        utils.arrangeObjectsOnWidth(temp, -6.5f);
    }

    [Command(requiresAuthority = false)]
    public void CmdEndTurn()
    {
        if (round != GameRound.Dating)
        {
            cardsPlayedByCurrentPlayer = 0;
            int nextTurnIndex = getNextIndex(turnId);
            if (nextTurnIndex == 99)
            {
                OnRoundEnded();
                return;
            }
            if (players[nextTurnIndex] == datingId)
                nextTurnIndex = getNextIndex(players[nextTurnIndex]);
            if (nextTurnIndex == 99)
            {
                OnRoundEnded();
                return;
            }
            turnId = players[nextTurnIndex];
        }
        else
        {
            if (cardsPlayedByCurrentPlayer == 99)
            {
                OnRoundEnded();
                cardsPlayedByCurrentPlayer = 0;
            }
            else
            {
                turnId = datingId;
                playerManagers[getPlayerIndex(turnId)].enableEndTurnButton(playerManagers[getPlayerIndex(turnId)].connectionToClient, false);
                foreach (PlayerManager p in playerManagers)
                    p.potManager.showDateButton(playerManagers[getPlayerIndex(datingId)].netIdentity.connectionToClient, true);
            }
        }
    }

    public void dateIsChosen()
    {
        foreach (PlayerManager p in playerManagers)
            p.potManager.showDateButton(playerManagers[getPlayerIndex(datingId)].netIdentity.connectionToClient, false);
        playerManagers[getPlayerIndex(turnId)].enableEndTurnButton(playerManagers[getPlayerIndex(turnId)].connectionToClient, true);
        cardsPlayedByCurrentPlayer = 99;
    }

    private void OnRoundEnded()
    {
        turnId = 99;
        foreach (PlayerManager p in playerManagers)
        {
            if (round == GameRound.Dating)
                p.potManager.resetPot();
            if (p.isHost)
                p.OnRoundEnded(p.connectionToClient, GameRound.GetNext(round).name);
        }
    }

    private void changeTurn(uint oldTurnId, uint newTurnId)
    {
        foreach (PlayerManager player in playerManagers)
        {
            if (newTurnId == player.netId)
            {
                player.changeTurn(true);
                if (newTurnId == NetworkClient.localPlayer.netId)
                    player.changeLocalTurn(true);
            }
            else if (oldTurnId == player.netId)
            {
                player.changeTurn(false);
                if (oldTurnId == NetworkClient.localPlayer.netId)
                    player.changeLocalTurn(false);
            }
        }
    }

    [Command(requiresAuthority = false)]
    internal void putCardOnTable(string str)
    {
        PlayerManager currentPlayer = playerManagers[getPlayerIndex(turnId)];
        int nextPlayerIndex = getNextNonDaterIndex(getPlayerIndex(turnId));
        PlayerManager nextPlayer = playerManagers[nextPlayerIndex];

        PotManager potThatWillGetCard = null;
        if (round == GameRound.Normal)
            potThatWillGetCard = currentPlayer.potManager;
        else if (round == GameRound.Sabotage)
            potThatWillGetCard = nextPlayer.potManager;

        potThatWillGetCard.addNewCard(
            str,
            round == GameRound.Normal,
            ++cardsPlayedByCurrentPlayer == round.playedCardsLimit);

        if (cardsPlayedByCurrentPlayer == round.playedCardsLimit)
            currentPlayer.allCardsPlayed(currentPlayer.connectionToClient);
    }
    private int getNextNonDaterIndex(int index)
    {
        ++index;
        if (index >= players.Count)
            return getNextNonDaterIndex(-1);
        if (players[index] == datingId)
            return getNextNonDaterIndex(index);
        else
            return index;
    }
    private int getPlayerIndex(uint id)
    {
        int index = players.IndexOf(id);
        if (index == -1)
            return 99;
        return index;
    }
    private int getNextIndex(uint id)
    {
        if (id == 99)
            return 0;
        int i = players.IndexOf(id);
        if (++i >= players.Count)
            return 99;
        return i;
    }
}

public class GameRound
{
    public string name;
    public int inHandLimit;
    public int playedCardsLimit;
    public string[] availableCards;
    public GameRound nextRound;
    private GameRound(string roundName, int inHandLimit, int playableLimit, string resourceAssetName = "na")
    {
        this.name = roundName;
        this.inHandLimit = inHandLimit;
        this.playedCardsLimit = playableLimit;
        if (resourceAssetName != "na")
            this.availableCards = Resources.Load<TextAsset>(resourceAssetName).text.Split('\n');
    }
    public static GameRound Normal = new GameRound("Normal Round", 5, 2, "perks"),
    Sabotage = new GameRound("Sabotage Round", 3, 1, "reds"),
    Dating = new GameRound("Dating Round", -1, -1);
    public static GameRound GetRound(string name)
    {
        if (name.Contains("Normal"))
            return Normal;
        else if (name.Contains("Sabotage"))
            return Sabotage;
        else
            return Dating;
    }
    public static GameRound GetNext(GameRound round)
    {
        if (round == Normal)
            return Sabotage;
        if (round == Sabotage)
            return Dating;
        else
            return Normal;
    }
}