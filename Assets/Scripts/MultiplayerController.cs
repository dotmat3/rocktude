using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerController {

    private static MultiplayerController instance;

    public static MultiplayerController DefaultInstance {
        get {
            if (instance == null)
                instance = new MultiplayerController();
            return instance;
        }
    }

    private NetworkController networkController;
    private RoomInfo? roomInfo;
    private IDictionary<string, (int index, JoinRoomEvent joinRoomEvent)> players;

    public MultiplayerController() {
        networkController = NetworkController.DefaultInstance;
    }

    public void Update() {
        if (roomInfo == null) return;

        networkController.HandleEvents();
    }

    public void SetGameInfo(RoomInfo roomInfo, IDictionary<string, (int index, JoinRoomEvent joinRoomEvent)> players) {
        this.roomInfo = roomInfo;
        this.players = players;
    }

    public void Exit() {
        this.roomInfo = null;
        this.players.Clear();

        networkController.Disconnect();
    }

    public RoomInfo? GetRoomInfo() {
        return roomInfo;
    }

    public IDictionary<string, (int index, JoinRoomEvent joinRoomEvent)> GetPlayers() {
        return players;
    }

    #region Multiplayer Events
    public void PlaceTurret(Turret turret) {
        networkController.SendEvent(new PlaceTurretEvent(turret.type, turret.transform.position, turret.playerId, turret.index));
    }

    public void UpgradeTurret(Turret turret) {
        networkController.SendEvent(new UpgradeTurretEvent(turret.GetIdentifier()));
    }

    public void StartButton() {
        networkController.SendEvent(new StartButtonEvent());
    }

    #endregion
}
