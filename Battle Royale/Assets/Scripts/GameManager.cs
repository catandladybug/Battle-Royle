using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{

    [Header("Players")]
    public string playerPrefabLocation;
    public PlayerControl[] players;
    public Transform[] spawnPoints;
    public int alivePlayers;

    private int playersInGame;

    // instance
    public static GameManager instance;

    void Awake()
    {
        
        instance = this;

    }

    void Start()
    {
        
        players = new PlayerControl[PhotonNetwork.PlayerList.Length];
        alivePlayers = players.Length;

        photonView.RPC("ImInGame", RpcTarget.AllBuffered);

    }

    [PunRPC]
    void ImInGame ()
    {

        playersInGame++;

        if (PhotonNetwork.IsMasterClient && playersInGame == PhotonNetwork.PlayerList.Length)
            photonView.RPC("SpawnPlayer", RpcTarget.All);

    }

    [PunRPC]
    void SpawnPlayer ()
    {

        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        // initialize player for all players
        playerObj.GetComponent<PlayerControl>().photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);

    }

}
