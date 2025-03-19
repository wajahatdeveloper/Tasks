using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class ReconnectionManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject reconnectingPanel;
    [SerializeField] private float reconnectInterval = 5f;
    [SerializeField] private int maxReconnectAttempts = 3;

    private bool wasInRoom = false;
    private int currentReconnectAttempts = 0;
    private bool isReconnecting = false;
    private bool manualDisconnect = false;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        reconnectingPanel.SetActive(false);
        wasInRoom = PhotonNetwork.InRoom;
        Debug.Log($"[ReconnectionManager] Initialized. WasInRoom: {wasInRoom}, InRoom: {PhotonNetwork.InRoom}");
    }

    public void ManualDisconnect()
    {
        Debug.Log("[ReconnectionManager] Manual disconnect initiated");
        manualDisconnect = true;
        PhotonNetwork.Disconnect();
    }

    public override void OnJoinedRoom()
    {
        wasInRoom = true;
        Debug.Log($"[ReconnectionManager] Joined room: {PhotonNetwork.CurrentRoom.Name}");
        
        if (isReconnecting)
        {
            ReconnectSuccess();
        }

        manualDisconnect = false;
    }

    public override void OnLeftRoom()
    {
        if (!manualDisconnect)
        {
            wasInRoom = false;
            Debug.Log("[ReconnectionManager] Naturally left room");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"[ReconnectionManager] Disconnected. Cause: {cause}, WasInRoom: {wasInRoom}, Manual: {manualDisconnect}, Reconnecting: {isReconnecting}");
        
        if (wasInRoom && !manualDisconnect && !isReconnecting)
        {
            Debug.Log("[ReconnectionManager] Starting reconnection process");
            StartReconnectionProcess();
        }
        else
        {
            Debug.Log("[ReconnectionManager] Not eligible for reconnection");
            wasInRoom = false;
        }
    }

    private void StartReconnectionProcess()
    {
        Debug.Log($"[ReconnectionManager] Starting reconnection process. Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        isReconnecting = true;
        currentReconnectAttempts = 0;
        reconnectingPanel.SetActive(true);
        AttemptReconnection();
    }

    private void AttemptReconnection()
    {
        currentReconnectAttempts++;
        Debug.Log($"[ReconnectionManager] Attempt #{currentReconnectAttempts} (Max: {maxReconnectAttempts})");
        
        if (currentReconnectAttempts > maxReconnectAttempts)
        {
            Debug.Log("[ReconnectionManager] Max attempts reached");
            ReturnToLobby();
            return;
        }

        Debug.Log($"[ReconnectionManager] Trying ReconnectAndRejoin...");
        
        if (!PhotonNetwork.ReconnectAndRejoin())
        {
            Debug.LogError("[ReconnectionManager] Failed to start reconnection process");
            StartCoroutine(DelayedReconnectionAttempt());
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"[ReconnectionManager] Join failed. Code: {returnCode}, Message: {message}");
        if (isReconnecting)
        {
            if (returnCode == ErrorCode.GameDoesNotExist)
            {
                Debug.Log("[ReconnectionManager] Room no longer exists");
                ReturnToLobby();
            }
            else
            {
                Debug.LogWarning("[ReconnectionManager] Temporary join failure");
                StartCoroutine(DelayedReconnectionAttempt());
            }
        }
    }

    private IEnumerator DelayedReconnectionAttempt()
    {
        Debug.Log($"[ReconnectionManager] Waiting {reconnectInterval}s before next attempt");
        yield return new WaitForSeconds(reconnectInterval);
        Debug.Log("[ReconnectionManager] Restarting reconnection attempt");
        AttemptReconnection();
    }

    private void ReconnectSuccess()
    {
        Debug.Log($"[ReconnectionManager] Reconnect successful! In Room: {PhotonNetwork.InRoom}");
        isReconnecting = false;
        currentReconnectAttempts = 0;
        reconnectingPanel.SetActive(false);
    }

    private void ReturnToLobby()
    {
        Debug.Log($"[ReconnectionManager] Returning to lobby. Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        isReconnecting = false;
        currentReconnectAttempts = 0;
        reconnectingPanel.SetActive(false);
        manualDisconnect = false;
        
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("[ReconnectionManager] Leaving current room");
            PhotonNetwork.LeaveRoom();
        }
        
        PhotonNetwork.LoadLevel("LobbyScene");
        StartCoroutine(ReconnectAfterSceneLoad());
    }

    private IEnumerator ReconnectAfterSceneLoad()
    {
        Debug.Log("[ReconnectionManager] Starting scene load monitoring");
        while (PhotonNetwork.LevelLoadingProgress > 0.9f)
        {
            yield return null;
        }
        
        Debug.Log($"[ReconnectionManager] Scene load complete. Connected: {PhotonNetwork.IsConnected}");
        
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[ReconnectionManager] Reconnecting to Photon");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log($"[ReconnectionManager] Connected to master. IsReconnecting: {isReconnecting}");
        if (isReconnecting)
        {
            Debug.Log("[ReconnectionManager] Attempting ReconnectAndRejoin from OnConnectedToMaster");
            PhotonNetwork.ReconnectAndRejoin();
        }
    }

    // Add other Photon callbacks with debug logs
    public override void OnJoinedLobby()
    {
        Debug.Log("[ReconnectionManager] Joined lobby");
    }

    public override void OnConnected()
    {
        Debug.Log("[ReconnectionManager] Connected to Photon");
    }
}