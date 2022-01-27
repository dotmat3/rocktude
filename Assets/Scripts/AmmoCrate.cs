using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCrate : MonoBehaviour {
    public float horizontalSpeed = 10f;
    public float verticalSpeed = 10f;
    public float gravitySpeed = 10f;

    private MalusController malusController;
    private MultiplayerController multiplayerController;
    private Rigidbody rb;

    private bool hitSomething;

    void Start() {
        multiplayerController = MultiplayerController.DefaultInstance;
        rb = GetComponent<Rigidbody>();
        malusController = FindObjectOfType<MalusController>();
    }

    void FixedUpdate() {
        float accx = 0, accy = 0, accz = 0;
        
        if (Application.platform == RuntimePlatform.Android) {
            Vector3 acc = GyroController.GetRelativeAcceleration();
            accx = acc.x;
            accz = acc.y;
        } else {
            if (Input.GetKey(KeyCode.A)) accx = -0.5f;
            if (Input.GetKey(KeyCode.D)) accx = 0.5f;
            if (Input.GetKey(KeyCode.S)) accz = -0.5f;
            if (Input.GetKey(KeyCode.W)) accz = 0.5f;
        }
        
        accx *= horizontalSpeed * Time.deltaTime / Time.timeScale;
        accy = -gravitySpeed * Time.deltaTime / Time.timeScale;
        accz *= verticalSpeed * Time.deltaTime / Time.timeScale;

        Vector3 delta = new Vector3(accx, accy, accz);
        rb.MovePosition(rb.position + delta);
    }

    void LateUpdate() {
        if (hitSomething) {
            Destroy(gameObject);
            malusController.OnAmmoCrateHit();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // Colliding with the terrain
        hitSomething = true;
    }

    private void OnTriggerEnter(Collider collider) {
        // Colliding with a turret
        hitSomething = true;

        Turret turret = collider.gameObject.GetComponent<Turret>();
        if (!turret.IsActive()) {
            malusController.EnableTurret(turret);
            multiplayerController.DisableMalus(turret.GetIdentifier());
        }
    }
}
