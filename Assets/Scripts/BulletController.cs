using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    private GameController gameController;

    void Start() {
        gameController = FindObjectOfType<GameController>();
    }

    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "Enemy") {
            Enemy enemy = collider.GetComponent<Enemy>();
            enemy.TakeDamage(1);

            gameController.UpdateMoney(gameController.money + enemy.health);
        }

        Destroy(gameObject);
    }

    void OnBecameInvisible() {
        Destroy(gameObject);
    }
}
