using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance;

    public AudioMixer audioMixer;
    private readonly string BGMVolume = "BGM";
    private readonly string SFXVolume = "SFX";
    private readonly string tunnelingSizeKey = "TunnelingSize";

    private TunnelingVignetteController _tunnelingVignetteController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 컨트롤러와 렌더러 캐시
        _tunnelingVignetteController = FindAnyObjectByType<TunnelingVignetteController>();

        // 시작 시 저장된 값 적용
        float startValue = PlayerPrefs.GetFloat(tunnelingSizeKey, 0.7f);
        ApplyTunnelingSize(startValue);
    }

    public void SetBGMVolume(float value)
    { // 오디오
        audioMixer.SetFloat(BGMVolume, Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat(BGMVolume, value);
    }

    public void SetSFXVolume(float value)
    { // 오디오
        audioMixer.SetFloat(SFXVolume, Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat(SFXVolume, value);
    }

    public float GetBGMVolume() => PlayerPrefs.GetFloat(BGMVolume, 1f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat(SFXVolume, 1f);

    public void SetTunnelingSize(float value)
    { // 터널링
        PlayerPrefs.SetFloat(tunnelingSizeKey, value);
        ApplyTunnelingSize(value);
    }

    public float GetTunnelingSize() => PlayerPrefs.GetFloat(tunnelingSizeKey, 1f);

    private void ApplyTunnelingSize(float value)
    {
        VignetteParameters VignetteParametersValue = _tunnelingVignetteController.defaultParameters;
        VignetteParametersValue.apertureSize = value;
        _tunnelingVignetteController.defaultParameters = VignetteParametersValue;
    }
}
