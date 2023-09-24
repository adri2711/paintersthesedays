using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    Vector3 startPos;
    Vector3 offset = new Vector3();
    int quadrant = 0;
    private IEnumerator shake;

    private void Start()
    {
        startPos = transform.localPosition;
    }
    public void SmallShake(float dur, bool circular = true, float intensity = 1f)
    {
        shake = Run(dur, intensity, false, new Vector2(0.3f, 1f));
        StartCoroutine(shake);
    }
    public void MediumShake(float dur, bool circular = true, float intensity = 2f)
    {
        shake = Run(dur, intensity, circular, new Vector2(1f, 1f));
        StartCoroutine(shake);
    }
    public void BigShake(float dur, bool circular = true, float intensity = 4f)
    {
        shake = Run(dur, intensity, circular, new Vector2(1f, 1f));
        StartCoroutine(shake);
    }
    private IEnumerator Run(float dur, float intensity, bool circular, Vector2 axis)
    {
        float elapsedTime = 0f;

        while (elapsedTime < dur)
        {
            elapsedTime += Time.deltaTime;
            float strength = 1f;
            if (curve != null)
            {
                strength = curve.Evaluate(elapsedTime / dur);
            }

            if (circular)
            {
                quadrant = (quadrant + 90) % 360;
                float angle = (Random.Range(0, 90) + quadrant) * Mathf.Deg2Rad;
                offset = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
            }
            else
            {
                offset = Random.insideUnitSphere;
            }
            offset.x *= axis.x * Random.Range(0f, 1f);
            offset.y *= axis.y * Random.Range(0f, 1f);
            offset.z = 0f;

            transform.localPosition = startPos + offset * strength * intensity;
            yield return null;
        }

        transform.localPosition = startPos;
    }
    public void Stop()
    {
        StopCoroutine(shake);
    }
}
