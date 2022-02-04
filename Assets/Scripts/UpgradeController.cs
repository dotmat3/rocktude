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
    public GameObject upgradeDrawer;

    public AudioClip sellSound;
    public AudioClip upgradeSound;

    private TurretSelector turretSelector;
    private MultiplayerController multiplayerController;

    private Turret selectedTurret;
    private NextUpgradeInfo info;

    void Start() {
        turretSelector = FindObjectOfType<TurretSelector>();
        multiplayerController = MultiplayerController.DefaultInstance;

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
        upgradeDrawer.SetActive(true);
        turretsDrawer.SetActive(false);
    }

    public void Hide() {
        upgradeDrawer.SetActive(false);
        turretsDrawer.SetActive(true);
    }

    public void SellSelectedTurret() {
        SellTurret(selectedTurret);

        multiplayerController.SellTurret(selectedTurret);

        gameController.UpdateMoney(gameController.money + selectedTurret.sellValue);
        Hide();
    }

    public void SellTurret(string index) {
        SellTurret(gameController.GetTurret(index));
    }

    public void SellTurret(Turret turret) {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;

        Destroy(turret.gameObject);
        gameController.RemoveTurret(turret);

        AudioController.PlayOneShot(sellSound, 2);
    }

    public void UpgradeSelectedTurret() {
        gameController.UpdateMoney(gameController.money - info.cost);
        
        Turret newTurret = UpgradeTurret(selectedTurret);

        multiplayerController.UpgradeTurret(selectedTurret);

        turretSelector.OnTurretSelected(newTurret);
    }

    public Turret UpgradeTurret(string index) {
        return UpgradeTurret(gameController.GetTurret(index));
    }

    public Turret UpgradeTurret(Turret turret) {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return null;

        Transform prevTransform = turret.transform;
        Transform cannonTransform = turret.cannon.transform;
        NextUpgradeInfo info = turret.GetComponent<NextUpgradeInfo>();
        Destroy(turret.gameObject);

        Turret newTurret = Instantiate(info.nextUpgrade, prevTransform.position, prevTransform.rotation);
        newTurret.index = turret.index;
        newTurret.playerId = turret.playerId;
        newTurret.cannon.transform.rotation = cannonTransform.rotation;

        gameController.StoreTurret(newTurret);

        newTurret.Activate();

        AudioController.PlayOneShot(upgradeSound, 2);

        return newTurret;
    }
}
