using System.Collections;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [Header("Point Light")]
    [SerializeField] private Light flashLight;
    [SerializeField] private float intensity = 3f;
    [SerializeField] private float duration = 0.04f;
    [SerializeField] private Color lightColor = new Color(1f, 0.85f, 0.3f); // Warm orange

    private Coroutine flashRoutine;

    private void Awake()
    {
        if (flashLight != null)
            flashLight.enabled = false;
    }

    public void Play()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashLight.enabled = true;
        flashLight.intensity = intensity;
        flashLight.color = lightColor;

        yield return new WaitForSeconds(duration);

        flashLight.enabled = false;
        flashRoutine = null;
    }
}