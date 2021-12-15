using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;

[Serializable]
public struct Rooms {
    public List<RoomInfo> rooms;
}

[Serializable]
public struct RoomInfo {
    public string name;
    public int map;
    public int maxPlayers;
    public string code;
}


public class RoomController : MonoBehaviour {
    public const string SERVER_ADDRESS = "http://skylion.zapto.org";
    public const int SERVER_PORT = 2043;

    // Index of the map associated to the level 1
    public const int START_MAP_INDEX = 4;

    [Header("Main")]
    public GameObject mainUI;

    [Header("Join Room")]
    public GameObject joinRoomUI;
    public GameObject roomsList;
    public GameObject roomPrefab;

    [Header("Create Room")]
    public GameObject createRoomUI;
    public InputField nameField;
    public Dropdown mapDropdown;
    public Dropdown playersDropdown;

    [Header("Waiting Room")]
    public GameObject waitRoomUI;
    public Text roomName;
    public RawImage QRCode;
    public Text roomCode;
    public List<GameObject> waitingPlayers;
    public Button startButton;

    private NetworkController networkController;
    private FirebaseAuth auth;

    private RoomInfo? roomInfo;
    private IDictionary<string, (int index, JoinRoomEvent joinRoomEvent)> joinedPlayers = new Dictionary<string, (int, JoinRoomEvent)>();

    void Start() {
        networkController = NetworkController.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    void Update() {
        networkController.HandleEvents();
    }

    #region Create Room
    public void CreateRoom() {
        string url = $"{SERVER_ADDRESS}:{SERVER_PORT}/room";

        RoomInfo createRoomInfo = new RoomInfo {
            name = nameField.text,
            map = mapDropdown.value,
            maxPlayers = playersDropdown.value + 2
        };

        string data = JsonUtility.ToJson(createRoomInfo);

        void callback(UnityWebRequest request) {
            string response = request.downloadHandler.text;

            RoomInfo roomInfo = JsonUtility.FromJson<RoomInfo>(response);
            roomInfo.name = createRoomInfo.name;
            roomInfo.map = createRoomInfo.map;
            roomInfo.maxPlayers = createRoomInfo.maxPlayers;

            createRoomUI.SetActive(false);
            ShowWaitingRoom(roomInfo);
        }

        StartCoroutine(NetworkController.SendPostRequest(url, data, callback));
    }
    #endregion

    #region Waiting Room
    public void ShowWaitingRoom(RoomInfo roomInfo) {
        ResetWaitingRoom();
        
        this.roomInfo = roomInfo;

        waitRoomUI.SetActive(true);

        for (int i = 0; i < waitingPlayers.Count; i++)
            if (i >= roomInfo.maxPlayers)
                waitingPlayers[i].SetActive(false);

        roomName.text = "Name: " + roomInfo.name;
        roomCode.text = "Code: " + roomInfo.code;

        string QRData = JsonUtility.ToJson(roomInfo);
        string url = $"http://api.qrserver.com/v1/create-qr-code/?data={QRData}&size=400x400";

        void callback(UnityWebRequest request) => QRCode.texture = ((DownloadHandlerTexture) request.downloadHandler).texture;

        StartCoroutine(NetworkController.SendGetTextureRequest(url, callback));

        networkController.Connect();

        UserController userController = UserController.DefaultInstance;
        string userId = userController.GetUserId();
        string username = userController.GetUsername();
        JoinRoomEvent joinRoomEvent = new JoinRoomEvent(roomInfo.code, userId, username);
        networkController.SendEvent(joinRoomEvent);

        Debug.Log("Sent Join Room event");

        joinedPlayers[userId] = (0, joinRoomEvent);
        waitingPlayers[0].GetComponent<Text>().text = "You";
    }

    public void OnRoomLeave() {
        networkController.Disconnect();

        ResetWaitingRoom();
    }

    private void ResetWaitingRoom() {
        roomInfo = null;

        roomName.text = "Name: ";
        roomCode.text = "Code: ";

        QRCode.texture = Texture2D.whiteTexture;

        for (int i = 0; i < waitingPlayers.Count; i++)
            waitingPlayers[i].SetActive(true);

        joinedPlayers.Clear();
        UpdateRoomPlayers();

        startButton.gameObject.SetActive(false);
    }

    public void OnPlayerJoin(JoinRoomEvent joinRoomEvent) {
        Debug.Log("New player joined! Welcome, " + joinRoomEvent.username);

        int playerIndex = joinedPlayers.Count;
        joinedPlayers[joinRoomEvent.userId] = (playerIndex, joinRoomEvent);
        UpdateRoomPlayers();

        if (joinedPlayers.Count >= 2)
            startButton.gameObject.SetActive(true);
    }

    public void OnPlayerLeave(LeaveRoomEvent leaveRoomEvent) {
        Debug.Log("Player left the room! Goodbye, " + leaveRoomEvent.username);

        int playerIndex = joinedPlayers[leaveRoomEvent.userId].index;

        if (joinedPlayers.ContainsKey(leaveRoomEvent.userId))
            joinedPlayers.Remove(leaveRoomEvent.userId);

        List<string> keys = new List<string>(joinedPlayers.Keys);
        foreach (string userId in keys) {
            var (index, joinRoomEvent) = joinedPlayers[userId];
            if (index >= playerIndex)
                joinedPlayers[userId] = (index - 1, joinRoomEvent);
        }

        UpdateRoomPlayers();

        if (joinedPlayers.Count < 2)
            startButton.gameObject.SetActive(false);
    }

    private void UpdateRoomPlayers() {
        for (int i = 0; i < waitingPlayers.Count; i++)
            waitingPlayers[i].GetComponent<Text>().text = $"Player {i + 1}";

        string currentUsername = UserController.DefaultInstance.GetUsername();

        foreach (var (index, joinRoomEvent) in joinedPlayers.Values)
            if (joinRoomEvent.username == currentUsername)
                waitingPlayers[index].GetComponent<Text>().text = "You";
            else
                waitingPlayers[index].GetComponent<Text>().text = joinRoomEvent.username;
    }

    public void SendStartGame() {
        networkController.SendEvent(new StartGameEvent());
    }

    public void StartGame(StartGameEvent startGameEvent) {
        if (roomInfo == null) return;

        MultiplayerController.DefaultInstance.SetGameInfo(roomInfo.Value, joinedPlayers);
        SceneManager.LoadScene(roomInfo.Value.map + START_MAP_INDEX);
    }
    #endregion

    #region Join Room
    public void LoadRooms() {
        string url = $"{SERVER_ADDRESS}:{SERVER_PORT}/rooms";

        foreach (Transform child in roomsList.transform)
            Destroy(child.gameObject);

        void callback(UnityWebRequest request) {
            Debug.Log(request.downloadHandler.text);

            Rooms roomsStruct = JsonUtility.FromJson<Rooms>(request.downloadHandler.text);

            roomsStruct.rooms.ForEach((room) => {
                GameObject roomObject = Instantiate(roomPrefab, roomsList.transform);
                roomObject.GetComponentInChildren<Text>().text = room.name + " (" + room.code + ")";
                roomObject.GetComponentInChildren<Button>().onClick.AddListener(() => {
                    Debug.Log("Clicked " + room.name + " - " + room.code);
                    joinRoomUI.SetActive(false);
                    ShowWaitingRoom(room);
                });
            });
        };

        StartCoroutine(NetworkController.SendGetRequest(url, callback));
    }
    #endregion

    #region Main
    public void BackToMainScene() {
        SceneManager.LoadScene(0);
    }
    #endregion
}
