using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Firebase.Auth;

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

    private MultiplayerController multiplayerController;
    private Turret currentTurret;
    private int terrainMask;

    private bool buying = false;
    private int buyingTurretIndex = -1;
    private int turretCost = -1;
    private int numTurretsPlaced = 0;

    private FirebaseAuth auth;

    void Start() {
        multiplayerController = MultiplayerController.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

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
        if (Input.GetMouseButtonDown(0) && !GameController.IsPointerOverUI()) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit)) {
                string userId = UserController.DefaultInstance.GetUserId();
                currentTurret = PlaceTurret(buyingTurretIndex, hit.point, userId, numTurretsPlaced);
                numTurretsPlaced++;
            }
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
                multiplayerController.PlaceTurret(currentTurret);

                currentTurret.Activate();
                currentTurret.ChangeStatus(Turret.TurretStatus.Idle);
                currentTurret.PlayReloadSound();

                // Store the placed turret info
                gameController.StoreTurret(currentTurret);

                // Remove the current turret otherwise the CancelBuying method will destroy it
                currentTurret = null;

                gameController.UpdateMoney(gameController.money - turretCost);

                CancelBuying();
            }
    }

    public void OnDrawerEnter() {
        if (currentTurret != null)
            CancelBuying();
    }

    public void CancelBuying() {
        if (buyingTurretIndex == -1) return;

        if (currentTurret) {
            gameController.RemoveTurret(currentTurret);
            Destroy(currentTurret.gameObject);
            currentTurret = null;
        }

        buttons[buyingTurretIndex].GetComponentInChildren<Text>().text = "$" + turretCost;

        buyingTurretIndex = -1;
        turretCost = -1;

        buying = false;
    }

    public Turret PlaceTurret(int type, Vector3 position, string playerId, int index) {
        Turret prefab = (Turret) purchasables[type];
        
        Turret turret = Instantiate(prefab, position, Quaternion.identity);
        turret.ChangeStatus(Turret.TurretStatus.Selected);

        turret.type = type;
        turret.playerId = playerId;
        turret.index = index;

        return turret;
    }

    public bool IsBuying() {
        return buying;
    }
}
