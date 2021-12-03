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

    private int terrainMask;

    public Button buyButton;
    public Text buyText;

    private bool buying;

    private void Start() {
        networkController = gameController.GetComponent<NetworkController>();

        terrainMask = LayerMask.GetMask("Terrain");

        buyButton.onClick.AddListener(onBuyClick);

        buying = false;
    }

    void Update() {
        if (buying) {
            HandleNewTurret();

            if (currentTurret != null) {
                    FollowMouse();
                    HandlePlacement();
            }
        }
    }

    void onBuyClick() {
        if (buying) {
            buying = false;
            buyText.text = "$" + turretCost;
        } else {

            if (gameController.money < turretCost) return;

            buying = true;
            buyText.text = "Buying";
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

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainMask)) {
            currentTurret.transform.position = hit.point;
            // currentTurret.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }

    void HandlePlacement() {
        if (Input.GetMouseButtonUp(0))
            if (currentTurret.getCurrentStatus() == Turret.TurretStatus.Colliding) {
                Destroy(currentTurret.gameObject);
                currentTurret = null;

                buying = false;
                buyText.text = "$" + turretCost;
            } else {
                networkController.SendEvent(new PlaceTurretEvent(0, currentTurret.transform.position));

                currentTurret.Activate();
                currentTurret.ChangeStatus(Turret.TurretStatus.Idle);
                currentTurret = null;

                buying = false;

                buyText.text = "$" + turretCost;

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

    public bool isBuying() {
        return buying;
    }
}
