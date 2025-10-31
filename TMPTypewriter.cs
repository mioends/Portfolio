using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TMPTypewriter : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text textComponent;

    [Tooltip("초당 보이는 문자 수")]
    public float charsPerSecond = 40f;
    [Tooltip("마침표(.,!?) 뒤 지연")]
    public WaitForSeconds punctuationPauseWs;
    public float punctuationPause = 0.25f;
    [Tooltip("쉼표 뒤 지연")]
    public WaitForSeconds commaPauseWs;
    public float commaPause = 0.12f;

    [Tooltip("사용자가 클릭하면 즉시 전체 텍스트를 표시")]
    public bool allowSkipOnClick = true;
    [Tooltip("다시 Play 호출 시 자동으로 이전 코루틴 중지")]
    public bool stopPreviousOnPlay = true;

    // 내부 상태
    private Coroutine revealCoroutine;
    private string cachedFullText = "";
    private int lastVisibleCount = 0;

    void Reset()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void Awake()
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();

        commaPauseWs = new WaitForSeconds(commaPause);
        punctuationPauseWs = new WaitForSeconds(punctuationPause);
    }

    public void Play(string fullText)
    { // 인자로 전체 텍스트 받아서 재생
        if (stopPreviousOnPlay && revealCoroutine != null)
        { // 이미 스크립트가 진행 중이면 stopPreviousOnPlay
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        cachedFullText = fullText ?? "";
        // SetText 한 번만 하고 maxVisibleCharacters로 제어
        textComponent.SetText(cachedFullText, true);
        textComponent.ForceMeshUpdate(); // textInfo를 즉시 채우기 위해

        // 초기화
        textComponent.maxVisibleCharacters = 0;
        lastVisibleCount = 0;

        revealCoroutine = StartCoroutine(RevealCoroutine());
    }
    
    public void Skip()
    { // 즉시 전체 표시 (스킵)
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        textComponent.ForceMeshUpdate();
        textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
        lastVisibleCount = textComponent.textInfo.characterCount;
    }

    public void StopReveal()
    { // 현재 재생 중이면 중지
        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }
    }

    private IEnumerator RevealCoroutine()
    {
        textComponent.ForceMeshUpdate();
        int totalVisible = textComponent.textInfo.characterCount;
        if (totalVisible == 0)
        {
            revealCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        lastVisibleCount = 0;

        while (lastVisibleCount < totalVisible)
        {
            elapsed += Time.deltaTime;
            int target = Mathf.FloorToInt(elapsed * charsPerSecond);

            if (target > lastVisibleCount)
            {
                target = Mathf.Min(target, totalVisible);
                while (lastVisibleCount < target)
                {
                    lastVisibleCount++;
                    textComponent.maxVisibleCharacters = lastVisibleCount;

                    int charIndex = lastVisibleCount - 1;
                    if (charIndex >= 0 && charIndex < textComponent.textInfo.characterCount)
                    {
                        char lastChar = textComponent.textInfo.characterInfo[charIndex].character;
                        if (IsSentencePunctuation(lastChar))
                        {
                            if (punctuationPause > 0f)
                                yield return (punctuationPauseWs);
                        }
                        else if (lastChar == ',')
                        {
                            if (commaPause > 0f)
                                yield return commaPauseWs;
                        }
                    }
                }
            }
            yield return null;
        }
        revealCoroutine = null;
    }

    private bool IsSentencePunctuation(char c)
    {
        return c == '.' || c == '!' || c == '?' || c == '…' || c == '。' || c == '！' || c == '？';
    }

    // 스킵 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!allowSkipOnClick) return;
        Skip();
    }
}