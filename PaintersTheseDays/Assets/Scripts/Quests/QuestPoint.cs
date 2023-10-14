using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;

public class QuestPoint : MonoBehaviour
{

    private static FirstPersonController player;
    [NonSerialized]public bool questActive = false;
    public SpotQuest quest;
    public float activationRange = 5f;
    private Camera refCamera;
    private Texture2D refImage;

    void Start()
    {
        if (player == null) player = FindObjectOfType<FirstPersonController>();
        TakeReference();
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
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 1);
        refCamera.targetTexture = rt;
        refImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
        refCamera.Render();
        RenderTexture.active = rt;
        refImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        refImage.Apply();
        refCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        GetComponentInChildren<SpriteRenderer>().sprite = Sprite.Create(refImage, new Rect(0f, 0f, refImage.width, refImage.height), new Vector2(0.5f, 0.5f), 300f);
        refCamera.enabled = false;
    }
    
    private void ActivatePoint()
    {
        questActive = true;
    }

    private void DeactivatePoint()
    {
        questActive = false;
    }
    
    private void ActiveLoop()
    {

    }
}
