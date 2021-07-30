using UnityEngine;
using Mirror;

public class RoomPlayerManager : NetworkRoomPlayer
{
    [Header("Player Data")]
    [SyncVar]
    public string playerName;
    [SyncVar]
    public int charIndex = 1;
    public override void OnStartClient()
    {
        base.OnStartClient();
        gameObject.name = "RoomPlayer" + index;
    }

    [Command]
    public void CmdSetChar(int currentCharUi)
    {
        charIndex = currentCharUi;
    }

    [Command]
    internal void CmdSetName(string text)
    {
        playerName = text;
    }
}
