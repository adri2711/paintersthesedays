using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class PaintingController : MonoBehaviour
{
    public static Shader defaultShader;
    List<Paint> _paints = new List<Paint>();
    Paint _currentPaint;

    private GameObject _characterSignalsInterfaceTarget;
    private FirstPersonController _firstPersonController;
    private Camera _camera;

    public int iterations = 3;
    public float sizeIncrease = 5f;

    void Start()
    {
        defaultShader = Resources.Load<Shader>("Shaders/DefaultShader");
        _characterSignalsInterfaceTarget = transform.parent.parent.gameObject;
        _firstPersonController = _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>();
        _camera = GetComponent<Camera>();

        _currentPaint = Paint.CombinePaint(new Paint(Color.cyan), new Paint(Color.magenta));
    }

    void Update()
    {
        if (_firstPersonController.currentActiveCanvas != null)
        {
            if (Input.GetMouseButton(0))
            {
                HashSet<int> brushTris = new HashSet<int>();

                brushTris.Add(SelectTriangle(Input.mousePosition));
                for (int i = 0; i < iterations; i++)
                {
                    int points = (int)((iterations - 1) * Mathf.Pow(2, i + 1));
                    for (int j = 0; j < points; j++)
                    {
                        float angle = j * 360f / points;
                        Vector3 brushOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * sizeIncrease * i;
                        brushTris.Add(SelectTriangle(Input.mousePosition + brushOffset));
                    }
                }

                foreach (int t in brushTris)
                {
                    if (t >= 0)
                    {
                        _firstPersonController.currentActiveCanvas.SetTriangleMaterial(t, _currentPaint.GetMaterial());
                    }
                }
            }
        }
    }

    private int SelectTriangle(Vector3 originPos)
    {
        RaycastHit hit;
        if (!Physics.Raycast(_camera.ScreenPointToRay(originPos), out hit))
        {
            return -1;
        }

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
        {
            return -1;
        }

        Mesh mesh = meshCollider.sharedMesh;
        int[] triangles = mesh.triangles;
        return hit.triangleIndex;
    }
}
