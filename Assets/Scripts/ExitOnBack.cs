using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitOnBack : MonoBehaviour {
    
    void Start() {
        
    }

    void Update() {
        if (Application.isPlaying && Input.GetKeyUp(KeyCode.Escape)) {
            Debug.Log("Hi");
            if (Application.platform == RuntimePlatform.Android) {
                AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call<bool>("moveTaskToBack", true);
            } else {
                Application.Quit();
            }
        }
    }
}
