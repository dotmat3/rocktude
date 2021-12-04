using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Turret : MonoBehaviour {

    public enum TurretStatus {
        Idle,
        Selected,
        Colliding
    }

    private bool active = false;
    private int collisionCounter = 0;

    public GameObject cannon;
    public int sellValue;

    [Header("Shooting")]
    public GameObject bullet;
    public float radius = 10f;
    public float shootForce = 10f;
    public float damage = 1f;
    public float timeBetweenShooting = 2f;

    public Transform attackPoint;
    public ParticleSystem shootParticles;

    private bool readyToShoot;
    private int enemiesMask, obstaclesMask;
    private TurretStatus currentStatus;

    void Awake() {
        readyToShoot = true;

        ChangeStatus(TurretStatus.Idle);
    }

    void Start() {
        enemiesMask = LayerMask.GetMask("Enemy");
        obstaclesMask = LayerMask.GetMask("Object");
    }

    void Update() {
        if (!active) return;

        // Find nearest enemy
        GameObject enemy = FindNearestEnemy();
        if (!enemy) return;

        // Rotate towards the enemy
        Vector3 enemyPos = enemy.transform.position;
        Vector3 forward = enemyPos - cannon.transform.position;
        cannon.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        // Shoot when mouse clicked
        if (readyToShoot)
            Shoot(enemy);
    }

    GameObject FindNearestEnemy() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, enemiesMask);

        float minimumDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        RaycastHit hit;
        foreach (Collider collider in hitColliders) {

            Vector3 direction = (collider.transform.position - transform.position).normalized;

            // If the enemy is behind an obstacle, ignore it
            if (Physics.Raycast(transform.position, direction, out hit, radius, obstaclesMask))
                continue;
               
            Debug.DrawLine(transform.position, transform.position + direction * radius, Color.red);

            // Compute the distance to the enemy to obtain the nearest one
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < minimumDistance) {
                minimumDistance = distance;
                nearestEnemy = collider.gameObject;
            }
        }

        return nearestEnemy;
    }

    public void Shoot(GameObject enemy) {
        readyToShoot = false;

        Vector3 enemyPos = enemy.transform.position;
        Vector3 direction = (enemyPos - attackPoint.position).normalized;

        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
        currentBullet.transform.forward = direction;

        currentBullet.GetComponent<Rigidbody>().AddForce(direction * shootForce, ForceMode.Impulse);

        shootParticles.Play();

        Invoke("ResetShot", timeBetweenShooting);
    }

    private void ResetShot() {
        readyToShoot = true;
    }

    public void Activate() {
        active = true;
    }

    public void Deactivate() {
        active = false;
    }

    public void ChangeStatus(TurretStatus status) {
        currentStatus = status;

        switch (status) {
            case TurretStatus.Selected:
                GetComponent<FieldOfView>().Show();
                break;
            case TurretStatus.Colliding:
                GetComponent<FieldOfView>().Colliding();
                break;
            case TurretStatus.Idle:
                GetComponent<FieldOfView>().Hide();
                break;
        }
    }

    public TurretStatus getCurrentStatus() {
        return currentStatus;
    }

    private void OnTriggerEnter(Collider collider) {
        if (currentStatus == TurretStatus.Idle) return;

        collisionCounter++;
        ChangeStatus(TurretStatus.Colliding);
    }

    private void OnTriggerExit(Collider collider) {
        if (currentStatus == TurretStatus.Idle) return;

        collisionCounter--;
        if (collisionCounter == 0)
            ChangeStatus(TurretStatus.Selected);
    }
}