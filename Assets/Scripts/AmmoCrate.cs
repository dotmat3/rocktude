using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCrate : MonoBehaviour {
    private MalusController malusController;
    private MultiplayerController multiplayerController;

    public float horizontalSpeed = 10f;
    public float verticalSpeed = 10f;
    public float gravitySpeed = 10f;

    void Start() {
        malusController = FindObjectOfType<MalusController>();
        multiplayerController = MultiplayerController.DefaultInstance;
    }

    void Update() {
        float accx = 0, accy = 0, accz = 0;
        
        if (Application.platform == RuntimePlatform.Android) { 
            Vector3 acc = Input.acceleration;
            accx = acc.x;
            accz = acc.y;
        } else {
            if (Input.GetKey(KeyCode.A)) accx = -0.5f;
            if (Input.GetKey(KeyCode.D)) accx = 0.5f;
            if (Input.GetKey(KeyCode.S)) accz = -0.5f;
            if (Input.GetKey(KeyCode.W)) accz = 0.5f;
        }
        
        accx *= horizontalSpeed * Time.unscaledDeltaTime;
        accy = -gravitySpeed * Time.unscaledDeltaTime;
        accz *= verticalSpeed * Time.unscaledDeltaTime;

        Vector3 delta = new Vector3(accx, accy, accz);
        gameObject.transform.Translate(delta);
    }

    private void OnCollisionEnter(Collision collision) {
        // Colliding with the terrain
        Destroy(gameObject);
        malusController.SpawnAmmo();
    }

    private void OnTriggerEnter(Collider collider) {
        // Colliding with a turret
        Destroy(gameObject);

        Turret turret = collider.gameObject.GetComponent<Turret>();
        if (!turret.IsActive()) {
            malusController.EnableTurret(turret);
            multiplayerController.DisableMalus(turret.GetIdentifier());
        }
        malusController.SpawnAmmo();
    }
}
