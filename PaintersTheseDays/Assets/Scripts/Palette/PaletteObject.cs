using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;

public class PaletteObject : MonoBehaviour
{
    private GameObject _characterSignalsInterfaceTarget;
    private ICharacterSignals _characterSignals;
    private FirstPersonController _firstPersonController;
    private PaintingController _paintingController;
    private Camera _camera;
    private MeshRenderer _model;
    private BrushObject _brushObject;
    private Object _paintChunkPrefab;
    private Object _glassPrefab;
    private List<Paint> _paints = new List<Paint>();
    private List<PaintChunk> _paintChunks = new List<PaintChunk>();
    private PaintChunk _mixerPaintChunk;
    private PaletteGlass _glass;
    private bool _canClick = true;
    private bool _erase = false;
    private bool _active = false;

    void Start()
    {
        _paintChunkPrefab = Resources.Load("Prefab/Models/PaintChunk");
        _glassPrefab = Resources.Load("Prefab/Models/Glass");
        _characterSignalsInterfaceTarget = transform.parent.parent.parent.gameObject;
        _characterSignals = _characterSignalsInterfaceTarget.GetComponent<ICharacterSignals>();
        _firstPersonController = _characterSignalsInterfaceTarget.GetComponent<FirstPersonController>();
        _paintingController = GetComponentInParent<PaintingController>();
        _camera = GetComponentInParent<Camera>();
        _brushObject = transform.parent.parent.GetComponentInChildren<BrushObject>();
        _model = transform.Find("PaletteModel").GetComponent<MeshRenderer>();
        _model.enabled = false;
        SetPaints(new Color[] { Color.cyan, Color.yellow, Color.magenta, Color.black, Color.white }, true);

        _characterSignals.EnteredCanvas.Subscribe(w =>
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
        PaletteGlass paletteGlass = hit.collider.GetComponentInParent<PaletteGlass>();
        if (paintChunk != null)
        {
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
                    _mixerPaintChunk.Setup(new Paint(paintChunk.paint.GetColor()), true);
                    _paintChunks.Add(_mixerPaintChunk);
                    _mixerPaintChunk.timesMixed++;
                }
                else
                {
                    _mixerPaintChunk.timesMixed++;
                    AddPaintToMix(paintChunk.paint, Mathf.Max(1f / _mixerPaintChunk.timesMixed, 0.15f));
                    UpdatePaintMixMaterial();
                }
            }
            else
            {
                if (_erase) 
                {
                    if (!paintChunk.paint.IsPrimary())
                    {
                        _erase = false;
                        _paints.Remove(paintChunk.paint);
                        GeneratePaintChunks();
                    }
                }
                else
                {
                    _paintingController.SetSelectedBrushPaint(paintChunk.paint);
                    _brushObject.SetPaint(paintChunk.paint);
                }
            }
        }
        else if (paletteGlass != null)
        {
            _erase = true;
        }
        else return;
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
        foreach (Paint paint in _paints)
        {
            if (paint.GetColor() == _mixerPaintChunk.paint.GetColor())
            {
                return;
            }
        }
        _paints.Add(_mixerPaintChunk.paint);
    }
    public void ClearMix()
    {
        Destroy(_mixerPaintChunk);
    }
    public void AddPaint(Color c, bool primary = false)
    {
        _paints.Add(new Paint(c, primary));
    }
    public void SetPaints(Color[] colors, bool primary = false)
    {
        _paints = new List<Paint>();
        foreach (Color c in colors)
        {
            _paints.Add(new Paint(c, primary));
        }
    }
    private void Activate()
    {
        _active = true;
        _model.enabled = true;
        _erase = false;
        _model.GetComponent<Animator>().Play("Show");
        if (_paintChunks.Count == 0) GeneratePaintChunks();
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
        if (_glass != null)
        {
            Destroy(_glass.gameObject);
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
        _glass = Instantiate(_glassPrefab, _model.transform).GetComponent<PaletteGlass>();
    }
}
