using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Turret : Purchasable {

    public enum TurretStatus {
        Idle,
        Selected,
        Colliding
    }

    private bool active = false;
    private int collisionCounter = 0;

    public GameObject cannon;
    public int cost;
    public int sellValue;
    public int type;

    [Header("Shooting")]
    public GameObject bullet;
    public float radius = 10f;
    public float shootForce = 10f;
    public float damage = 1f;
    public float timeBetweenShooting = 2f;

    public Transform attackPoint;
    public ParticleSystem shootParticles;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    private bool readyToShoot;
    private int enemiesMask, obstaclesMask;
    private TurretStatus currentStatus;

    public ParticleSystem disabledEffectPrefab;
    private ParticleSystem disabledEffectInstance;

    [HideInInspector]
    public int index;
    [HideInInspector]
    public string playerId;

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

        if (readyToShoot) {
            Vector3? predictedEnemyPos = FindFirstEnemyPosition();
            if (!predictedEnemyPos.HasValue) return;

            // Rotate towards the predicted enemy position
            Vector3 forward = predictedEnemyPos.Value - cannon.transform.position;
            Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
            cannon.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, rotation.eulerAngles.z);

            Shoot(predictedEnemyPos.Value);
        }
    }

    public string GetIdentifier() {
        return playerId + "-" + index;
    }

    Vector3? FindFirstEnemyPosition() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, enemiesMask);

        float maximumTravelledDistance = -Mathf.Infinity;
        Vector3? firstEnemyPos = null;

        foreach (Collider collider in hitColliders) {
            Enemy enemy = collider.gameObject.GetComponent<Enemy>();
            Vector3 predictedPos = PredictEnemyPosition(enemy);

            Vector3 direction = (predictedPos - transform.position).normalized;

            // If the enemy is behind an obstacle, ignore it
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, radius, obstaclesMask)) {
                float distancePredictedPos = Vector3.Distance(predictedPos, transform.position);
                
                // Ignore the enemy if the obstacle distance is smaller than the enemy distance
                if (hit.distance < distancePredictedPos)
                    continue;
            }

            Debug.DrawLine(transform.position, transform.position + direction * radius, Color.red);
            
            float travelledDistance = enemy.GetDistanceTravelled();
            if (travelledDistance > maximumTravelledDistance) {
                maximumTravelledDistance = travelledDistance;
                firstEnemyPos = predictedPos;
            }
        }

        return firstEnemyPos;
    }

    Vector3 PredictEnemyPosition(Enemy enemy) {
        float enemyDistance = Vector3.Distance(enemy.transform.position, transform.position);
        float projectileTravelTime = enemyDistance / shootForce;
        float predictedDistance = enemy.GetDistanceTravelled() + enemy.speed * projectileTravelTime;
        Vector3 predictedEnemyPos = enemy.GetPositionAtDistance(predictedDistance);

        return predictedEnemyPos;
    }

    #region Not used
    GameObject FindNearestEnemy() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, enemiesMask);

        float minimumDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (Collider collider in hitColliders) {

            Vector3 direction = (collider.transform.position - transform.position).normalized;

            // If the enemy is behind an obstacle, ignore it
            // If we are going to use this, remember the case when an enemy is in front of an obstacle,
            // the ray is going to hit the obstacle behind and therefore not shoot to the enemy
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, radius, obstaclesMask))
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
    #endregion

    public void Shoot(Vector3 predictedPos) {
        readyToShoot = false;

        Vector3 direction = (predictedPos - attackPoint.position).normalized;

        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
        currentBullet.transform.forward = direction;

        currentBullet.GetComponent<Rigidbody>().AddForce(direction * shootForce, ForceMode.Impulse);

        shootParticles.Play();

        AudioController.PlayOneShot(shootSound, 0);

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

    public bool IsActive() => active;

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

        if (collider.gameObject.layer != LayerMask.NameToLayer("AmmoCrate"))
            ChangeStatus(TurretStatus.Colliding);
    }

    private void OnTriggerExit(Collider collider) {
        if (currentStatus == TurretStatus.Idle) return;

        collisionCounter--;
        if (collisionCounter == 0)
            ChangeStatus(TurretStatus.Selected);
    }

    public override int getCost() {
        return cost;
    }

    public void ActivateMalus() {
        if (disabledEffectInstance) return;
        disabledEffectInstance = Instantiate(disabledEffectPrefab, transform);
        Deactivate();
    }

    public void DisableMalus() {
        Destroy(disabledEffectInstance);
        disabledEffectInstance = null;
        Activate();
    }

    public void PlayReloadSound() {
        AudioController.PlayOneShot(reloadSound, 3);
    }
}