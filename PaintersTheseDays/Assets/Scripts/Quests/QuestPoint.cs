using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;

public class QuestPoint : MonoBehaviour
{
    private Camera refCamera;
    private Texture2D refImage;
    private ParticleSystem particles;

    private static FirstPersonController player;
    [NonSerialized]public bool questActive = false;
    public SpotQuest quest;
    public float activationRange = 8f;
    HashSet<Color> refColors = new HashSet<Color>();
    HashSet<Color> paintingColors = new HashSet<Color>();
    Vector3 gradient;

    void Start()
    {
        if (player == null) player = FindObjectOfType<FirstPersonController>();
        particles = GetComponentInChildren<ParticleSystem>();
        particles.transform.Find("LineParticles").GetComponent<ParticleSystem>().Stop();
        TakeReference();
        StartCoroutine(ProcessRef(quest.leniency));

        player.GetComponent<ICharacterSignals>().PlacedCanvas.Subscribe(w =>
        {
            particles.Stop();
        }).AddTo(this);
    }
    void Update()
    {
        if (!questActive && Vector3.Distance(transform.position, player.transform.position) <= activationRange)
        {
            ActivatePoint();
        }
        else if (questActive && Vector3.Distance(transform.position, player.transform.position) > activationRange)
        {
            DeactivatePoint();
        }
        if (questActive)
        {
            ActiveLoop();
        }
    }

    private void TakeReference()
    {
        refCamera = gameObject.AddComponent<Camera>();
        refCamera.enabled = true;
        RenderTexture rt = new RenderTexture(Screen.width / 16, Screen.height / 16, 24);
        rt.filterMode = FilterMode.Point;
        refCamera.targetTexture = rt;
        refImage = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        refCamera.Render();
        RenderTexture.active = rt;
        refImage.ReadPixels(new Rect(0, 0, refImage.width, refImage.height), 0, 0);
        refImage.Apply();
        refCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        //GetComponentInChildren<SpriteRenderer>().sprite = Sprite.Create(refImage, new Rect(0f, 0f, refImage.width, refImage.height), new Vector2(0.5f, 0.5f), 10f);
        refCamera.enabled = false;
    }

    private void ValidatePainting(PaintingData paintingData)
    {
        if (paintingData == null) return;
        for (int i = 0; i < paintingData.materials.Length; i++)
        {
            if (paintingData.materials[i].name == "base") continue;
            Color t = ReduceColor(paintingData.materials[i].color, quest.leniency);
            paintingColors.Add(t);
        }
        
        Debug.Log("Ref: " + refColors.Count + ", Painting: " + paintingColors.Count);
        Debug.Log("Strokes: " + paintingData.strokeCount);


        Vector3 pg = CalculatePaintingGradient(paintingData, quest.leniency);
        float scale = (refImage.height * refImage.width) / paintingData.materials.Length / 4f;
        float gDiff = Mathf.Abs(((gradient.x + gradient.y + gradient.z) / 3f) * scale - ((pg.x + pg.y + pg.z) / 3f));
        Debug.Log("Painting Gradient: " + pg + ", Ref Gradient: " + gradient * scale + ", Diff: " + gDiff);
        if (paintingColors.Count > 1 && paintingData.strokeCount > 500)
        {
            quest.valid = true;
        }
    }

    private Vector3 CalculatePaintingGradient(PaintingData paintingData, float leniency)
    {
        Vector3 paintingGradient = Vector3.zero;

        List<List<int>> adj = paintingData.GenerateAdjacency();

        for(int i = 0; i < adj.Count; i++)
        {
            Vector3 avg = Vector3.zero;
            for (int j = 0; j < adj[i].Count; j++)
            {
                avg += CTV(ReduceColor(paintingData.materials[j].color, leniency));
            }
            Vector3 g = (avg / adj[i].Count) - CTV(ReduceColor(paintingData.materials[i].color, leniency));
            paintingGradient += Abs(g);
        }
        paintingGradient /= adj.Count;
        return paintingGradient;
    }
    private IEnumerator ProcessRef(float leniency)
    {
        gradient = Vector3.zero;
        for (int i = 0; i < refImage.width; i++)
        {
            for (int j = 0; j < refImage.height; j++)
            {
                //Reduce color
                Color p =ReduceColor(refImage.GetPixel(i, j), leniency);
                refImage.SetPixel(i, j, p);
                refColors.Add(p);

                //Find gradient
                Vector3 avg = Vector3.zero;
                int c = 0;
                if (i < refImage.width - 1)
                {
                    avg += CTV(ReduceColor(refImage.GetPixel(i + 1, j), leniency));
                    c++;
                }
                if (j < refImage.height - 1)
                {
                    avg += CTV(ReduceColor(refImage.GetPixel(i, j + 1), leniency));
                    c++;
                }
                if (i > 0)
                {
                    avg += CTV(ReduceColor(refImage.GetPixel(i - 1, j), leniency));
                    c++;
                }
                if (j > 0)
                {
                    avg += CTV(ReduceColor(refImage.GetPixel(i, j - 1), leniency));
                    c++;
                }
                Vector3 g = (avg / c) - CTV(p);
                gradient += Abs(g);
            }
            yield return new WaitForEndOfFrame();
        }
        refImage.Apply();
        gradient /= refImage.width * refImage.height;
    }

    private Color ReduceColor(Color c, float leniency)
    {
        float r = (Mathf.Floor((c.r * 255f) / leniency) * leniency) / 255f;
        float g = (Mathf.Floor((c.g * 255f) / leniency) * leniency) / 255f;
        float b = (Mathf.Floor((c.b * 255f) / leniency) * leniency) / 255f;
        return new Color(r, g, b);
    }

    private Vector3 CTV(Color c)
    {
        return new Vector3(c.r, c.g, c.b);
    }

    private Vector3 Abs(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    private void ActivatePoint()
    {
        questActive = true;
        particles.Play();
    }

    private void DeactivatePoint()
    {
        questActive = false;
        particles.Play();
        particles.transform.Find("LineParticles").GetComponent<ParticleSystem>().Stop();
        if (player.canPlaceCanvas)
        {
            ValidatePainting(FirstPersonController.paintingSave);
        }
    }
    
    private void ActiveLoop()
    {

    }

    public void SetQuest(SpotQuest quest)
    {
        this.quest = quest;
    }
}
