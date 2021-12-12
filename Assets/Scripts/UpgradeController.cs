using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeController : MonoBehaviour {

    public GameController gameController;

    public Text description;
    public Button upgradeButton;
    public Button sellButton;

    public GameObject turretsDrawer;

    private TurretSelector turretSelector;

    private Turret selectedTurret;
    private NextUpgradeInfo info;

    void Start() {
        turretSelector = FindObjectOfType<TurretSelector>();

        gameController.AddOnMoneyUpdate(UpdateUpgradeButtons);
    }

    private void UpdateUpgradeButtons() {
        if (selectedTurret && info)
            upgradeButton.interactable = gameController.money >= info.cost;
    }

    public void UpdateView(Turret turret) {
        selectedTurret = turret;
        info = selectedTurret.GetComponent<NextUpgradeInfo>();

        Text upgradeBtnText = upgradeButton.GetComponentInChildren<Text>();
        Text sellBtnText = sellButton.GetComponentInChildren<Text>();

        if (info) {
            description.text = info.description;

            upgradeBtnText.text = "upgrade\n$" + info.cost;

            UpdateUpgradeButtons();
        } else {
            description.text = "Max upgrade reached!";
            upgradeBtnText.text = "Max";

            upgradeButton.interactable = false;
        }

        sellBtnText.text = "sell\n$" + turret.sellValue;
    }

    public void Show() {
        gameObject.SetActive(true);
        turretsDrawer.SetActive(false);
    }

    public void Hide() {
        gameObject.SetActive(false);
        turretsDrawer.SetActive(true);
    }

    public void SellTurret() {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;

        Destroy(selectedTurret.gameObject);

        gameController.UpdateMoney(gameController.money + selectedTurret.sellValue);

        gameObject.SetActive(false);
        turretsDrawer.SetActive(true);
    }

    public void UpgradeTurret() {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;
        
        gameController.UpdateMoney(gameController.money - info.cost);

        Transform prevTransform = selectedTurret.transform;

        Destroy(selectedTurret.gameObject);

        Turret newTurret = Instantiate(info.nextUpgrade, prevTransform.position, prevTransform.rotation);
        newTurret.Activate();

        turretSelector.OnTurretSelected(newTurret);
    }
}
