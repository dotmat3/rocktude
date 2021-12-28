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

    public AudioClip impactSound;

    private GameController gameController;
    private MultiplayerController multiplayerController;
    private Rigidbody rb;

    private int enemyMask;
    private Vector3 forward;
    private bool active = true;

    void Start() {
        gameController = FindObjectOfType<GameController>();
        multiplayerController = MultiplayerController.DefaultInstance;
        rb = GetComponent<Rigidbody>();

        GyroController.Calibrate();

        enemyMask = LayerMask.GetMask("Enemy");

        if (active)
            InvokeRepeating("SendUpdate", 0.02f, 0.02f);
    }

    void FixedUpdate() {
        if (active) {
            Vector3 acc = GyroController.GetRelativeAcceleration();
            forward = new Vector3(-acc.x, -1f, -acc.y);

            Vector3 direction = new Vector3(8f * forward.x, 4f * forward.y, 4f * forward.z);
            transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            rb.velocity = direction * speed * Time.deltaTime / Time.timeScale;
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
        AudioController.PlayOneShot(impactSound, 2);

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
