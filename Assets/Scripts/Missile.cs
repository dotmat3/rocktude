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

    private bool exploded = false;

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
            float accx = 0, accz = 0;

            if (Application.platform == RuntimePlatform.Android) {
                Vector3 acc = GyroController.GetRelativeAcceleration();
                accx = acc.x;
                accz = acc.y;
            } else {
                if (Input.GetKey(KeyCode.A)) accx = 0.5f;
                if (Input.GetKey(KeyCode.D)) accx = -0.5f;
                if (Input.GetKey(KeyCode.S)) accz = 0.5f;
                if (Input.GetKey(KeyCode.W)) accz = -0.5f;
            }

            forward = new Vector3(-accx, -1f, -accz);

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
        if (exploded) return;
        
        exploded = true;
        StartCoroutine(HandleCollision());
    }

    private IEnumerator HandleCollision() {
        GetComponentInChildren<MeshRenderer>().enabled = false;
        AudioController.PlayOneShot(impactSound, 2);

        Instantiate(missileImpactEffect, transform.position, Quaternion.identity);

        for (int i = 1; i < radius; i++) {
            Collider[] colliders = Physics.OverlapSphere(transform.position, i, enemyMask);

            foreach (Collider collider in colliders) {
                Enemy enemy = collider.GetComponent<Enemy>();
                enemy.TakeDamage(damage);
            }

            yield return new WaitForSeconds(0.1f);
        }

        gameController.ShowDrawer();
        Destroy(gameObject);
    }

    public override int getCost() {
        return cost;
    }
}
