using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Enemy : MonoBehaviour {

    public EndOfPathInstruction endOfPathInstruction;
    public float speed = 5f;
    public int health = 1;
    public GameObject impactEffect;

    protected float distanceTravelled;

    private GameController gameController;
    private WaveController waveController;
    private LeaderboardController leaderboardController;
    private PathCreator pathCreator;

    private float height;
    private Vector3 lastPoint;

    private void Start() {
        gameController = FindObjectOfType<GameController>();
        waveController = FindObjectOfType<WaveController>();
        leaderboardController = FindObjectOfType<LeaderboardController>();

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
        }
    }

    public Vector3 GetPositionAtDistance(float distance) {
        Vector3 position = pathCreator.path.GetPointAtDistance(distance, endOfPathInstruction);
        position.y += height;
        return position;
    }

    public void TakeDamage(int amount) {
        health -= amount;

        if (health <= 0) {
            // The enemy was destroyed by a turret
            GameObject effect = Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);

            leaderboardController.UpdateEnemyKilled(1);

            waveController.EnemyRemoved();
            Destroy(gameObject);
        }
    }

    public float GetDistanceTravelled() => distanceTravelled;
}
