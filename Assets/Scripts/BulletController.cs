using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    protected GameController gameController;
    protected LeaderboardController leaderboardController;
    protected Turret turret;

    void Start() {
        gameController = FindObjectOfType<GameController>();
        leaderboardController = FindObjectOfType<LeaderboardController>();
    }

    protected virtual void OnTriggerEnter(Collider collider) {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;

        Destroy(gameObject);

        if (collider.gameObject.tag == "Enemy") {
            Enemy enemy = collider.GetComponent<Enemy>();
            enemy.TakeDamage(turret.damage);

            leaderboardController.UpdateCollectedMoney(turret.damage);

            gameController.UpdateMoney(gameController.money + turret.damage);
        }
    }

    void OnBecameInvisible() {
        Destroy(gameObject);
    }

    public void SetTurret(Turret turret) => this.turret = turret;
}
