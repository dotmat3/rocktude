using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class CameraFeedController : MonoBehaviour {

    public RawImage showImage;

    private WebCamTexture camTexture;
    private int width;
    private int height;
    private Color32[] data;
    private Color32[] croppedData;
    private Texture2D croppedTexture;
    private IBarcodeReader barcodeReader;

    IEnumerator Start() {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam)) {
            width = (int)showImage.rectTransform.rect.width;
            height = (int)showImage.rectTransform.rect.height;

            camTexture = new WebCamTexture(null, 1920, 1080);
            if (camTexture != null) {
                camTexture.Play();
                data = new Color32[camTexture.width * camTexture.height];
                croppedData = new Color32[width * height];
                croppedTexture = new Texture2D(width, height);

                barcodeReader = new BarcodeReader();
                InvokeRepeating("ReadQR", 0, 0.5f);
            }
        }
    }

    void ReadQR() { 
        try {
            Result result = barcodeReader.Decode(croppedTexture.GetPixels32(), width, height);
            if (result != null)
                Debug.Log("Result: " + result.Text);
        }
        catch (Exception ex) { Debug.LogWarning(ex.Message); }
    }

    void Update() {
        if (data != null) {
            // Crop the camera frame
            Color[] cropped = camTexture.GetPixels((camTexture.width - width) / 2, (camTexture.height - height) / 2, width, height);
            croppedTexture.SetPixels(cropped);
            croppedTexture.Apply();

            // Draw the cropped camera frame on screen
            showImage.texture = croppedTexture;
        }
    }
}
