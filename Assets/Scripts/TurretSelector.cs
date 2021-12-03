using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSelector : MonoBehaviour {

    public Camera cam;

    private Turret selectedTurret;
    private LayerMask turretsMask;

    void Start() {
        turretsMask = LayerMask.GetMask("Turret");
    }

    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, turretsMask)) {
                if (selectedTurret)
                    selectedTurret.ChangeStatus(Turret.TurretStatus.Idle);

                selectedTurret = hit.transform.gameObject.GetComponent<Turret>();
                selectedTurret.ChangeStatus(Turret.TurretStatus.Selected);
            } else if (selectedTurret) selectedTurret.ChangeStatus(Turret.TurretStatus.Idle);
        }
    }
}
