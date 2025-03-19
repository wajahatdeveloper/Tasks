using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomInfoText;
    [SerializeField] private Button joinButton;
    
    private string roomName;
    
    public System.Action OnJoinButtonPressed;
    
    private void Start()
    {
        joinButton.onClick.AddListener(() => OnJoinButtonPressed?.Invoke());
    }
    
    public void SetRoomInfo(RoomInfo room)
    {
        roomName = room.Name;
        roomInfoText.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
    }
}