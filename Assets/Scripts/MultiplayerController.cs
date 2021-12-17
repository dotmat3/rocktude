using System;
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

    public void OnPlayerLeave(LeaveRoomEvent leaveRoomEvent) {
        players.Remove(leaveRoomEvent.userId);
        GameObject.FindObjectOfType<GameController>().UpdatePlayers();
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

    public void SellTurret(Turret turret) {
        networkController.SendEvent(new SellEvent(turret.GetIdentifier()));
    }

    public void SpawnMissile() {
        networkController.SendEvent(new MissileEvent());
    }

    public void UpdateMissile(Missile missile) {
        networkController.SendEvent(new UpdateMissileEvent(missile.transform.position, missile.transform.rotation));
    }

    public void StartButton() {
        networkController.SendEvent(new StartButtonEvent());
    }

    public void EnableMalus(List<string> turretsIds) {
        networkController.SendEvent(new EnableMalusEvent(turretsIds));
    }

    public void DisableMalus(string turretId) {
        networkController.SendEvent(new DisableMalusEvent(turretId));
    }

    public void ToggleSpeedUp() {
        networkController.SendEvent(new SpeedUpEvent());
    }

    #endregion
}
