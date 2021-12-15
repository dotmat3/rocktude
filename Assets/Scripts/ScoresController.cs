using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[Serializable]
public struct ScoreEntry {
    public int score;
    public string username;
    public string userId;
}

[Serializable]
public struct Scores {
    public List<ScoreEntry> scores;
}

public class ScoresController : MonoBehaviour {

    public const string SERVER_ADDRESS = "http://skylion.zapto.org";
    public const int SERVER_PORT = 2043;

    public GameObject moneyScoresUI;
    public GameObject enemyScoresUI;

    public ScoreItem scoreItem;

    void Start() {
        LoadScores(moneyScoresUI, "money");
        LoadScores(enemyScoresUI, "enemy");
    }

    void LoadScores(GameObject scoresUI, string type) {
        string url = $"{SERVER_ADDRESS}:{SERVER_PORT}/scores/{type}";

        // Remove previously added scores 
        foreach (Transform child in scoresUI.transform)
            Destroy(child.gameObject);

        int index = 0;

        void callback(UnityWebRequest request) {
            Scores scoresStruct = JsonUtility.FromJson<Scores>(request.downloadHandler.text);

            scoresStruct.scores.Sort((a, b) => b.score.CompareTo(a.score));

            scoresStruct.scores.GetRange(0, Math.Min(scoresStruct.scores.Count, 5)).ForEach((score) => {
                ScoreItem item = Instantiate(scoreItem, scoresUI.transform);
                item.SetValue(++index, score.username, score.score);
            });
        };

        StartCoroutine(NetworkController.SendGetRequest(url, callback));
    }

    public void Back() {
        SceneManager.LoadScene(0);
    }
}
