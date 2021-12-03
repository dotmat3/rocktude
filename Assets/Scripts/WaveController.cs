using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class WaveController : MonoBehaviour {

    public Transform spawnPoint;
    public List<Enemy> enemies;
    public Enemy boss;

    private GameController gameController;

    private int enemiesCurrentRound = 0;
    private int numberEnemiesToSpawn;
    private int enemyIndex = 0;

    void Start() {
        gameController = FindObjectOfType<GameController>();
    }

    void SpawnEnemy() {
        Enemy enemy = enemies[enemyIndex];
        Enemy newEnemy = Instantiate(enemy, spawnPoint.position, Quaternion.identity);
        newEnemy.transform.forward = spawnPoint.forward;

        numberEnemiesToSpawn--;

        if (numberEnemiesToSpawn == 0)
            CancelInvoke();
    }

    void SpawnBoss() {
        Enemy newBoss = Instantiate(boss, spawnPoint.position, Quaternion.identity);
        newBoss.transform.forward = spawnPoint.forward;
    }

    public void EnemyRemoved() {
        enemiesCurrentRound--;

        if (enemiesCurrentRound == 0)
            EndRound();
    }

    public void EndRound() {
        if (gameController.round == 41 && gameController.lives > 0)
            gameController.Victory();

        gameController.round++;
        gameController.UpdateRounds();
    }

    public void StartRound() {
        if (enemiesCurrentRound != 0) return;

        int round = gameController.round - 1;

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
}
