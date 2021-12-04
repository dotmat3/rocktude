using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelector : MonoBehaviour {

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) GoBack();
    }

    public void GoBack() {
        SceneManager.LoadScene(0);
    }

    public void SelectMap(int index) {
        SceneManager.LoadScene(index);
    }
}
