using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretPlacer : MonoBehaviour {

    public Camera cam;
    public Turret turret;
    public int turretCost = 100;
    public GameController gameController;
    public List<Turret> turrets;

    private Turret currentTurret;
    private NetworkController networkController;

    public float collisionRadius = 1f;

    private int turretsMask;

    public Button buyButton;

    private bool isBuying;

    private void Start() {
        networkController = gameController.GetComponent<NetworkController>();

        turretsMask = LayerMask.GetMask("Turret");

        buyButton.onClick.AddListener(onBuyClick);

        isBuying = false;
    }

    void Update() {
        if (isBuying) {
            HandleNewTurret();

            if (currentTurret != null) {
                    FollowMouse();
                    // CheckTurretCollisions();
                    HandlePlacement();
            }
        }
    }

    void onBuyClick() {
        if (isBuying) {
            isBuying = false;
            Text btnText = buyButton.GetComponent<Text>();
            btnText.text = "$" + turretCost;
        } else {

            if (gameController.money < turretCost) return;

            isBuying = true;
            Text btnText = buyButton.GetComponent<Text>();
            btnText.text = "Buying";
        }
    }

    void HandleNewTurret() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
                currentTurret = PlaceTurret(turret, hit.point);
        }
    }

    void FollowMouse() {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~turretsMask)) {
            currentTurret.transform.position = hit.point;
            // currentTurret.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }

    void HandlePlacement() {
        if (Input.GetMouseButtonUp(0)) {
            networkController.SendEvent(new PlaceTurretEvent(0, currentTurret.transform.position));

            currentTurret.Activate();
            currentTurret.ChangeStatus(Turret.TurretStatus.Idle);
            currentTurret = null;
            
            isBuying = false;

            Text btnText = buyButton.GetComponent<Text>();
            btnText.text = "$" + turretCost;

            gameController.UpdateMoney(gameController.money - turretCost);
        }
    }

    public Turret PlaceTurret(int index, Vector3 position) {
        Turret gameObject = turrets[index];

        return PlaceTurret(gameObject, position);
    }

    public Turret PlaceTurret(Turret turret, Vector3 position) {
        Turret gameObject = Instantiate(turret, position, Quaternion.identity);
        gameObject.ChangeStatus(Turret.TurretStatus.Selected);

        return gameObject;
    }

    void CheckTurretCollisions() {
        Vector3 turretPos = currentTurret.transform.position;

        var layerMask = 1 << turretsMask;
        Collider[] hitColliders = Physics.OverlapSphere(turretPos, collisionRadius, layerMask);

        string str = "";
        foreach (Collider collider in hitColliders) {
            str += collider.name + " ";
        }

        Debug.Log("Colliding with " + str);

        if (hitColliders.Length != 0)
            currentTurret.ChangeStatus(Turret.TurretStatus.Colliding);
        else
            currentTurret.ChangeStatus(Turret.TurretStatus.Selected);
    }
}
