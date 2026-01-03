using UnityEngine;

public class QuestIntroNPC : MonoBehaviour
{
    [Header("Quest Settings")]
    public string requiredEvidenceTag = "Glasses";

    [Header("Quest Dialogue")]
    public NPC.DialogueBlock questBlock = new NPC.DialogueBlock
    {
        id = "quest_intro",
        lines = new NPC.DialogueLine[]
        {
            new NPC.DialogueLine
            {
                speaker = NPC.DialogueLine.Speaker.NPC,
                text = "Hey! You over there!"
            },
            new NPC.DialogueLine
            {
                speaker = NPC.DialogueLine.Speaker.NPC,
                text = "I lost my glasses. I can't see anything."
            },
            new NPC.DialogueLine
            {
                speaker = NPC.DialogueLine.Speaker.NPC,
                text = "Find them for me. They must be somewhere in this room."
            }
        }
    };

    private NPC npc;
    private NPC.DialogueBlock originalIntroBlock;
    private bool questCompleted = false;

    void Start()
    {
        npc = GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError("QuestIntroNPC: NPC component not found!");
            enabled = false;
            return;
        }

        originalIntroBlock = npc.introBlock;
        npc.lockDialogueStage = true;

        // 🔒 Nếu NPC đã qua intro → quest coi như xong
        if (npc.dialogueStage > 0)
        {
            questCompleted = true;
            enabled = false;
            return;
        }

        // 🔑 Nếu CHƯA có evidence → dùng quest intro
        if (!EvidenceManager.Instance.HasEvidence(requiredEvidenceTag))
        {
            npc.introBlock = questBlock;
            npc.dialogueStage = 0; // GIỮ NGUYÊN STAGE 0
        }
        else
        {
            CompleteQuest();
        }
    }

    void Update()
    {
        if (questCompleted) return;
        if (EvidenceManager.Instance == null) return;

        if (EvidenceManager.Instance.HasEvidence(requiredEvidenceTag))
        {
            CompleteQuest();
        }
    }

    void CompleteQuest()
    {
        questCompleted = true;

        npc.introBlock = originalIntroBlock;
        npc.lockDialogueStage = false;
        npc.dialogueStage = 0; // intro thật sẽ chạy

        Debug.Log($"[QuestIntroNPC] Quest completed for {npc.npcName}");

        enabled = false;
    }
}
