using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MalusController : MonoBehaviour {
    public GameObject malusMessage;
    public GameObject ammoPrefab;
    public AudioClip outOfAmmoSound;

    public float MALUS_PROB = 0.2f;
    public int N_TURRETS = 2;
    public float AMMO_INTERVAL_S = 1f;

    private GameController gameController;
    private MultiplayerController multiplayerController;
    
    private bool startMalus = false;
    private HashSet<string> disabledTurrets = new HashSet<string>();

    void Start() {
        gameController = FindObjectOfType<GameController>();
        multiplayerController = MultiplayerController.DefaultInstance;
    }

    public void CheckMalus() {
        if (gameController.GetGameStatus() != GameStatus.IDLE) return;

        int nTurrets = gameController.GetTurrets().Count;
        if (nTurrets == 0) return;

        float p = Random.value;
        if (p < MALUS_PROB && disabledTurrets.Count == 0) {
            ShowMessage();
            startMalus = true;
        }
    }

    public void ShowMessage() {
        malusMessage.SetActive(true);
        Invoke("HideMessage", 5f * Time.timeScale);
    }

    public void HideMessage() {
        malusMessage.SetActive(false);
    }

    public void OnStartRound() {
        if (!startMalus || disabledTurrets.Count > 0) return;
        StartMalus();
    }

    public void StartMalus() {
        startMalus = false;
        Dictionary<string, Turret> currentTurrets = gameController.GetTurrets();
        List<string> turretsIds = GenerateRandomKeys(currentTurrets, N_TURRETS);
        DisableTurrets(turretsIds);
        GyroController.Calibrate();
        SpawnAmmo();
        multiplayerController.EnableMalus(turretsIds);
    }

    private List<T> GenerateRandomKeys<T, U>(Dictionary<T, U> dict, int n) {
        List<T> keysList = new List<T>(dict.Keys);

        List<T> chosenKeys = new List<T>();
        while (chosenKeys.Count < Math.Min(dict.Count, n)) {
            int randomIdx = (int)Math.Floor(Random.value * dict.Count);
            T key = keysList[randomIdx];
            if (chosenKeys.Contains(key)) continue;

            chosenKeys.Add(key);
        }
        return chosenKeys;
    }

    public void DisableTurrets(List<string> turretsIds) {
        AudioController.PlayOneShot(outOfAmmoSound, 2);
        Dictionary<string, Turret> turrets = gameController.GetTurrets();
        foreach (string id in turretsIds) {
            Turret turret = turrets[id];
            turret.ActivateMalus();
            disabledTurrets.Add(id);
        }
    }

    public void EnableTurret(string turretId) {
        Turret turret = gameController.GetTurret(turretId);
        EnableTurret(turret);
    }

    public void EnableTurret(Turret turret) {
        turret.DisableMalus();
        disabledTurrets.Remove(turret.GetIdentifier());
    }

    public void SpawnAmmo() {
       Instantiate(ammoPrefab);
    }

    public void OnAmmoCrateHit() {
        if (disabledTurrets.Count != 0)
            Invoke("SpawnAmmo", AMMO_INTERVAL_S * Time.timeScale);
    }
}
