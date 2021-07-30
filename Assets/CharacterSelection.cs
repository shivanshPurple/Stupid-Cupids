using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CharacterSelection : MonoBehaviour
{
    public Image imgElement;
    [Header("On Ready vars")]
    public Button leftButton;
    public Button rightButton;
    public InputField nameInput;
    private RoomPlayerManager localPlayer;
    public GameObject otherPlayerPrefab;
    public Transform parentUiTransform;
    private GameObject[] otherPlayers;
    private int currentCharUi = 1;
    void Start()
    {
        imgElement.sprite = Resources.Load<Sprite>("Characters/Asset " + currentCharUi);
        imgElement.preserveAspect = true;
        nameInput.onValueChanged.AddListener(onChangePlayerName);
    }
    void LateUpdate()
    {
        otherPlayers = GameObject.FindGameObjectsWithTag("RoomPlayer");
        if (otherPlayers.Length > parentUiTransform.childCount - 1)
        {
            GameObject newPlayer = GameObject.Instantiate(otherPlayerPrefab);
            newPlayer.transform.SetParent(parentUiTransform);
            return;
        }
        for (int i = 0; i < otherPlayers.Length; i++)
        {
            Transform otherPlayerUI = parentUiTransform.GetChild(i + 1);
            otherPlayerUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -150 * i - 50);
            Image otherPlayerImage = otherPlayerUI.GetChild(0).GetComponent<Image>();
            otherPlayerImage.sprite = Resources.Load<Sprite>("Characters/Asset " + otherPlayers[i].GetComponent<RoomPlayerManager>().charIndex);
            otherPlayerImage.preserveAspect = true;
            otherPlayerUI.GetChild(1).GetComponent<Text>().text = otherPlayers[i].GetComponent<RoomPlayerManager>().playerName;
            if (otherPlayers[i].GetComponent<RoomPlayerManager>().readyToBegin)
                otherPlayerUI.GetChild(2).GetComponent<Image>().color = Color.white;
            else
                otherPlayerUI.GetChild(2).GetComponent<Image>().color = Color.clear;
        }
    }
    public void ClickRight()
    {
        currentCharUi += 1;
        currentCharUi = 12 == currentCharUi ? 1 : currentCharUi;
        imgElement.sprite = Resources.Load<Sprite>("Characters/Asset " + currentCharUi);
        NetworkClient.localPlayer.gameObject.GetComponent<RoomPlayerManager>().CmdSetChar(currentCharUi);
    }
    public void ClickLeft()
    {
        currentCharUi -= 1;
        currentCharUi = 0 == currentCharUi ? 11 : currentCharUi;
        imgElement.sprite = Resources.Load<Sprite>("Characters/Asset " + currentCharUi);
        NetworkClient.localPlayer.gameObject.GetComponent<RoomPlayerManager>().CmdSetChar(currentCharUi);
    }
    public void OnReady()
    {
        if (nameInput.text != "")
        {
            localPlayer = NetworkClient.localPlayer.gameObject.GetComponent<RoomPlayerManager>();
            localPlayer.CmdChangeReadyState(true);
        }
    }
    public void onChangePlayerName(string text)
    {
        NetworkClient.localPlayer.gameObject.GetComponent<RoomPlayerManager>().CmdSetName(text);
    }
}
