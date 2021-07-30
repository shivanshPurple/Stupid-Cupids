using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CardStackHandler : MonoBehaviour
{
    private List<Transform> cards = new List<Transform>();
    private List<Vector3> normalPositions = new List<Vector3>();
    public GameObject CardPrefab;
    public int mouseOverCard = -1;
    private float smoothness = 7;
    private int playedCard = -1;
    private bool isPlayedCustom;
    private SpriteRenderer shadow;

    void Start()
    {
        shadow = GameObject.FindGameObjectWithTag("Shadow").GetComponent<SpriteRenderer>();
    }
    public void createNewCards(List<string> strs, bool isPerk)
    {
        foreach (string str in strs)
        {
            GameObject temp = Instantiate(CardPrefab);
            if (!isPerk)
                temp.GetComponent<SpriteRenderer>().color = new Color(248f / 255, 65f / 255, 65f / 255);
            temp.GetComponentInChildren<TextMeshProUGUI>().text = str;
            temp.transform.parent = transform;
            cards.Add(temp.transform);
            temp.transform.localPosition += new Vector3(0, 7.5f, 0);
        }
        normalPositions = utils.objectSpacings(cards.Count, 2);
        for (int i = 0; i < cards.Count; i++)
        {
            normalPositions.Add(cards[i].localPosition);
            cards[i].GetComponent<SpriteRenderer>().sortingOrder = 10 + i;
            cards[i].GetComponentInChildren<Canvas>().sortingOrder = 10 + i;
        }
        showCards(false);
    }

    void Update()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (i != playedCard)
            {
                Vector3 finalPos = normalPositions[i];
                if (i < mouseOverCard) finalPos += new Vector3(1.2f, 0, 0);
                if (i == mouseOverCard) finalPos += new Vector3(0, 1, 0);
                if (i > mouseOverCard) finalPos += new Vector3(-1.2f, 0, 0);
                if (mouseOverCard == -1)
                {
                    finalPos = normalPositions[i];
                    cards[i].localRotation = Quaternion.Lerp(cards[i].localRotation, Quaternion.identity, smoothness * Time.deltaTime);
                }
                if (mouseOverCard != -1)
                    cards[i].localRotation = Quaternion.Lerp(cards[i].localRotation, Quaternion.Euler(0, 0, (i - mouseOverCard) * 2), smoothness * Time.deltaTime);
                cards[i].localPosition = Vector3.Lerp(cards[i].localPosition, finalPos, smoothness * Time.deltaTime);
            }
        }
        if (playedCard != -1)
        {
            if (isPlayedCustom)
            {
                cards[playedCard].position = Vector3.Lerp(cards[playedCard].position, new Vector3(0, -2, 0), smoothness * Time.deltaTime);
                shadow.color = Color.Lerp(shadow.color, new Color(0, 0, 0, 0.5f), smoothness * Time.deltaTime);
            }
            else
            {
                if (9.8f - cards[playedCard].localPosition.y < 1f)
                {
                    GameManager.singleton.putCardOnTable(cards[playedCard].GetComponentInChildren<TextMeshProUGUI>().text);
                    Destroy(cards[playedCard].gameObject);
                    cards.RemoveAt(playedCard);
                    playedCard = -1;
                    shadow.sortingOrder = 0;
                    normalPositions = utils.objectSpacings(cards.Count, 2);
                }
                else
                {
                    cards[playedCard].localPosition = Vector3.Lerp(cards[playedCard].localPosition, Vector3.down * -20, smoothness * Time.deltaTime);
                    shadow.color = Color.Lerp(shadow.color, new Color(0, 0, 0, 0.5f), smoothness * Time.deltaTime);
                }
            }
        }
    }
    public void OnMouseHover(Transform t)
    {
        if (t != null && !isPlayedCustom) mouseOverCard = cards.IndexOf(t);
        else mouseOverCard = -1;
    }
    public void playCard(Transform t)
    {
        if (playedCard == -1)
        {
            playedCard = cards.IndexOf(t);
            isPlayedCustom = cards[playedCard].GetComponentInChildren<TextMeshProUGUI>().text.Contains("__");
            if (isPlayedCustom)
            {
                cards[playedCard].GetComponent<SpriteRenderer>().sortingOrder = 21;
                cards[playedCard].GetComponentInChildren<Canvas>().sortingOrder = 21;
                shadow.sortingOrder = 20;
                TextMeshProUGUI fake = cards[playedCard].GetComponentInChildren<TextMeshProUGUI>();
                InputField typing = cards[playedCard].GetComponentInChildren<InputField>();
                string[] split = fake.text.Split(new string[] { "_____" }, System.StringSplitOptions.None);
                split[0] += "<mark=#00a6ff50>";
                split[1] = "</mark>" + split[1];
                string emptyPlaceholder = "[....]";
                fake.text = split[0] + emptyPlaceholder + split[1];
                typing.interactable = true;
                typing.Select();
                typing.onValueChanged.AddListener((string value) =>
                {
                    if (value == "")
                        fake.text = split[0] + emptyPlaceholder + split[1];
                    else
                        fake.text = split[0] + value + split[1];
                });
                typing.onEndEdit.AddListener((_) =>
                {
                    if (typing.text != "")
                    {
                        fake.text = fake.GetParsedText();
                        isPlayedCustom = false;
                    }
                });
            }
        }
    }
    internal void showCards(bool show)
    {
        cards.ForEach((Transform card) => card.gameObject.SetActive(show));
    }
}
