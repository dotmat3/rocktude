using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PathCreation;
using System;
using UnityEngine.SceneManagement;

public enum GameStatus {
    IDLE,
    VICTORY,
    GAME_OVER
}

public class GameController : MonoBehaviour {

    public PathCreator pathLevel;
    public Missile missile;
    public Transform missileSpawnPosition;

    public GameObject drawer;
    public GameObject playersUI;

    [Header("Round")]
    public int round = 1;
    public int maxRound = 40;
    public Text roundText;

    [Header("Money")]
    public int startMoney = 600;
    public Text moneyText;
    public int money;

    [Header("Lives")]
    public int startLives = 100;
    public Text livesText;
    public int lives;

    [Header("Other")]
    public Text gameOverText;
    public Text victoryText;

    private GameStatus gameStatus = GameStatus.IDLE;

    private readonly List<Action> onMoneyUpdate = new List<Action>();
    private MultiplayerController multiplayerController;

    private Dictionary<string, Turret> turrets = new Dictionary<string, Turret>();

    void Start() {
        multiplayerController = MultiplayerController.DefaultInstance;

        if (IsMultiplayer()) {
            RoomInfo? roomInfo = multiplayerController.GetRoomInfo();
            var players = multiplayerController.GetPlayers();
            playersUI.GetComponent<Text>().text = $"players\n{players.Count}/{roomInfo.Value.maxPlayers}";
            playersUI.SetActive(true);
        }

        UpdateMoney(startMoney);
        UpdateLives(startLives);
        UpdateRounds();
    }

    void Update() {
        multiplayerController.Update();

        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (IsMultiplayer()) {
                multiplayerController.Exit();
                SceneManager.LoadScene(2);
            } else
                SceneManager.LoadScene(1);
        }
    }

    #region Update UI
    public void UpdateMoney(int amount) {
        money = amount;

        moneyText.text = "$" + money.ToString();

        onMoneyUpdate.ForEach(action => action.Invoke());
    }

    public void UpdateLives(int amount) {
        lives = amount;

        livesText.text = lives.ToString();

        if (amount <= 0)
            Defeat();
    }

    public void UpdateRounds() {
        if (round < 41)
            roundText.text = "round\n" + round + "/" + maxRound;
        else
            roundText.text = "round\nboss";
    }
    #endregion

    public void Victory() {
        victoryText.gameObject.SetActive(true);
        gameStatus = GameStatus.VICTORY;
    }

    public void Defeat() {
        gameOverText.gameObject.SetActive(true);
        gameStatus = GameStatus.GAME_OVER;
    }

    public void AddOnMoneyUpdate(Action action) => onMoneyUpdate.Add(action);

    public Missile SpawnMissile() {
        if (gameStatus != GameStatus.IDLE)
            return null;

        return Instantiate(missile, missileSpawnPosition.position, missile.transform.rotation);
    }

    public void BuyMissile() {
        SpawnMissile();

        UpdateMoney(money - missile.getCost());

        multiplayerController.SpawnMissile();

        HideDrawer();
    }

    public void ShowDrawer() {
        drawer.SetActive(true);
    }

    public void HideDrawer() {
        drawer.SetActive(false);
    }

    public GameStatus GetGameStatus() {
        return gameStatus;
    }

    public bool IsMultiplayer() {
        return multiplayerController.GetRoomInfo().HasValue;
    }

    public Dictionary<string, Turret> GetTurrets() {
        return turrets;
    }

    public Turret GetTurret(string index) {
        return turrets[index];
    }

    public void StoreTurret(Turret turret) {
        turrets[turret.GetIdentifier()] = turret;
    }
}
