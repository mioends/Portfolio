using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TMPTypewriter : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text textComponent;

    [Tooltip("�ʴ� ���̴� ���� ��")]
    public float charsPerSecond = 40f;
    [Tooltip("��ħǥ(.,!?) �� ����")]
    public WaitForSeconds punctuationPauseWs;
    public float punctuationPause = 0.25f;
    [Tooltip("��ǥ �� ����")]
    public WaitForSeconds commaPauseWs;
    public float commaPause = 0.12f;

    [Tooltip("����ڰ� Ŭ���ϸ� ��� ��ü �ؽ�Ʈ�� ǥ��")]
    public bool allowSkipOnClick = true;
    [Tooltip("�ٽ� Play ȣ�� �� �ڵ����� ���� �ڷ�ƾ ����")]
    public bool stopPreviousOnPlay = true;

    // ���� ����
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
    { // ���ڷ� ��ü �ؽ�Ʈ �޾Ƽ� ���
        if (stopPreviousOnPlay && revealCoroutine != null)
        { // �̹� ��ũ��Ʈ�� ���� ���̸� stopPreviousOnPlay
            StopCoroutine(revealCoroutine);
            revealCoroutine = null;
        }

        cachedFullText = fullText ?? "";
        // SetText �� ���� �ϰ� maxVisibleCharacters�� ����
        textComponent.SetText(cachedFullText, true);
        textComponent.ForceMeshUpdate(); // textInfo�� ��� ä��� ����

        // �ʱ�ȭ
        textComponent.maxVisibleCharacters = 0;
        lastVisibleCount = 0;

        revealCoroutine = StartCoroutine(RevealCoroutine());
    }
    
    public void Skip()
    { // ��� ��ü ǥ�� (��ŵ)
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
    { // ���� ��� ���̸� ����
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
        return c == '.' || c == '!' || c == '?' || c == '��' || c == '��' || c == '��' || c == '��';
    }

    // ��ŵ ó��
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!allowSkipOnClick) return;
        Skip();
    }
}