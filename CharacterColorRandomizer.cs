using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterColorRandomizer : MonoBehaviour
{
    [Header("Parts to Color")]
    [SerializeField] private SpriteRenderer hairRenderer;
    [SerializeField] private SpriteRenderer skinRenderer;
    [SerializeField] private SpriteRenderer topRenderer;
    [SerializeField] private SpriteRenderer bottomRenderer;
    [SerializeField] private SpriteRenderer eyesRenderer;

    [Header("Color Palettes")]
    [SerializeField] private Color[] hairColors;
    [SerializeField] private Color[] skinColors;
    [SerializeField] private Color[] topColors;
    [SerializeField] private Color[] bottomColors;
    [SerializeField] private Color[] eyeColors;

    void OnEnable()
    {
        ApplyRandomColors();
    }

    public void ApplyRandomColors()
    {
        // 랜덤 색상 적용
        if (hairRenderer != null && hairColors.Length > 0)
            hairRenderer.color = hairColors[Random.Range(0, hairColors.Length)];

        if (skinRenderer != null && skinColors.Length > 0)
            skinRenderer.color = skinColors[Random.Range(0, skinColors.Length)];

        if (topRenderer != null && topColors.Length > 0)
            topRenderer.color = topColors[Random.Range(0, topColors.Length)];

        if (bottomRenderer != null && bottomColors.Length > 0)
            bottomRenderer.color = bottomColors[Random.Range(0, bottomColors.Length)];

        if (eyesRenderer != null && eyeColors.Length > 0)
            eyesRenderer.color = eyeColors[Random.Range(0, eyeColors.Length)];

        // 씬 이름 확인
        if (SceneManager.GetActiveScene().name == "05Scene")
        {
            // 채도 50% 감소
            DesaturateAll(0.5f);
        }
    }

    private void DesaturateAll(float desaturationAmount)
    {
        if (hairRenderer != null)
            hairRenderer.color = DesaturateColor(hairRenderer.color, desaturationAmount);
        if (skinRenderer != null)
            skinRenderer.color = DesaturateColor(skinRenderer.color, desaturationAmount);
        if (topRenderer != null)
            topRenderer.color = DesaturateColor(topRenderer.color, desaturationAmount);
        if (bottomRenderer != null)
            bottomRenderer.color = DesaturateColor(bottomRenderer.color, desaturationAmount);
        if (eyesRenderer != null)
            eyesRenderer.color = DesaturateColor(eyesRenderer.color, desaturationAmount);
    }

    private Color DesaturateColor(Color color, float desaturationAmount)
    {
        // RGB -> HSV 변환
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s *= (1f - desaturationAmount); // 채도 감소 (예: 0.5 = 50% 감소)
        return Color.HSVToRGB(h, Mathf.Clamp01(s), v);
    }
}
