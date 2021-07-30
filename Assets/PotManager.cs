using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class PotManager : NetworkBehaviour
{
    private class cardInPot
    {
        public Transform transform;
        public Vector3 defaultPosition;
        private int row;
        public cardInPot(Transform transform)
        {
            this.transform = transform;
        }
        public void setRow(int r, int indexInRow)
        {
            row = r;
            transform.GetComponent<SpriteRenderer>().sortingOrder = 2 * row + indexInRow + 2;
            transform.GetComponentInChildren<Canvas>().sortingOrder = 2 * row + indexInRow + 2;
        }
        public void lerpPos(float v)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, defaultPosition, v);
        }
    }
    private List<cardInPot> cards = new List<cardInPot>();
    public GameObject CardPrefab;
    private SpriteRenderer shadow;
    public Transform pot;
    private Vector3 normalPosition;
    private int currentRow = 0;
    internal bool isZoomed = false;
    private Vector3 defaultScale, zoomedPosition = new Vector3(0, 0, 0), zoomedScale = new Vector3(1.5f, 1.5f, 0);
    public float smoothness = 20;
    [SyncVar(hook = nameof(pointsHook))]
    private int points = 0;
    public Text pointText;
    [SerializeField]
    private Button DateButton;
    

    void Start()
    {
        pointText.text = points.ToString();
        shadow = GameObject.FindGameObjectWithTag("Shadow").GetComponent<SpriteRenderer>();
        shadow.transform.position = new Vector3(0, 0, 0);
        defaultScale = pot.localScale;
        DateButton.onClick.AddListener(date);
    }
    [ClientRpc]
    internal void addNewCard(string str, bool isPerk, bool shouldZoom)
    {
        Transform temp = (Instantiate(CardPrefab)).transform;
        if (!isPerk)
            temp.GetComponent<SpriteRenderer>().color = new Color(248f / 255, 65f / 255, 65f / 255);
        temp.GetComponentInChildren<TextMeshProUGUI>().text = str;
        temp.SetParent(pot);
        temp.localPosition = Vector3.zero;
        temp.localRotation = Quaternion.identity;
        temp.localScale = Vector3.one * 0.5f;
        cards.Add(new cardInPot(temp));

        List<Vector3> thisRowPositions;
        List<Transform> thisRowCards;
        if (cards.Count == 2 * (currentRow + 1))
            thisRowCards = new List<Transform>() { cards[2 * currentRow].transform, cards[2 * currentRow + 1].transform };
        else
            thisRowCards = new List<Transform>() { cards[2 * currentRow].transform };

        thisRowPositions = utils.objectSpacings(thisRowCards.Count, 2.5f);

        for (int j = 0; j < thisRowPositions.Count; j++)
        {
            thisRowPositions[j] -= new Vector3(0, 2.5f * currentRow, 0);
            cards[2 * currentRow + j].defaultPosition = thisRowPositions[j];
            cards[2 * currentRow + j].setRow(currentRow, j);
            if (j == 1) currentRow++;
        }
        if (shouldZoom && NetworkClient.localPlayer.netId == GameManager.singleton.turnId)
            isZoomed = true;
    }
    void Update()
    {
        for (int i = 0; i < cards.Count; i++)
            cards[i].lerpPos(smoothness * Time.deltaTime);
        if (isZoomed)
        {
            pot.position = Vector3.Lerp(pot.position, zoomedPosition, smoothness * Time.deltaTime);
            pot.localScale = Vector3.Lerp(pot.localScale, zoomedScale, smoothness * Time.deltaTime);
            shadow.color = Color.Lerp(shadow.color, new Color(0, 0, 0, 0.5f), smoothness * Time.deltaTime);
        }
        else
        {
            pot.position = Vector3.Lerp(pot.position, transform.GetChild(0).position + new Vector3(0, 4, 0), smoothness * Time.deltaTime);
            pot.localScale = Vector3.Lerp(pot.localScale, defaultScale, smoothness * Time.deltaTime);
            shadow.color = Color.Lerp(shadow.color, new Color(0, 0, 0, 0), smoothness * Time.deltaTime);
        }
    }

    internal void zoomCards() { isZoomed = isZoomed == false; }

    [TargetRpc]
    internal void showDateButton(NetworkConnection target, bool show)
    {
        if (!isLocalPlayer)
        {
            DateButton.gameObject.SetActive(show);
        }
    }

    [Command(requiresAuthority = false)]
    void date()
    {
        points++;
        GameManager.singleton.dateIsChosen();
    }

    void pointsHook(int oldPoints, int newPoints)
    {
        pointText.text = newPoints.ToString();
    }

    [ClientRpc]
    internal void resetPot()
    {
        foreach (cardInPot card in cards)
            Destroy(card.transform.gameObject);
        cards.Clear();
        currentRow = 0;
    }
}
