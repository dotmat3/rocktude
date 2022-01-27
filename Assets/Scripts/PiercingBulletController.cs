using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingBulletController : BulletController {

    private int hitCount;

    protected override void OnTriggerEnter(Collider collider) {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;

       if (collider.gameObject.tag == "Enemy") {
            Enemy enemy = collider.GetComponent<Enemy>();
            enemy.TakeDamage(turret.damage);

            leaderboardController.UpdateCollectedMoney(turret.damage);

            gameController.UpdateMoney(gameController.money + turret.damage);

            hitCount++;
            if (hitCount == (turret as PiercingTurret).piercing)
                Destroy(gameObject);

        } else {
            Destroy(gameObject);
        }
    }
}
