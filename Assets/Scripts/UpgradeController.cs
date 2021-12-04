using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeController : MonoBehaviour {

    public Text description;
    public Button upgradeButton;
    public Button sellButton;

    public GameObject turretsDrawer;

    private GameController gameController;

    private Turret selectedTurret;
    private NextUpgradeInfo info;

    void Start() {
        gameController = FindObjectOfType<GameController>();
    }

    public void UpdateView(Turret turret) {
        selectedTurret = turret;
        info = selectedTurret.GetComponent<NextUpgradeInfo>();

        description.text = info.description;
        upgradeButton.GetComponentInChildren<Text>().text = "upgrade\n$" + info.cost;
        sellButton.GetComponentInChildren<Text>().text = "sell\n$" + info.sellValue;
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
        Destroy(selectedTurret.gameObject);

        gameController.UpdateMoney(gameController.money + info.sellValue);

        gameObject.SetActive(false);
        turretsDrawer.SetActive(true);
    }

    public void UpgradeTurret() {
        // TODO...
    }
}
