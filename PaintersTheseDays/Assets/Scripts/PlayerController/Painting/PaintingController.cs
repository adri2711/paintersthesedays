using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PaintingController : MonoBehaviour
{
    public static Shader defaultShader;
    private GameObject _characterSignalsInterfaceTarget;
    private FirstPersonController _firstPersonController;
    private Camera _camera;
    List<Brush> _brushes = new List<Brush>();
    int _selectedBrush = 0;
    bool _canScroll = true;
    Vector3 prevMousePosition = Vector3.zero;
    float minActivationDistance = 50f;

    void Start()
    {
        defaultShader = Resources.Load<Shader>("Shaders/xdd");
        _characterSignalsInterfaceTarget = transform.parent.parent.gameObject;
        _firstPersonController = _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>();
        _camera = GetComponent<Camera>();

        _brushes.Add(new Brush(Color.cyan, 3, 4f));
        _brushes.Add(new Brush(Color.yellow, 7, 4f));
        _brushes.Add(new Brush(Paint.CombineColors(Color.red, Color.blue, 0.7f), 3, 2f));

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

        if (_canScroll && Mathf.Abs(Input.mouseScrollDelta.y) >= 1)
        {
            _selectedBrush = (_selectedBrush + (int)Input.mouseScrollDelta.y + _brushes.Count) % _brushes.Count;
            StartCoroutine(BrushScrollDelay(0.2f));
        }
    }

    public void SetSelectedBrushPaint(Paint p)
    {
        _brushes[_selectedBrush].paint = p;
    }

    private IEnumerator BrushScrollDelay(float t)
    {
        _canScroll = false;
        yield return new WaitForSeconds(t);
        _canScroll = true;
    }

    private void PaintWithBrush(bool rightClick = false)
    {
        if (Vector3.Distance(Input.mousePosition, prevMousePosition) < minActivationDistance / _firstPersonController.currentActiveCanvas.subdivisions) return;
        prevMousePosition = Input.mousePosition;

        int iterations = rightClick ? 1 : _brushes[_selectedBrush].density;
        float sizeIncrease = rightClick ? 1f : _brushes[_selectedBrush].dispersion;
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

        if (brushTris.Count > 0)
        {
            _firstPersonController.currentActiveCanvas.SetMaterialToTriangles(brushTris, _brushes[_selectedBrush].GetMaterial());
            _firstPersonController.currentActiveCanvas.ApplyMaterials();
        }
    }

    private int SelectTriangle(Vector3 originPos)
    {
        RaycastHit hit;
        if (!Physics.Raycast(_camera.ScreenPointToRay(originPos), out hit)) return -1;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null || meshCollider.gameObject.GetComponent<PaintingCanvas>() == null) return -1;

        Mesh mesh = meshCollider.sharedMesh;
        int[] triangles = mesh.triangles;
        return hit.triangleIndex;
    }
}
