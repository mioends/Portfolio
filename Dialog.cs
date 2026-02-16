using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UIManager;

public class Dialog : MonoBehaviour
{
    public TextMeshProUGUI mark;
    public TextMeshProUGUI ETxt;
    public GameObject player;

    public bool isTalking = false;

    PlayerInputSystem input;
    NPCDamage NPCDamage;
    public HeadCheck headCheck;
    public DialogueAsset dialogueToUse;

    void Awake()
    {
        mark = GetComponentInChildren<TextMeshProUGUI>();
        player = FindAnyObjectByType<PlayerInputSystem>().gameObject;
        input = player.GetComponent<PlayerInputSystem>();
        headCheck = GetComponent<HeadCheck>();
        NPCDamage = GetComponent<NPCDamage>();
        MarkOnOff(true);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject == player)
        {
            if (input.isAction)
            {
                input.isAction = false;
                if (!isTalking)
                {
                    // 대화 시작 로직
                    if (UIManager.u_Instance.doIWantKillNPC) return;
                    isTalking = true;
                    NPCDamage.source.PlayOneShot(NPCDamage.hiClip);
                    MarkOnOff(false);
                    PlayerMoveInactive();

                    DialogueAsset selectedAsset;
                    string startNodeId = DetermineStartNodeId(out selectedAsset);
                    DialogueManager.d_Instance.SetDialogueAsset(selectedAsset);
                    DialogueManager.d_Instance.StartDialogue(startNodeId);
                }
                else
                {
                    // 대화 중인데 액션키 누르면 다음 대사 출력
                    DialogueManager.d_Instance.OnENest();
                }
            }
        }
    }


    public string DetermineStartNodeId(out DialogueAsset selectedAsset)
    {
        DialogueState state = UIManager.u_Instance.GetCurrentDialogueState();

        switch (state)
        {
            case DialogueState.GiveZombieQuest:
                selectedAsset = DialogueManager.d_Instance.zombieQuestGiveAsset;
                return "ZombieQuest_Give01";

            case DialogueState.RemindZombieQuest:
                selectedAsset = DialogueManager.d_Instance.zombieQuestReminderAsset;
                return "ZombieQuest_Reminder";

            case DialogueState.CompleteZombieQuest:
                var detectedItems = headCheck.GetDetectedItems();
                for (int i = detectedItems.Count - 1; i >= 0; i--)
                {
                    detectedItems[i].SetActive(false);
                }
                selectedAsset = DialogueManager.d_Instance.zombieQuestCompleteAsset;
                return "ZombieQuest_Complete";

            case DialogueState.StartNPCKillQuest:
                selectedAsset = DialogueManager.d_Instance.npcKillQuestAsset;
                return "NPCKillQuest01";

            case DialogueState.FinalQuest:
                selectedAsset = DialogueManager.d_Instance.finalQuestAsset;
                return "finalQuest";
            default:
                selectedAsset = DialogueManager.d_Instance.finalQuestAsset;
                return "finalQuest";
        }
    }


    private void PlayerMoveInactive()
    {
        player.GetComponent<PlayerControl>().enabled = false;
        player.GetComponent<PlayerFire>().enabled = false;
    }

    public void MarkOnOff(bool set)
    {
        mark.enabled = set;
        ETxt.enabled = set;
    }
}
