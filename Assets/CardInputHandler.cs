using UnityEngine;
using Mirror;

public class CardInputHandler : MonoBehaviour
{
    void OnMouseEnter()
    {
        if (transform.GetComponentInParent<CardStackHandler>() != null)
            transform.GetComponentInParent<CardStackHandler>().OnMouseHover(transform);
    }
    void OnMouseExit()
    {
        if (transform.GetComponentInParent<CardStackHandler>() != null)
            transform.GetComponentInParent<CardStackHandler>().OnMouseHover(null);
    }
    void OnMouseUp()
    {
        if (transform.GetComponentInParent<CardStackHandler>() != null)
            transform.GetComponentInParent<CardStackHandler>().playCard(transform);
        if (transform.GetComponentInParent<PotManager>() != null && (NetworkClient.localPlayer.netId != GameManager.singleton.turnId || GameManager.singleton.round == GameRound.Dating))
            transform.GetComponentInParent<PotManager>().zoomCards();
    }
}