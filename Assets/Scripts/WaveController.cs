using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour {

    public Transform spawnPoint;
    public List<Enemy> enemies;
    public Enemy boss;
    public StartButtonBehaviour startButton;

    private GameController gameController;
    private MalusController malusController;

    private int enemiesCurrentRound = 0;
    private int numberEnemiesToSpawn;
    private int enemyIndex = 0;

    private bool autoStartRound = false;

    void Start() {
        gameController = FindObjectOfType<GameController>();
        malusController = GetComponent<MalusController>();
    }

    void SpawnEnemy() {
        Enemy enemy = enemies[enemyIndex];
        Enemy newEnemy = Instantiate(enemy, spawnPoint.position, Quaternion.identity);
        newEnemy.transform.forward = spawnPoint.forward;

        numberEnemiesToSpawn--;

        if (numberEnemiesToSpawn == 0) {
            CancelInvoke();
            malusController.CheckMalus();
        }
    }

    void SpawnBoss() {
        Enemy newBoss = Instantiate(boss, spawnPoint.position, Quaternion.identity);
        newBoss.transform.forward = spawnPoint.forward;
    }

    public void EnemyRemoved() {
        enemiesCurrentRound--;

        if (enemiesCurrentRound <= 0)
            EndRound();
    }

    public void EndRound() {
        if (gameController.round == 41 && gameController.lives > 0)
            gameController.Victory();

        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;

        gameController.round++;
        gameController.UpdateRounds();

        if (autoStartRound) StartRound();
        else startButton.ShowStartRound();
    }

    public void StartRound() {
        if (enemiesCurrentRound > 0) return;

        int round = gameController.round - 1;

        malusController.OnStartRound();

        if (round == 40) {
            enemiesCurrentRound = 1;
            numberEnemiesToSpawn = 1;
            SpawnBoss();
        } else {
            enemyIndex = round / 10;
            numberEnemiesToSpawn = 10 + (5 * (round % 10));
            enemiesCurrentRound = numberEnemiesToSpawn;

            float interval = 1f - (.1f * (round % 10));
            InvokeRepeating("SpawnEnemy", interval, interval);
        }
    }

    public Enemy SpawnPreviousTier(Enemy enemy) {
        Enemy prefab = enemies[enemy.health - 1];
        Enemy newEnemy = Instantiate(prefab, enemy.transform.position, enemy.transform.rotation);
        newEnemy.SetDistanceTravelled(enemy.GetDistanceTravelled());
        return newEnemy;
    }

    public void EnableAutoRound() => autoStartRound = true;
    public void DisableAutoRound() => autoStartRound = false;
}
