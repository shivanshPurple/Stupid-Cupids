using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class RoomPlayerManager : NetworkRoomPlayer
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(hookName))]
    public string playerName;
    [SyncVar(hook = nameof(hookChar))]
    public int charIndex = 1;
    [SyncVar]
    public bool isHost = false;
    [SerializeField]
    private Text text;
    [SerializeField]
    private SpriteRenderer charSprite;
    [SerializeField]
    private SpriteRenderer readySprite;

    public override void OnStartClient()
    {
        base.OnStartClient();
        gameObject.name = "RoomPlayer" + index;
        GameObject[] roomPlayers = GameObject.FindGameObjectsWithTag("RoomPlayer");
        for (int i = 0; i < roomPlayers.Length; i++)
            roomPlayers[i].transform.position = new Vector3(3.5f, 2 - 1.5f * i);
        if (isLocalPlayer)
            CmdRegisterRoomHost();
    }

    [Command]
    private void CmdRegisterRoomHost()
    {
        if (NetworkManager.singleton.numPlayers == 1)
            isHost = true;
    }

    [Command]
    public void CmdSetChar(int currentCharUi)
    {
        charIndex = currentCharUi;
    }

    private void hookChar(int oldChar, int newChar)
    {
        charSprite.sprite = Resources.Load<Sprite>("Characters/Asset " + newChar);
    }

    [Command]
    internal void CmdSetName(string text)
    {
        playerName = text;
    }
    private void hookName(string oldName, string newName)
    {
        text.text = newName;
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        readySprite.enabled = newReadyState;
    }
}