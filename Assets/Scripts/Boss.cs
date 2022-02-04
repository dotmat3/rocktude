using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy {

    private Vector3 initialScale;

    public override void Start() {
        base.Start();

        initialScale = gameObject.transform.localScale;
    }

    public override void TakeDamage(int amount) {
        AudioController.PlayOneShot(hitSound, 1);
        // The enemy is already dead, stop killing him!
        if (health <= 0)
            return;

        health -= amount;

        // Spawn particle effect
        GameObject effect = Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effect, 2f);

        if (health <= 0) {
            Destroy(gameObject);
            OnKill();
        }

        // Shrink the boss size
        float newScale = Mathf.Max(0.02f, (float)health / initialHealth * initialScale.x);
        gameObject.transform.localScale = newScale * Vector3.one;
    }
}
