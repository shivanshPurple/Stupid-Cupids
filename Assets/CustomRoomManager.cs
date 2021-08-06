using Mirror;
using UnityEngine;
public class CustomRoomManager : NetworkRoomManager
{
    public void Server()
    {
        StartServer();
    }

    public void Client()
    {
        StartClient();
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        gamePlayer.GetComponent<PlayerManager>().playerName = roomPlayer.GetComponent<RoomPlayerManager>().playerName;
        gamePlayer.GetComponent<PlayerManager>().charIndex = roomPlayer.GetComponent<RoomPlayerManager>().charIndex;
        gamePlayer.GetComponent<PlayerManager>().isHost = roomPlayer.GetComponent<RoomPlayerManager>().isHost;
        NetworkServer.Destroy(roomPlayer);
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        StopHost();
    }
}

