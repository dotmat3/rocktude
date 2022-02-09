using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Enemy : MonoBehaviour {

    public EndOfPathInstruction endOfPathInstruction;
    public float speed = 5f;
    public int health = 1;
    protected int initialHealth;
    protected float initialSpeed;
    public GameObject impactEffect;

    public AudioClip hitSound;

    protected float distanceTravelled;

    protected GameController gameController;
    protected WaveController waveController;
    protected LeaderboardController leaderboardController;
    protected PathCreator pathCreator;

    protected float height;
    protected Vector3 lastPoint;

    protected bool hit = false;

    public float beginSlowTime = -1;
    public float slowDuration;

    public void Awake() {
        gameController = FindObjectOfType<GameController>();
        waveController = FindObjectOfType<WaveController>();
        leaderboardController = FindObjectOfType<LeaderboardController>();

        initialHealth = health;
        initialSpeed = speed;
    }

    public virtual void Start() {
        height = GetComponent<MeshRenderer>().bounds.size.y;

        pathCreator = gameController.pathLevel;
        lastPoint = pathCreator.path.GetPoint(pathCreator.path.NumPoints - 1);
        lastPoint.y += height;
    }

    void Update() {
        if (pathCreator != null) {
            distanceTravelled += speed * Time.deltaTime;

            // Move the enemy along the path
            transform.position = GetPositionAtDistance(distanceTravelled);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

            if (transform.position == lastPoint) {
                // The enemy reached the end of the path
                gameController.UpdateLives(gameController.lives - health);
                waveController.EnemyRemoved();
                Destroy(gameObject);
            }

            if (beginSlowTime != -1 && (Time.time - beginSlowTime) >= (slowDuration * Time.timeScale)) {
                beginSlowTime = -1;
                speed = initialSpeed;
            }
        }
    }

    public void SlowDown(float slowness, float slowDuration) {
        beginSlowTime = Time.time;
        this.slowDuration = slowDuration;

        float newSpeed = Mathf.Max(1, initialSpeed - slowness);
        speed = newSpeed < speed ? newSpeed : speed;
    }

    public Vector3 GetPositionAtDistance(float distance) {
        Vector3 position = pathCreator.path.GetPointAtDistance(distance, endOfPathInstruction);
        position.y += height;
        return position;
    }

    public virtual void TakeDamage(int amount) {
        AudioController.PlayOneShot(hitSound, 1);

        // The enemy doesn't take damage
        if (hit || amount == 0)
            return;
        
        hit = true;

        // The enemy is already dead, stop killing him!
        if (health <= 0)
            return;

        health -= amount;

        // Spawn particle effect
        GameObject effect = Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effect, 2f);

        if (health <= 0)
            OnKill();
        else
           waveController.SpawnPreviousTier(this);

        Destroy(gameObject);
    }

    public void OnKill() {
        leaderboardController.UpdateEnemyKilled(1);
        waveController.EnemyRemoved();
    }

    public float GetDistanceTravelled() => distanceTravelled;

    public void SetDistanceTravelled(float newDist) => distanceTravelled = newDist;

    public float GetInitialSpeed() => initialSpeed;
}
