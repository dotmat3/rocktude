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

public class PlaceTurretEvent : SocketEvent {
    public int index;
    public Vector3 position;

    public PlaceTurretEvent(int index, Vector3 position) {
        this.index = index;
        this.position = position;
    }

    public override void ExecuteHandler() {
        TurretPlacer turretPlacer = Object.FindObjectOfType<TurretPlacer>();
        Turret turret = turretPlacer.PlaceTurret(index, position);
        turret.Activate();
        turret.ChangeStatus(Turret.TurretStatus.Idle);
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
        Missile missile = Object.FindObjectOfType<Missile>();
        missile.transform.position = position;
        missile.transform.rotation = rotation;
    }
}