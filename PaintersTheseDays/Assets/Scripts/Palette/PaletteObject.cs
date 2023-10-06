using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PaletteObject : MonoBehaviour
{
    private GameObject _characterSignalsInterfaceTarget;
    private ICharacterSignals _characterSignals;
    private FirstPersonController _firstPersonController;
    private PaintingController _paintingController;
    private Camera _camera;
    private MeshRenderer _model;
    private Object _paintChunkPrefab;
    private List<Paint> _paints = new List<Paint>();
    private List<PaintChunk> _paintChunks = new List<PaintChunk>();
    private PaintChunk _mixerPaintChunk;
    private bool _canClick = true;
    bool _active = false;

    void Start()
    {
        _paintChunkPrefab = Resources.Load("Prefab/Models/PaintChunk");
        _characterSignalsInterfaceTarget = transform.parent.parent.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _firstPersonController = _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>();
        _paintingController = GetComponentInParent<PaintingController>();
        _camera = GetComponentInParent<Camera>();
        _model = transform.Find("PaletteModel").GetComponent<MeshRenderer>();
        _model.enabled = false;

        _characterSignals.PlacedCanvas.Subscribe(w =>
        {
            Activate();
        }).AddTo(this);
        _characterSignals.ExitedCanvas.Subscribe(w =>
        {
            Deactivate();
        }).AddTo(this);
    }

    void Update()
    {
        if (_canClick && _firstPersonController.currentActiveCanvas != null)
        {
            if (Input.GetMouseButton(0))
            {
                ClickChunk();
            }
            if (Input.GetMouseButton(1))
            {
                ClickChunk(true);
            }
        }
    }
    private void ClickChunk(bool right = false)
    {
        RaycastHit hit;
        if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit)) return;
        PaintChunk paintChunk = hit.collider.GetComponent<PaintChunk>();
        if (paintChunk == null) return;
        if (right) 
        {
            if (paintChunk.isMix)
            {
                ConfirmMix();
                GeneratePaintChunks();
            }
            else if (_mixerPaintChunk == null)
            {
                Object c = Instantiate(_paintChunkPrefab, _model.transform);
                _mixerPaintChunk = c.AddComponent<PaintChunk>();
                _mixerPaintChunk.Setup(paintChunk.paint, true);
                _paintChunks.Add(_mixerPaintChunk);
                _mixerPaintChunk.timesMixed++;
            }
            else
            {
                _mixerPaintChunk.timesMixed++;
                AddPaintToMix(paintChunk.paint, Mathf.Max(1f / _mixerPaintChunk.timesMixed, 0.15f));
                UpdatePaintMixMaterial();
                Debug.Log(_mixerPaintChunk.paint.GetColor().ToString());
            }
        }
        else
        {
            _paintingController.SetSelectedBrushPaint(paintChunk.paint);
        }
        StartCoroutine(ClickDelay(0.2f));
    }
    public void AddPaintToMix(Paint p, float w)
    {
        if (_mixerPaintChunk.paint == null)
        {
            _mixerPaintChunk.paint = p;
        }
        else
        {
            _mixerPaintChunk.paint = Paint.CombinePaint(p, _mixerPaintChunk.paint, w);
        }
    }
    public void ConfirmMix()
    {
        _paints.Add(_mixerPaintChunk.paint);
    }
    public void ClearMix()
    {
        Destroy(_mixerPaintChunk);
    }
    public void AddPaint(Color c)
    {
        _paints.Add(new Paint(c));
    }
    public void SetPaints(Color[] colors)
    {
        _paints = new List<Paint>();
        foreach (Color c in colors)
        {
            _paints.Add(new Paint(c));
        }
    }
    private void Activate()
    {
        _active = true;
        SetPaints(new Color[] { Color.cyan, Color.yellow, Color.magenta, Color.black});
        _model.enabled = true;
        _model.GetComponent<Animator>().Play("Show");
        GeneratePaintChunks();
    }
    private void Deactivate()
    {
        _active = false;
        StartCoroutine(DeactivateCoroutine());
    }
    private IEnumerator DeactivateCoroutine()
    {
        _model.GetComponent<Animator>().Play("Hide");
        yield return new WaitForSeconds(1f);
        _model.enabled = _active;
    }
    private IEnumerator ClickDelay(float t)
    {
        _canClick = false;
        yield return new WaitForSeconds(t);
        _canClick = true;
    }
    private void UpdatePaintMixMaterial()
    {
        _mixerPaintChunk.UpdateMaterial();
    }
    private void GeneratePaintChunks()
    {
        foreach (PaintChunk p in _paintChunks)
        {
            Destroy(p.gameObject);
        }
        _paintChunks.Clear();
        for (int i = 0; i < _paints.Count; i++)
        {
            Object c = Instantiate(_paintChunkPrefab, _model.transform);
            PaintChunk paintChunk = c.AddComponent<PaintChunk>();
            paintChunk.Setup(_paints[i]);
            float a = Mathf.Deg2Rad * (90f + Mathf.Pow(-1, i + 1) * Mathf.Floor((i + 1) / 2) * 2f * 140f / Mathf.Max(_paints.Count, 5));
            Vector3 r = new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f);
            float o = 0.03f;
            Vector3 pos = paintChunk.transform.localPosition + r * o;
            paintChunk.transform.localPosition = pos;
            _paintChunks.Add(paintChunk);
        }
    }
}
