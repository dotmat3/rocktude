using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UpdateLeaderboardInfo {
    public string username;
    public string userId;
    public int score;
}

public class LeaderboardController : MonoBehaviour {
    
    public const string SERVER_ADDRESS = "http://skylion.zapto.org";
    public const int SERVER_PORT = 2043;

    public void UpdateCollectedMoney(int score) => SendUpdate("money", score);

    public void UpdateEnemyKilled(int score) => SendUpdate("enemy", score);

    private void SendUpdate(string type, int score) {
        string url = $"{SERVER_ADDRESS}:{SERVER_PORT}/scores/{type}";

        UserController userController = UserController.DefaultInstance;

        UpdateLeaderboardInfo info = new UpdateLeaderboardInfo {
            username = userController.GetUsername(),
            userId = userController.GetUserId(),
            score = score
        };

        string data = JsonUtility.ToJson(info);

        StartCoroutine(NetworkController.SendPutRequest(url, data));
    }
}
