using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using UnityEngine.Android;

public class CameraFeedController : MonoBehaviour {

    public RawImage showImage;

    private WebCamTexture camTexture;
    private int width;
    private int height;
    private Color32[] data;
    private Texture2D croppedTexture;
    private IBarcodeReader barcodeReader;

    private void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName) {
        // TODO...
    }

    private void PermissionCallbacks_PermissionGranted(string permissionName) {
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

    private void PermissionCallbacks_PermissionDenied(string permissionName) {
        // TODO...
    }

    void Start() {
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
        callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
        callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
        Permission.RequestUserPermission(Permission.Camera, callbacks);
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
        }
    }
}
