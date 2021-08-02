using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField]
    private Image imgElement;
    [SerializeField]
    private InputField nameInput;
    private int currentCharUi = 1;
    void Start()
    {
        imgElement.sprite = Resources.Load<Sprite>("Characters/Asset " + currentCharUi);
        imgElement.preserveAspect = true;
        nameInput.onValueChanged.AddListener(onChangePlayerName);
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
        NetworkClient.localPlayer.gameObject.GetComponent<RoomPlayerManager>().CmdChangeReadyState(true);
    }
    public void onChangePlayerName(string text)
    {
        NetworkClient.localPlayer.gameObject.GetComponent<RoomPlayerManager>().CmdSetName(text);
    }
}
