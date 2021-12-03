using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Turret))]
public class TurretRadius : Editor {

    void OnSceneGUI() {
        Turret turret = (Turret) target;
        Handles.color = Color.white;
        Handles.DrawWireArc(turret.transform.position, Vector3.up, Vector3.forward, 360, turret.radius);
    }
}
