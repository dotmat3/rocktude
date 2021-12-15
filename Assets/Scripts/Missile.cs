using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : Purchasable {

    public float force;
    public int cost;
    public int radius;
    public int damage;
    public float speed = 50f;
    public GameObject missileImpactEffect;

    private GameController gameController;
    private MultiplayerController multiplayerController;

    private int enemyMask;
    private Vector3 forward;
    private bool active = true;

    void Start() {
        gameController = FindObjectOfType<GameController>();
        multiplayerController = MultiplayerController.DefaultInstance;

        enemyMask = LayerMask.GetMask("Enemy");

        if (active)
            InvokeRepeating("SendUpdate", 0.02f, 0.02f);
    }

    void FixedUpdate() {
        if (active) {
            float x = Input.acceleration.x;
            float y = Input.acceleration.y;

            forward = new Vector3(-x, -1f, -y);

            Vector3 direction = new Vector3(8f * forward.x, 4f * forward.y, 4f * forward.z);
            transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            GetComponent<Rigidbody>().velocity = direction * speed * Time.unscaledDeltaTime;
        }
    }

    public void Activate() {
        active = true;
    }

    public void Deactivate() {
        active = false;
    }

    void SendUpdate() {
        multiplayerController.UpdateMissile(this);
    }

    void OnCollisionEnter(Collision collision) {
        GameObject effect = Instantiate(missileImpactEffect, transform.position, Quaternion.identity);
        Destroy(effect, 2f);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, enemyMask);

        foreach (Collider collider in colliders) {
            Enemy enemy = collider.GetComponent<Enemy>();
            enemy.TakeDamage(damage);
        }

        gameController.ShowDrawer();
        Destroy(gameObject);
    }

    public override int getCost() {
        return cost;
    }
}
