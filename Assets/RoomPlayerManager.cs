using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RoomPlayerManager : NetworkRoomPlayer
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(hookName))]
    public string playerName;
    [SyncVar(hook = nameof(hookChar))]
    public int charIndex = 1;
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
        readySprite.enabled = false;
        transform.position = new Vector3(3.5f, 2 - 0.5f * (GameObject.FindGameObjectsWithTag("RoomPlayer").Length - 1));
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
        readySprite.enabled = false;
    }
}