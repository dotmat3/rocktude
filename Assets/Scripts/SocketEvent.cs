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
        GameObject.FindObjectOfType<RoomController>().OnPlayerLeave(this);
    }
}

public class StartGameEvent : SocketEvent {

    public override void ExecuteHandler() {
        GameObject.FindObjectOfType<RoomController>().StartGame(this);
    }
}
#endregion

#region Game Events
public class PlaceTurretEvent : SocketEvent {
    public int index;
    public Vector3 position;

    public PlaceTurretEvent(int index, Vector3 position) {
        this.index = index;
        this.position = position;
    }

    public override void ExecuteHandler() {
        TurretPlacer turretPlacer = GameObject.FindObjectOfType<TurretPlacer>();
        Turret turret = turretPlacer.PlaceTurret(index, position);
        turret.Activate();
        turret.ChangeStatus(Turret.TurretStatus.Idle);
    }
}

public class StartButtonEvent : SocketEvent {

    public override void ExecuteHandler() {
        GameObject.FindObjectOfType<StartButtonBehaviour>().HandleButtonClick();
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
        missile.transform.position = position;
        missile.transform.rotation = rotation;
    }
}
#endregion