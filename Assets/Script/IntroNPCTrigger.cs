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

    private NPC npcComponent;
    private NPC.DialogueBlock originalIntroBlock;

    void Start()
    {
        npcComponent = GetComponent<NPC>();
        if (npcComponent == null)
        {
            Debug.LogError("QuestIntroNPC: Không tìm thấy NPC component!");
            enabled = false;
            return;
        }

        // Backup intro block gốc
        originalIntroBlock = npcComponent.introBlock;
        
        // Thay thế intro block bằng quest block
        npcComponent.introBlock = questBlock;
        
        // Đặt stage = -1 để không bị nhảy sang followup
        npcComponent.dialogueStage = -1;
    }

    void Update()
    {
        // Kiểm tra xem player đã có evidence chưa
        if (EvidenceManager.Instance != null && 
            EvidenceManager.Instance.HasEvidence(requiredEvidenceTag))
        {
            // Khôi phục lại intro block gốc
            npcComponent.introBlock = originalIntroBlock;
            
            // Reset stage về 0 để chạy lại intro thật
            npcComponent.dialogueStage = 0;
            
            Debug.Log("Quest completed! " + npcComponent.npcName + " chuyển sang intro thật.");
            
            // Tắt script này
            enabled = false;
        }
    }
}