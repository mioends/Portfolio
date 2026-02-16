using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager d_Instance;

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    [Header("Buttons")]
    public GameObject yesBtn;              // Yes/Okay 버튼
    public GameObject noBtn;               // No 버튼
    public Button nextButton;              // 다음 버튼 (선택지가 없을 때만 사용)
    public TextMeshProUGUI yesBtnText;     // Yes 버튼 안의 텍스트
    public TextMeshProUGUI noBtnText;      // No 버튼 안의 텍스트

    [Header("Dialogue Assets")]
    public DialogueAsset zombieQuestGiveAsset;
    public DialogueAsset zombieQuestReminderAsset;
    public DialogueAsset zombieQuestCompleteAsset;
    public DialogueAsset npcKillQuestAsset;
    public DialogueAsset finalQuestAsset;

    private Dictionary<string, DialogueNode> nodeDict;
    private DialogueNode currentNode;
    private DialogueAsset dialogueAsset;

    void Awake()
    {
        if (d_Instance == null)
            d_Instance = this;
        else if (d_Instance != this)
            Destroy(this.gameObject);

        nodeDict = new Dictionary<string, DialogueNode>();

        yesBtnText = yesBtn.GetComponentInChildren<TextMeshProUGUI>();
        noBtnText = noBtn.GetComponentInChildren<TextMeshProUGUI>();

        // 버튼 비활성화 초기화
        yesBtn.SetActive(false);
        noBtn.SetActive(false);
        nextButton.gameObject.SetActive(false);

    }

    public void StartDialogue(string startNodeId)
    { // 대사 출력 시작,
        if (nodeDict.ContainsKey(startNodeId))
        {
            dialoguePanel.SetActive(true);
            ShowNode(nodeDict[startNodeId]);
        }
    }

    public void ShowNode(DialogueNode node)
    {
        currentNode = node;

        node.speakerName.StringChanged += (localizedValue) =>
        {
            speakerText.text = localizedValue;
        };
        node.speakerName.RefreshString();

        node.dialogText.StringChanged += (localizedValue) =>
        {
            dialogueText.text = localizedValue;
        };
        node.dialogText.RefreshString();

        ClearButtons();

        if (node.choices != null && node.choices.Count > 0)
        {
            SetupChoices(node.choices);
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
        }
    }

    private void SetupChoices(List<DialogueChoice> choices)
    { // 선택지가 0일 경우 버튼 X
        if (choices.Count >= 1)
        { // 선택지가 한개 이상일 경우 yes/okay 버튼 활성화
            yesBtn.SetActive(true);
            yesBtnText.text = choices[0].choiceText.GetLocalizedString();
            choices[0].choiceText.RefreshString();
            yesBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            yesBtn.GetComponent<Button>().onClick.AddListener(() => {
                UIManager.u_Instance.OnYesOrOkayClick();
                OnChoiceSelected(choices[0]);
            });
        }

        if (choices.Count >= 2)
        { // 선택지가 두개 이상일 경우 No 버튼 활성화
            noBtn.SetActive(true);
            noBtnText.text = choices[1].choiceText.GetLocalizedString();
            choices[1].choiceText.RefreshString();
            noBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            noBtn.GetComponent<Button>().onClick.AddListener(() => {
                UIManager.u_Instance.OnNoClick();
                OnChoiceSelected(choices[1]);
            });
        }
    }


    private void ClearButtons()
    { // 버튼 전부 비활성화, 이벤트 삭제
        yesBtn.SetActive(false);
        noBtn.SetActive(false);
        nextButton.gameObject.SetActive(false);

        yesBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        noBtn.GetComponent<Button>().onClick.RemoveAllListeners();
    }

    public void OnChoiceSelected(DialogueChoice choice)
    {
        ClearButtons();

        if (choice.nextDialogue != null)
        {
            dialogueAsset = choice.nextDialogue;

            nodeDict.Clear();
            foreach (var node in dialogueAsset.nodes)
            {
                nodeDict[node.nodeId] = node;
            }

            ShowNode(nodeDict["01"]);
        }
        else if (!string.IsNullOrEmpty(choice.nextNodeId) && nodeDict.ContainsKey(choice.nextNodeId))
        {
            ShowNode(nodeDict[choice.nextNodeId]);
        }
        else
        {
            EndDialogue();
        }
    }

    public void OnENest()
    {
        if (!string.IsNullOrEmpty(currentNode.nextNodeId) && nodeDict.ContainsKey(currentNode.nextNodeId))
        {
            ShowNode(nodeDict[currentNode.nextNodeId]);
        }
    }
    public void OnNext()
    { // 다음 버튼 누르면 다음 대사 출력, 없으면 대사 끝내기
        if (!string.IsNullOrEmpty(currentNode.nextNodeId) && nodeDict.ContainsKey(currentNode.nextNodeId))
        {
            ShowNode(nodeDict[currentNode.nextNodeId]);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    { // 대화 끝. 대사창 비활성화, 버튼 비활성화, 현대 대사 삭제
        dialoguePanel.SetActive(false);
        ClearButtons();
        currentNode = null;
    }
    public void SetDialogueAsset(DialogueAsset newAsset)
    {
        dialogueAsset = newAsset;

        nodeDict.Clear();
        foreach (var node in dialogueAsset.nodes)
        {
            nodeDict[node.nodeId] = node;
        }
    }
}
