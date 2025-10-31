using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [Header("대사 데이터")]
    public List<DialogueData> dialogueDatas = new List<DialogueData>();

    [Header("UI 참조")]
    public GameObject dialogueCanvas;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Button nextButton;
    public TMPTypewriter typewriter;
    public TMP_Text nextButtonText; // 대답 텍스트

    private int currentDialogueSet = 0;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;

    Animator animator;
    int hashTalk = Animator.StringToHash("IsTalking");

    private bool isShakeMode = false;
    private Vector3 originalPanelPos;
    public float intensity = 0.01f;
    public float speed = 30f;

    void Awake()
    {
        typewriter = dialogueText.GetComponent<TMPTypewriter>();
        animator = GetComponent<Animator>();
        dialogueCanvas.SetActive(false);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextClicked);

        originalPanelPos = dialogueCanvas.transform.localPosition;
    }

    private void Start()
    {
        StartDialogue(0);
    }

    public void StartDialogue(int dialogueSetIndex)
    {
        StartDialogueInternal(dialogueSetIndex, false);
    }

    public void StartShakeDialogue(int dialogueSetIndex)
    {
        StartDialogueInternal(dialogueSetIndex, true);
    }

    private void StartDialogueInternal(int dialogueSetIndex, bool horrorMode)
    {
        if (dialogueSetIndex < 0 || dialogueSetIndex >= dialogueDatas.Count)
        {
            Debug.LogWarning($"잘못된 대사 인덱스: {dialogueSetIndex}");
            return;
        }

        currentDialogueSet = dialogueSetIndex;
        currentLineIndex = 0;
        dialogueActive = true;
        isShakeMode = horrorMode;

        var data = dialogueDatas[currentDialogueSet];
        nameText.text = data.npcName;

        dialogueCanvas.SetActive(true);
        PlayCurrentLine();

        if (isShakeMode)
            StartCoroutine(ShakeDialoguePanel());
    }

    private void PlayCurrentLine()
    {
        StopAllCoroutines();

        var data = dialogueDatas[currentDialogueSet];
        if (currentLineIndex >= data.lines.Count)
        {
            EndDialogue();
            return;
        }

        
        var line = data.lines[currentLineIndex]; // 현재 대사 표시
        dialogueText.text = "";
        isTyping = true;

        if (animator != null) animator.SetBool(hashTalk, true);

        typewriter.Play(line.dialogueText);
        StartCoroutine(WaitForTypingEnd());

        if (data.responseLines.Count > currentLineIndex)
        { // 현재 대사에 대응하는 응답 텍스트 설정
            nextButtonText.text = data.responseLines[currentLineIndex].responseText;
        }
        else
        {
            nextButtonText.text = "대화 끝";
        }
    }

    private IEnumerator WaitForTypingEnd()
    {
        while (true)
        {
            yield return null;
            if (dialogueText.maxVisibleCharacters >= dialogueText.textInfo.characterCount)
                break;
        }

        if (animator != null) animator.SetBool(hashTalk, false);
        isTyping = false;
    }

    private void OnNextClicked()
    {
        if (!dialogueActive)
            return;

        if (isTyping)
        {
            typewriter.Skip();
            isTyping = false;
            return;
        }

        currentLineIndex++;

        var data = dialogueDatas[currentDialogueSet];
        if (currentLineIndex < data.lines.Count)
        {
            PlayCurrentLine();

            if (isShakeMode)
            {
                StopCoroutine(nameof(ShakeDialoguePanel));
                StartCoroutine(ShakeDialoguePanel());
            }
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        dialogueCanvas.SetActive(false);
        dialogueActive = false;
        isTyping = false;
        isShakeMode = false;

        StopCoroutine(nameof(ShakeDialoguePanel));
        dialogueCanvas.transform.localPosition = originalPanelPos;
    }

    private IEnumerator ShakeDialoguePanel()
    {
        while (isShakeMode && dialogueActive)
        {
            dialogueCanvas.transform.localPosition = originalPanelPos +
                (Vector3)Random.insideUnitCircle * intensity;

            yield return new WaitForSeconds(1f / speed);
        }

        dialogueCanvas.transform.localPosition = originalPanelPos;
    }
}
