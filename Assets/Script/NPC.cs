using UnityEngine;
using System.Collections.Generic;

// Đảm bảo script này xuất hiện dưới dạng Component trong Inspector
public class NPC : MonoBehaviour
{
    // ===== NPC IDENTITY =====
    [Header("NPC Identity")]
    public string npcName = "NPC Name";
    public Sprite portrait;

    // ===== DIALOGUE STAGES =====

    [Header("Stage 0 - Intro (first time)")]
    public DialogueBlock introBlock;

    [Header("Stage 1 - Follow-up (after intro)")]
    public DialogueBlock followUpBlock;


    [HideInInspector]
    public int dialogueStage = 0;
    // 0 = chưa đọc gì
    // 1 = đã đọc intro
    // 2+ = bắt đầu dùng conditional blocks

    // ===== CONDITIONAL DIALOGUES =====
    [Header("Stage 2+ - Conditional Dialogues")]
    public List<DialogueBlock> conditionalBlocks = new List<DialogueBlock>();

    // ===== STRUCT =====
    [System.Serializable]
    public class DialogueBlock
    {
        public string id;

        [TextArea(3, 10)]
        public string[] lines;

        [Header("Conditions")]
        public List<string> requiredEvidenceTags;
        [Header("Knowledge Condition (Dialogue Info)")]
        public string requiredNPC;      // "Sang" / "Mai" / "Tan" / "May"
        public List<int> requiredKeys;  // các key phải có trong dict


        [Header("Read Control")]
        public bool readOnceOnly;
        [HideInInspector]
        public bool hasRead;

        [Header("Knowledge Gain")]
        public string targetNPC;   // Sang / Mai / Tan / May
        public int infoKey;
        [TextArea]
        public string infoValue;
    }

}
