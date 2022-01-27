using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBulletController : BulletController {

    public ParticleSystem explosionEffect;

    protected override void OnTriggerEnter(Collider collider) {
        if (gameController.GetGameStatus() != GameStatus.IDLE)
            return;


        if (collider.gameObject.tag == "Enemy") {
            RocketTurret rocketTurret = turret as RocketTurret;
            ParticleSystem.ShapeModule shape = explosionEffect.shape;
            shape.radius = rocketTurret.area;
            ParticleSystem explosion = Instantiate(explosionEffect, transform.position, explosionEffect.transform.rotation);

            int enemyMask = LayerMask.GetMask("Enemy");
            Collider[] hittedEnemies = Physics.OverlapSphere(transform.position, rocketTurret.area, enemyMask);
            foreach (Collider enemyCollider in hittedEnemies) { 
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                enemy.TakeDamage(turret.damage);
                leaderboardController.UpdateCollectedMoney(turret.damage);
                gameController.UpdateMoney(gameController.money + turret.damage);
            }
        }

        Destroy(gameObject);
    }
}
