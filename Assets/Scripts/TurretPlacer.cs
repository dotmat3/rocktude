using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[Serializable]
public abstract class Purchasable : MonoBehaviour {
    public abstract int getCost();
}

public class TurretPlacer : MonoBehaviour {

    public Camera cam;
    public GameController gameController;
    [SerializeField]
    public List<Purchasable> purchasables;
    public List<Button> buttons;

    private Turret currentTurret;
    private NetworkController networkController;
    private int terrainMask;

    private bool buying = false;
    private int buyingTurretIndex = -1;
    private int turretCost = -1;

    void Start() {
        networkController = gameController.GetComponent<NetworkController>();

        terrainMask = LayerMask.GetMask("Terrain");

        gameController.AddOnMoneyUpdate(UpdatePurchasableButtons);
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

    public void OnBuyClick(int turretIndex) {
        // Check if the game is not ended
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;

        // If the user selects the same turret
        if (buying && buyingTurretIndex == turretIndex) {
            CancelBuying();
            return;
        }

        // If the user clicks another turret while buying another one
        if (buying)
            CancelBuying();

        buyingTurretIndex = turretIndex;
        turretCost = purchasables[turretIndex].getCost();

        // If the user doesn't have enough money to buy the turret
        if (gameController.money < turretCost) {
            CancelBuying();
            return;
        }

        buying = true;
        buttons[turretIndex].GetComponentInChildren<Text>().text = "cancel";
    }

    void UpdatePurchasableButtons() {
        for (int i = 0; i < buttons.Count; i++) {
            int cost = purchasables[i].getCost();
            buttons[i].interactable = cost <= gameController.money;
        }
    }

    void HandleNewTurret() {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
                currentTurret = PlaceTurret(buyingTurretIndex, hit.point);
        }
    }

    void FollowMouse() {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainMask)) {
            currentTurret.transform.position = hit.point;
            // currentTurret.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }

    void HandlePlacement() {
        if (Input.GetMouseButtonUp(0))
            if (currentTurret.getCurrentStatus() == Turret.TurretStatus.Colliding) {
                CancelBuying();
            } else {
                networkController.SendEvent(new PlaceTurretEvent(0, currentTurret.transform.position));

                currentTurret.Activate();
                currentTurret.ChangeStatus(Turret.TurretStatus.Idle);
                // Remove the current turret otherwise the CancelBuying method will destroy it
                currentTurret = null;

                gameController.UpdateMoney(gameController.money - turretCost);

                CancelBuying();
            }
    }

    public void CancelBuying() {
        if (buyingTurretIndex == -1) return;

        if (currentTurret) {
            Destroy(currentTurret.gameObject);
            currentTurret = null;
        }

        buttons[buyingTurretIndex].GetComponentInChildren<Text>().text = "$" + turretCost;

        buyingTurretIndex = -1;
        turretCost = -1;

        buying = false;
    }

    public Turret PlaceTurret(int index, Vector3 position) {
        Turret gameObject = (Turret) purchasables[index];

        return PlaceTurret(gameObject, position);
    }

    public Turret PlaceTurret(Turret turret, Vector3 position) {
        Turret gameObject = Instantiate(turret, position, Quaternion.identity);
        gameObject.ChangeStatus(Turret.TurretStatus.Selected);

        return gameObject;
    }

    public bool IsBuying() {
        return buying;
    }
}
