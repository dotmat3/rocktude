using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowingBulletController : BulletController {

    protected override void OnTriggerEnter(Collider collider) {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;

        if (collider.gameObject.tag == "Enemy") {
            Enemy enemy = collider.GetComponent<Enemy>();
            enemy.TakeDamage(turret.damage);
            leaderboardController.UpdateCollectedMoney(turret.damage);
            gameController.UpdateMoney(gameController.money + turret.damage);

            // Prevent double slowness
            if (enemy.speed == enemy.GetInitialSpeed()) {
                gameController.StartCoroutine(SlowEnemy(enemy));
            }
        }
        Destroy(gameObject);
    }

    public IEnumerator SlowEnemy(Enemy enemy) {
        // Check if the enemy has been killed in the meantime
        if (enemy == null) yield break;
        SlowTurret slowTurret = turret as SlowTurret;

        enemy.speed -= slowTurret.slowness;
        enemy.speed = Mathf.Max(1, enemy.speed);

        yield return new WaitForSeconds(slowTurret.slownessDuration);
        enemy.speed = enemy.GetInitialSpeed();
    }

}
