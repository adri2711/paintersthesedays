using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class PaintingTest : MonoBehaviour
{
    private GameObject _characterSignalsInterfaceTarget;
    private ICharacterSignals _characterSignals;
    private Camera _camera;

    void Start()
    {
        _characterSignalsInterfaceTarget = transform.parent.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (_characterSignalsInterfaceTarget.GetComponent<FirstPersonController>().currentActiveCanvas != null)
        {
            if (Input.GetMouseButton(0))
            {
                HashSet<int> brushTris = new HashSet<int>();

                int iterations = 2;
                for (int i = 0; i < iterations; i++)
                {
                    SelectTriangle(Input.mousePosition);
                    int points = (int)((iterations - 1) * Mathf.Pow(2, iterations + 1));
                    Vector3 off = new Vector2(1f, 0f);
                    for (int j = 0; j < points; j++)
                    {
                        SelectTriangle(Input.mousePosition + off);
                    }
                }

                foreach (int t in brushTris)
                {
                    _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>().currentActiveCanvas.SetTriangleMaterial(t, 0);
                }
                
            }
        }
    }

    private int SelectTriangle(Vector3 originPos)
    {
        RaycastHit hit;
        if (!Physics.Raycast(_camera.ScreenPointToRay(originPos), out hit))
        {
            Debug.Log("no hit");
            return -1;
        }

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
        {
            Debug.Log("no collider or no shared mesh whatever that means");
            return -1;
        }

        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        return hit.triangleIndex;
    }
}
