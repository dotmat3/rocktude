using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GyroController {
    private static Matrix4x4 baseMatrix = Matrix4x4.identity;
    private static bool gyroAvailable;
    private static bool gyroEnabled;

    public static Vector3 GetRelativeAcceleration() {
        CheckGyroEnabled();

        return baseMatrix.MultiplyVector(GetAcceleration());
    }

    public static Vector3 GetAcceleration() {
        CheckGyroEnabled();

        if (gyroAvailable)
            return Input.gyro.gravity;
        else
            return Input.acceleration;
    }

    public static void Calibrate() {
        CheckGyroEnabled();

        Vector3 acc;
        if (gyroAvailable) {
            acc = Input.gyro.gravity;
        }
        else {
            // Fallback to use the accelerometer
            acc = Input.acceleration;
        }

        Quaternion rotate = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, -1.0f), acc);
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotate, new Vector3(1.0f, 1.0f, 1.0f));
        baseMatrix = matrix.inverse;
    }

    public static void EnableGyro() {
        gyroAvailable = SystemInfo.supportsGyroscope;
        if (gyroAvailable)
            Input.gyro.enabled = true;
        gyroEnabled = true;
    }

    private static void CheckGyroEnabled() {
        if (!gyroEnabled) throw new System.Exception("First you need to enable the gyroscope!");
    }
}
