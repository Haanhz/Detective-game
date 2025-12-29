using UnityEngine;
using System.Collections.Generic;

public class NPC : MonoBehaviour
{
    [Header("NPC Identity")]
    public string npcName = "NPC Name";
    public Sprite portrait;

    [Header("Profile Linking")]
    public int profileIndex;

    [Header("Stage 0 - Intro (first time)")]
    public DialogueBlock introBlock;

    [Header("Stage 1 - Follow-up (after intro)")]
    public DialogueBlock followUpBlock;

    [HideInInspector]
    public int dialogueStage = 0;

    [Header("Stage 2+ - Conditional Dialogues")]
    public List<DialogueBlock> conditionalBlocks = new List<DialogueBlock>();

    // ===== NEW STRUCTS FOR MULTI-SPEAKER =====
    [System.Serializable]
    public struct DialogueLine
    {
        public enum Speaker { NPC, Player }
        public Speaker speaker;
        [TextArea(2, 5)]
        public string text;
    }

    [System.Serializable]
    public class DialogueBlock
    {
        public string id;

        // Thay đổi từ string[] sang DialogueLine[]
        public DialogueLine[] lines;

        [Header("Conditions")]
        public List<string> requiredEvidenceTags;
        [Header("Knowledge Condition (Dialogue Info)")]
        public string requiredNPC;
        public List<int> requiredKeys;

        [Header("Read Control")]
        public bool readOnceOnly;
        [HideInInspector]
        public bool hasRead;

        [Header("Knowledge Gain")]
        public string targetNPC; 
        public int infoKey;
        [TextArea]
        public string infoValue;
    }
}