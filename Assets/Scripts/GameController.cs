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

    void Start() {
        UpdateMoney(startMoney);
        UpdateLives(startLives);
        UpdateRounds();
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) SceneManager.LoadScene(1);
    }

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

    public void Victory() {
        victoryText.gameObject.SetActive(true);
        gameStatus = GameStatus.VICTORY;
    }

    public void Defeat() {
        gameOverText.gameObject.SetActive(true);
        gameStatus = GameStatus.GAME_OVER;
    }

    public void AddOnMoneyUpdate(Action action) => onMoneyUpdate.Add(action);

    public void SpawnMissile() {
        if (gameStatus != GameStatus.IDLE)
            return;

        Instantiate(missile, missileSpawnPosition.position, missile.transform.rotation);

        UpdateMoney(money - missile.getCost());

        ToggleDrawer();
    }

    public void ToggleDrawer() {
        drawer.SetActive(!drawer.activeSelf);
    }

    public GameStatus GetGameStatus() {
        return gameStatus;
    }
}
