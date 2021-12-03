using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PathCreation;

public class GameController : MonoBehaviour {

    public PathCreator pathLevel;

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

    void Start() {
        UpdateMoney(startMoney);
        UpdateLives(startLives);
        UpdateRounds();
    }

    public void UpdateMoney(int amount) {
        money = amount;

        moneyText.text = "$" + money.ToString();
    }

    public void UpdateLives(int amount) {
        lives = amount;

        livesText.text = lives.ToString();

        if (amount == 0)
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
    }

    public void Defeat() {
        gameOverText.gameObject.SetActive(true);
    }
}
