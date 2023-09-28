using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class PaintingController : MonoBehaviour
{
    public static Shader defaultShader;
    private GameObject _characterSignalsInterfaceTarget;
    private FirstPersonController _firstPersonController;
    private Camera _camera;
    List<Brush> brushes = new List<Brush>();
    int _selectedBrush = 0;
    bool canScroll = true;

    void Start()
    {
        defaultShader = Resources.Load<Shader>("Shaders/DefaultShader");
        _characterSignalsInterfaceTarget = transform.parent.parent.gameObject;
        _firstPersonController = _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>();
        _camera = GetComponent<Camera>();

        brushes.Add(new Brush(Color.cyan, 3, 4f));
        brushes.Add(new Brush(Color.yellow, 5, 4f));
        brushes.Add(new Brush(Paint.CombineColors(Color.red, Color.blue, 0.7f), 3, 2f));

    }

    void Update()
    {
        if (_firstPersonController.currentActiveCanvas != null)
        {
            if (Input.GetMouseButton(0))
            {
                PaintWithBrush();
            }
            if (Input.GetMouseButton(1))
            {
                PaintWithBrush(true);
            }
        }

        if (canScroll && Mathf.Abs(Input.mouseScrollDelta.y) >= 1)
        {
            _selectedBrush = (_selectedBrush + (int)Input.mouseScrollDelta.y + brushes.Count) % brushes.Count;
            StartCoroutine(BrushScrollDelay(0.2f));
        }
    }

    private IEnumerator BrushScrollDelay(float t)
    {
        canScroll = false;
        yield return new WaitForSeconds(t);
        canScroll = true;
    }

    private void PaintWithBrush(bool rightClick = false)
    {
        int iterations = rightClick ? 1 : brushes[_selectedBrush].density;
        float sizeIncrease = rightClick ? 1f : brushes[_selectedBrush].dispersion;
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
            if (t < 0) continue;
            _firstPersonController.currentActiveCanvas.SetTriangleMaterial(t, brushes[_selectedBrush].GetMaterial());
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
