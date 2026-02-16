using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightningEffect : MonoBehaviour
{
    public Volume volume;
    private ColorAdjustments colorAdjustments;
    AudioSource audioSource;
    public AudioClip thunderClip;

    public float effectDuration = 1f;       // 효과가 서서히 사라지는 시간
    private float elapsed = 0f;
    private bool isFading = false;

    private float startExposure = 2.84f;
    private float startContrast = 100f;
    private float startSaturation = -100f;

    private float defaultExposure = 0f;
    private float defaultContrast = 0f;
    private float defaultSaturation = 0f;

    WaitForSeconds thunderws;

    void Start()
    {
        if (volume.profile.TryGet(out colorAdjustments))
        {
            defaultExposure = colorAdjustments.postExposure.value;
            defaultContrast = colorAdjustments.contrast.value;
            defaultSaturation = colorAdjustments.saturation.value;
        }
        audioSource = GetComponent<AudioSource>();
        thunderws = new WaitForSeconds(0.1f);
        // 랜덤 번개 시작
        StartCoroutine(LightningRoutine());
    }

    void Update()
    {
        if (isFading)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / effectDuration);

            colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, defaultExposure, t);
            colorAdjustments.contrast.value = Mathf.Lerp(startContrast, defaultContrast, t);
            colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, defaultSaturation, t);

            if (t >= 1f)
            {
                isFading = false;
                elapsed = 0f;
            }
        }
    }

    public void TriggerLightningEffect()
    {
        if (colorAdjustments == null)
            return;

        colorAdjustments.postExposure.value = startExposure;
        colorAdjustments.contrast.value = startContrast;
        colorAdjustments.saturation.value = startSaturation;

        elapsed = 0f;
        isFading = true;
    }

    private IEnumerator LightningRoutine()
    {
        while (true)
        {
            // 랜덤 대기
            float waitTime = Random.Range(15f, 25f);
            yield return new WaitForSeconds(waitTime);

            // 번개 효과 발동
            TriggerLightningEffect();
            yield return thunderws;
            audioSource.PlayOneShot(thunderClip);
        }
    }
}
