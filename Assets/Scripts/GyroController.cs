using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroController {
    Matrix4x4 baseMatrix = Matrix4x4.identity;
    bool gyroAvailable;

    public GyroController() {
        gyroAvailable = SystemInfo.supportsGyroscope;

        Vector3 acc;
        if (gyroAvailable) {
            if (!Input.gyro.enabled) Input.gyro.enabled = true;
            acc = Input.gyro.gravity;
        } else {
            // Fallback to use the accelerometer
            acc = Input.acceleration;
        }

        Quaternion rotate = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, -1.0f), acc);
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotate, new Vector3(1.0f, 1.0f, 1.0f));
        baseMatrix = matrix.inverse;
    }

    public Vector3 GetRelativeAcceleration() {
        return baseMatrix.MultiplyVector(GetAcceleration());
    }

    public Vector3 GetAcceleration() {
        if (gyroAvailable)
            return Input.gyro.gravity;
        else
            return Input.acceleration;
    }
}
