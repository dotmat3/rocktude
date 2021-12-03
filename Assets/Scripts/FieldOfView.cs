using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour {

    [Range(0, 1)]
    public float meshResolution;

    public MeshFilter viewMeshFilter;
    public bool debug = false;

    private bool show = false;
    private float radius;
    private int obstaclesMask;
    private Mesh viewMesh;

    void Start() {
        radius = GetComponent<Turret>().radius;
        obstaclesMask = LayerMask.GetMask("Object");

        viewMesh = new Mesh {
            name = "View Mesh"
        };
        viewMeshFilter.mesh = viewMesh;
    }

    void LateUpdate() {
        if (show)
            DrawFieldOfView();
    }

    void DrawFieldOfView() {
        int stepCount = Mathf.RoundToInt(360 * meshResolution);
        float stepAngleSize = 360f / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= stepCount; i++) {
            float angle = transform.eulerAngles.y - 360 / 2 + stepAngleSize * i;

            if (debug) {
                Vector3 dir = DirFromAngle(angle);
                Debug.DrawLine(transform.position, transform.position + dir * radius, Color.red);
            }

            ViewCastInfo newViewCast = ViewCast(angle);
            viewPoints.Add(newViewCast.point);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount-1; i++) {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2) {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    ViewCastInfo ViewCast(float angle) {
        Vector3 dir = DirFromAngle(angle);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, radius, obstaclesMask))
            return new ViewCastInfo(true, hit.point, hit.distance, angle);
        else
            return new ViewCastInfo(false, transform.position + dir * radius, radius, angle);
    }

    public Vector3 DirFromAngle(float angle) {
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    public void Show() {
        show = true;
        viewMeshFilter.gameObject.SetActive(true);
    }

    public void Hide() {
        show = false;
        viewMeshFilter.gameObject.SetActive(false);
    }

    struct ViewCastInfo {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle) {
            this.hit = hit;
            this.point = point;
            this.distance = distance;
            this.angle = angle;
        }
    }

}
