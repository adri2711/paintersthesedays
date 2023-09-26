using System.Collections;
using System.Collections.Generic;
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
                SelectTriangle();
            }
        }
    }

    private void SelectTriangle()
    {
        RaycastHit hit;
        if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            Debug.Log("no hit");
            return;
        }

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
        {
            Debug.Log("no collider or no shared mesh whatever that means");
            return;
        }

        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        int triangleIndex = hit.triangleIndex;

        _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>().currentActiveCanvas.SetTriangleMaterial(triangleIndex, 0);
    }


}
