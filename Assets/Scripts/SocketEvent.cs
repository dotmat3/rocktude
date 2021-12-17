using System.Collections.Generic;
using UnityEngine;

public class SocketEvent {
    public string type;

    public SocketEvent() {
        type = GetType().Name;
    }
    public string ToJson() {
        return JsonUtility.ToJson(this);
    }

    public virtual void ExecuteHandler() { Debug.Log("Event base handler called"); }
}

#region Room Events
public class JoinRoomEvent : SocketEvent {
    public string code;
    public string userId;
    public string username;

    public JoinRoomEvent(string code, string userId, string username) {
        this.code = code;
        this.userId = userId;
        this.username = username;
    }

    public override void ExecuteHandler() {
        GameObject.FindObjectOfType<RoomController>().OnPlayerJoin(this);
    }
}

public class LeaveRoomEvent : SocketEvent {
    public string code;
    public string userId;
    public string username;

    public LeaveRoomEvent(string code, string userId, string username) {
        this.code = code;
        this.userId = userId;
        this.username = username;
    }

    public override void ExecuteHandler() {
        RoomController roomController = GameObject.FindObjectOfType<RoomController>();
        if (roomController)
            roomController.OnPlayerLeave(this);
        else {
            MultiplayerController multiplayerController = MultiplayerController.DefaultInstance;
            multiplayerController.OnPlayerLeave(this);
        }
    }
}

public class StartGameEvent : SocketEvent {

    public override void ExecuteHandler() {
        RoomController roomController = GameObject.FindObjectOfType<RoomController>();
        if (roomController)
            roomController.StartGame(this);
    }
}
#endregion

#region Game Events
public class PlaceTurretEvent : SocketEvent {
    public int turretType;
    public Vector3 position;
    public string playerId;
    public int index;

    public PlaceTurretEvent(int turretType, Vector3 position, string playerId, int index) {
        this.turretType = turretType;
        this.position = position;
        this.playerId = playerId;
        this.index = index;
    }

    public override void ExecuteHandler() {
        TurretPlacer turretPlacer = GameObject.FindObjectOfType<TurretPlacer>();
        Turret turret = turretPlacer.PlaceTurret(turretType, position, playerId, index);
        turret.Activate();
        turret.ChangeStatus(Turret.TurretStatus.Idle);

        // Store the placed turret info
        GameController gameController = GameObject.FindObjectOfType<GameController>();
        gameController.StoreTurret(turret);
    }
}

public class UpgradeTurretEvent : SocketEvent {
    public string index;

    public UpgradeTurretEvent(string index) {
        this.index = index;
    }

    public override void ExecuteHandler() {
        UpgradeController upgradeController = GameObject.FindObjectOfType<UpgradeController>();
        upgradeController.UpgradeTurret(index);
    }
}

public class SellEvent : SocketEvent {
    public string index;

    public SellEvent(string index) {
        this.index = index;
    }

    public override void ExecuteHandler() {
        UpgradeController upgradeController = GameObject.FindObjectOfType<UpgradeController>();
        upgradeController.SellTurret(index);
    }
}

public class StartButtonEvent : SocketEvent {

    public override void ExecuteHandler() {
        GameObject.FindObjectOfType<StartButtonBehaviour>().HandleButtonClick();
    }
}

public class MissileEvent : SocketEvent {

    public override void ExecuteHandler() {
        GameController gameController = GameObject.FindObjectOfType<GameController>();
        Missile missile = gameController.SpawnMissile();
        missile.Deactivate();
    }
}

public class UpdateMissileEvent : SocketEvent {
    public Vector3 position;
    public Quaternion rotation;

    public UpdateMissileEvent(Vector3 position, Quaternion rotation) {
        this.position = position;
        this.rotation = rotation;
    }

    public override void ExecuteHandler() {
        Missile missile = GameObject.FindObjectOfType<Missile>();
        if (missile) {
            missile.transform.position = position;
            missile.transform.rotation = rotation;
        }
    }
}

public class EnableMalusEvent : SocketEvent {
    public List<string> disabledTurrets;

    public EnableMalusEvent(List<string> disabledTurrets) {
        this.disabledTurrets = disabledTurrets;
    }

    public override void ExecuteHandler() {
        MalusController malusController = GameObject.FindObjectOfType<MalusController>();
        malusController.DisableTurrets(disabledTurrets);        
    }
}

public class DisableMalusEvent : SocketEvent {
    public string turretId;

    public DisableMalusEvent(string turretId) {
        this.turretId = turretId;
    }

    public override void ExecuteHandler() {
        MalusController malusController = GameObject.FindObjectOfType<MalusController>();
        malusController.EnableTurret(turretId);
    }
}

public class SpeedUpEvent : SocketEvent {

    public override void ExecuteHandler() {
        GameController gameController = GameObject.FindObjectOfType<GameController>();
        gameController.ToggleSpeedUpTime();
    }
}
#endregion
