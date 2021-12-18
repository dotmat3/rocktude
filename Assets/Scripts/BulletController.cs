using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    private GameController gameController;
    private LeaderboardController leaderboardController;

    void Start() {
        gameController = FindObjectOfType<GameController>();
        leaderboardController = FindObjectOfType<LeaderboardController>();
    }

    void OnTriggerEnter(Collider collider) {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;

        Destroy(gameObject);

        if (collider.gameObject.tag == "Enemy") {
            Enemy enemy = collider.GetComponent<Enemy>();
            enemy.TakeDamage(1);

            leaderboardController.UpdateCollectedMoney(1);

            gameController.UpdateMoney(gameController.money + 1);
        }
    }

    void OnBecameInvisible() {
        Destroy(gameObject);
    }
}
