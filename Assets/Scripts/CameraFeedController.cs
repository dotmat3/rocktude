using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using UnityEngine.Android;

public class CameraFeedController : MonoBehaviour {

    public RawImage showImage;
    public GameObject cameraFeedUI;
    public RoomController roomController;

    private WebCamTexture camTexture;
    private int width;
    private int height;
    private Color32[] data;
    private Texture2D croppedTexture;
    private IBarcodeReader barcodeReader;

    void OnEnable() {
        StartCoroutine("StartCamera");
    }

    IEnumerator StartCamera() {
        // Request camera permission
        if (Application.platform == RuntimePlatform.Android) {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera)) {
                Permission.RequestUserPermission(Permission.Camera);
                yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.Camera));
            }
        }
        else {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
                yield break;
        }

        width = (int) showImage.rectTransform.rect.width;
        height = (int) showImage.rectTransform.rect.height;

        camTexture = new WebCamTexture(null, 1920, 1080);
        if (camTexture != null) {
            camTexture.Play();
            data = new Color32[camTexture.width * camTexture.height];
            croppedTexture = new Texture2D(width, height);

            // Draw the cropped camera frame on screen
            showImage.texture = croppedTexture;

            barcodeReader = new BarcodeReader();
            InvokeRepeating("ReadQR", 0, 0.5f);
        }
    }

    public void Stop() {
        CancelInvoke();
        camTexture.Stop();
    }

    void ReadQR() { 
        try {
            Result result = barcodeReader.Decode(croppedTexture.GetPixels32(), width, height);
            if (result != null) {
                Stop();
                cameraFeedUI.SetActive(false);
                RoomInfo roomInfo = JsonUtility.FromJson<RoomInfo>(result.Text);
                roomController.ShowWaitingRoom(roomInfo);
            }
        }
        catch (Exception ex) { Debug.LogWarning(ex.Message); }
    }

    void Update() {
        if (camTexture != null && camTexture.isPlaying && data != null) {
            // Crop the camera frame
            Color[] cropped = camTexture.GetPixels((camTexture.width - width) / 2, (camTexture.height - height) / 2, width, height);
            croppedTexture.SetPixels(cropped);
            croppedTexture.Apply();
        }
    }
}
