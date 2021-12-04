using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurretSelector : MonoBehaviour {

    public Camera cam;

    public GameObject upgradeDrawer;

    private Turret selectedTurret;

    private LayerMask turretsMask;
    private TurretPlacer turretPlacer;
    private UpgradeController upgradeController;

    void Start() {
        turretsMask = LayerMask.GetMask("Turret");

        turretPlacer = FindObjectOfType<TurretPlacer>();
        upgradeController = upgradeDrawer.GetComponent<UpgradeController>();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0) && !turretPlacer.IsBuying() && !EventSystem.current.IsPointerOverGameObject()) {

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, turretsMask))
                OnTurretSelected(hit.transform.gameObject.GetComponent<Turret>());
            else
                OnClearSelection();
        }
    }

    void OnTurretSelected(Turret turret) {
        if (selectedTurret)
            selectedTurret.ChangeStatus(Turret.TurretStatus.Idle);
        
        selectedTurret = turret;
        selectedTurret.ChangeStatus(Turret.TurretStatus.Selected);

        upgradeController.UpdateView(selectedTurret);
        upgradeController.Show();
    }

    void OnClearSelection() {
        if (selectedTurret) {
            selectedTurret.ChangeStatus(Turret.TurretStatus.Idle);
            selectedTurret = null;

            upgradeController.Hide();
        }
    }
}
