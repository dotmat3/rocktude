using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowingBulletController : BulletController {

    protected override void OnTriggerEnter(Collider collider) {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;

        if (collider.gameObject.tag == "Enemy") {
            Enemy enemy = collider.GetComponent<Enemy>();

            SlowTurret st = (turret as SlowTurret);
            enemy.SlowDown(st.slowness, st.slownessDuration);

            enemy.TakeDamage(turret.damage);
            leaderboardController.UpdateCollectedMoney(turret.damage);
            gameController.UpdateMoney(gameController.money + turret.damage);
        }
        Destroy(gameObject);
    }
}
