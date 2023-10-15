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

    void Start()
    {
        if (player == null) player = FindObjectOfType<FirstPersonController>();
        particles = GetComponentInChildren<ParticleSystem>();
        particles.Stop();
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
        GetComponentInChildren<SpriteRenderer>().sprite = Sprite.Create(refImage, new Rect(0f, 0f, refImage.width, refImage.height), new Vector2(0.5f, 0.5f), 10f);
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

        bool bad = true;

        if (paintingColors.Count > 1 && paintingData.strokeCount > 800)
        {
            bad = false;
        }

        if (bad)
        {
            Debug.Log("bad");
        }
        else
        {
            Debug.Log("good");
        }
    }
    private IEnumerator ProcessRef(float leniency)
    {
        for (int i = 0; i < refImage.width; i++)
        {
            for (int j = 0; j < refImage.height; j++)
            {
                Color p = ReduceColor(refImage.GetPixel(i, j), leniency);
                refImage.SetPixel(i, j, p);
                refColors.Add(p);
            }
            yield return new WaitForEndOfFrame();
        }
        refImage.Apply();
    }

    private Color ReduceColor(Color c, float leniency)
    {
        float r = (Mathf.Floor((c.r * 255f) / leniency) * leniency) / 255f;
        float g = (Mathf.Floor((c.g * 255f) / leniency) * leniency) / 255f;
        float b = (Mathf.Floor((c.b * 255f) / leniency) * leniency) / 255f;
        return new Color(r, g, b);
    }

    private void ActivatePoint()
    {
        questActive = true;
        particles.Play();
    }

    private void DeactivatePoint()
    {
        questActive = false;
        particles.Stop();
        ValidatePainting(FirstPersonController.paintingSave);
    }
    
    private void ActiveLoop()
    {

    }

    public void SetQuest(SpotQuest quest)
    {
        this.quest = quest;
    }
}
