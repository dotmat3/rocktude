using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreItem : MonoBehaviour {

    public Text position;
    public Text username;
    public Text score;

    public void SetValue(int index, string username, int score) {
        position.text = index.ToString();
        this.username.text = username;
        this.score.text = score.ToString();
    }
}
