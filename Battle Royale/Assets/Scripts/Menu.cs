using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{

    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;
    public GameObject lobbyBrowseScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefab;

    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>();

    void Start()
    {

        // disable menu buttons at start
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;

        // enable cursor since its hidden in the game
        Cursor.lockState = CursorLockMode.None;

        // are we in a game?
        if (PhotonNetwork.InRoom)
        {
            // go to lobby
            SetScreen(lobbyScreen);
            UpdateLobbyUI();

            // make room visible again
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;

        }

    }

    // changes currently visible screen
    void SetScreen(GameObject screen)
    {

        // disable all other screens
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        lobbyBrowseScreen.SetActive(false);

        // activate the requested screen
        screen.SetActive(true);

        if(screen == lobbyBrowseScreen)
            UpdateLobbyBrowserUI();

    }

    public void OnBackButton()
    {

        SetScreen(mainScreen);

    }

    // MAIN SCREEN

    public void OnPlayerNameValueChanged (TMP_InputField playerNameInput)
    {

        PhotonNetwork.NickName = playerNameInput.text;
        
    }

    public override void OnConnectedToMaster()
    {

        // enable menu buttons once connected to server 
        createRoomButton.interactable = true;
        findRoomButton.interactable = true;

    }
    
    public void OnCreateRoomButton ()
    {

        SetScreen(createRoomScreen);

    }

    public void OnFindRoomButton ()
    {

        SetScreen(lobbyBrowseScreen);

    }

    // CREATE ROOM SCREEN

    public void OnCreateButton (TMP_InputField roomNameInput)
    {

        NetworkManager.instance.CreateRoom(roomNameInput.text);

    }

    // LOBBY SCREEN

    public override void OnJoinedRoom ()
    {

        SetScreen(lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);

    }

    public override void OnPlayerLeftRoom (Player player)
    {

        UpdateLobbyUI();

    }

    [PunRPC]
    void UpdateLobbyUI ()
    {

        // enable or disable start button depending on if we're the host
        startGameButton.interactable = PhotonNetwork.IsMasterClient;

        // display all players
        playerListText.text = "";

        foreach (Player player in PhotonNetwork.PlayerList)
            playerListText.text += player.NickName + "\n";

        // set room info text
        roomInfoText.text = "<b>Room Name</b>\n" + PhotonNetwork.CurrentRoom.Name;

    }

    public void OnStartGameButton ()
    {

        // hide room
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // tell everyone to load game scene
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");

    }

    public void OnLeaveLobbyButton ()
    {

        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);

    }

    // LOBBY BROWSER SCREEN

    GameObject CreateRoomButton ()
    {

        GameObject buttonObj = Instantiate(roomButtonPrefab, roomListContainer.transform);
        roomButtons.Add(buttonObj);

        return buttonObj;

    }

    void UpdateLobbyBrowserUI ()
    {

        // disable all room buttons
        foreach(GameObject button in roomButtons)
            button.SetActive(false);

        // display all current rooms in master server
        for (int x = 0; x < roomList.Count; ++x)
        {

            // get or create buttons object
            GameObject button = x >= roomButtons.Count ? CreateRoomButton() : roomButtons[x];

            button.SetActive(true);

            // set room name and player text
            button.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = roomList[x].Name;
            button.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = roomList[x].PlayerCount + " / " + roomList[x].MaxPlayers;

            // set the button onclick event
            Button buttonComp = button.GetComponent<Button>();

            string roomName = roomList[x].Name;

            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => { OnJoinRoomButton(roomName); });

        }

    }

    public void OnJoinRoomButton (string roomName)
    {

        NetworkManager.instance.JoinRoom(roomName);

    }

    public void OnRefreshButton ()
    {

        UpdateLobbyBrowserUI();

    }

    public override void OnRoomListUpdate (List<RoomInfo> allRooms)
    {

        roomList = allRooms;

    }

}
