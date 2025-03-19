using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject roomListPanel;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRandomButton;
    [SerializeField] private TextMeshProUGUI connectionStatus;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private GameObject roomListItemPrefab;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        lobbyPanel.SetActive(false);
        roomListPanel.SetActive(false);
        
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        
        // Load saved player name
        playerNameInput.text = PlayerPrefs.GetString("PlayerName", "Player" + Random.Range(1000, 10000));
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        PhotonNetwork.JoinLobby();
        connectionStatus.text = "Connected to Master Server";
    }

    public override void OnJoinedLobby()
    {
        lobbyPanel.SetActive(true);
        roomListPanel.SetActive(true);
        connectionStatus.text = "In Lobby";
        
        // Update player name if changed
        PhotonNetwork.NickName = playerNameInput.text;
        PlayerPrefs.SetString("PlayerName", PhotonNetwork.NickName);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomList();
        
        foreach (RoomInfo room in roomList)
        {
            if (!room.IsVisible || room.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList.Remove(room.Name);
                }
                continue;
            }

            if (cachedRoomList.ContainsKey(room.Name))
            {
                cachedRoomList[room.Name] = room;
            }
            else
            {
                cachedRoomList.Add(room.Name, room);
            }
        }

        UpdateRoomListUI();
    }

    private void UpdateRoomListUI()
    {
        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject listItem = Instantiate(roomListItemPrefab, roomListContent);
            RoomListItem item = listItem.GetComponent<RoomListItem>();
            item.SetRoomInfo(room);
            item.OnJoinButtonPressed += () => JoinRoom(room.Name);
        }
    }

    private void ClearRoomList()
    {
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }
        cachedRoomList.Clear();
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.LogError("Room name cannot be empty!");
            return;
        }

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomNameInput.text, options);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel("GameScene"); // Load your game scene
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Room creation failed: " + message);
        // Show error UI
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError("Join random failed: " + message);
        // Show error UI
    }

    public void UpdatePlayerName()
    {
        PhotonNetwork.NickName = playerNameInput.text;
        PlayerPrefs.SetString("PlayerName", PhotonNetwork.NickName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        lobbyPanel.SetActive(false);
        roomListPanel.SetActive(false);
        connectionStatus.text = "Disconnected: " + cause.ToString();
    }
}