using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : Purchasable {

    public float force;
    public int cost;
    public int radius;
    public int damage;
    public GameObject missileImpactEffect;

    private GameController gameController;
    private int enemyMask;

    void Start() {
        GetComponent<Rigidbody>().AddForce(Vector3.down * force);
        gameController = FindObjectOfType<GameController>();

        enemyMask = LayerMask.GetMask("Enemy");
    }

    void Update() {
        float x = (float) Math.Round(Input.acceleration.x, 2);
        float y = (float) Math.Round(Input.acceleration.y, 2);

        Vector3 direction = new Vector3(x, 0f, -y);
        transform.Translate(direction * Time.deltaTime * 10f);
    }

    void OnCollisionEnter(Collision collision) {
        GameObject effect = Instantiate(missileImpactEffect, transform.position, Quaternion.identity);
        Destroy(effect, 2f);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, enemyMask);

        foreach (Collider collider in colliders) {
            Enemy enemy = collider.GetComponent<Enemy>();
            enemy.TakeDamage(damage);
        }

        gameController.ToggleDrawer();
        Destroy(gameObject);
    }

    public override int getCost() {
        return cost;
    }
}
