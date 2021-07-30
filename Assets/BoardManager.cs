using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    private RectTransform board;
    [SerializeField]
    private Text first, second;
    public enum playMode { show, hide, blank };
    public playMode mode = playMode.blank;
    public static BoardManager singleton;
    [SerializeField]
    private int width = 1920;
    void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }
    void Update()
    {
        if (mode == playMode.show)
        {
            board.anchoredPosition = Vector2.Lerp(board.anchoredPosition, Vector2.zero, 5 * Time.deltaTime);
            if (Mathf.Abs(board.anchoredPosition.x) < 1)
            {
                board.anchoredPosition = Vector2.zero;
                StartCoroutine(hideAfterSeconds());
            }
        }
        else if (mode == playMode.hide)
        {
            board.anchoredPosition = Vector2.Lerp(board.anchoredPosition, new Vector2(width, 0), 5 * Time.deltaTime);
            if (Mathf.Abs(board.anchoredPosition.x - width) < 1)
            {
                board.anchoredPosition = new Vector2(width, 0);
                mode = playMode.blank;
            }
        }
    }
    IEnumerator hideAfterSeconds()
    {
        yield return new WaitForSeconds(2);
        mode = playMode.hide;
    }
    public void showBoard(string text)
    {
        string[] split = text.Split(' ');
        first.text = split[0] + "\n";
        second.text = "\n." + split[1] + ".";
        board.anchoredPosition = new Vector3(-width, 0);
        mode = playMode.show;
    }
}
