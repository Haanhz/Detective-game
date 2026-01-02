using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;

public static class SaveSystem
{
    public static void SaveAll(GameObject player)
    {
        // 1. Lưu thông số GameManager
        PlayerPrefs.SetInt("SavedDays", GameManager.Instance.daysRemaining);
        PlayerPrefs.SetInt("CurrentNight", GameManager.Instance.currentNight);

        // 2. Lưu Bằng chứng & Profile
        string evidenceData = string.Join(",", EvidenceManager.Instance.collectedEvidence);
        PlayerPrefs.SetString("SavedEvidence", evidenceData);
        string weightsData = string.Join("|", EvidenceManager.Instance.evidenceWeights.Select(x => x.Key + ":" + x.Value));
        PlayerPrefs.SetString("SavedWeights", weightsData);
        string unlockData = string.Join(",", CharacterUnlockManager.unlockedIndices);
        PlayerPrefs.SetString("SavedUnlocks", unlockData);

        // 3. Lưu Vị trí Player
        if (player != null) {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
        }

        // 4. Lưu Dictionary hội thoại
        SaveDict("SangInfo", DialogueManager.Instance.Sang);
        SaveDict("MaiInfo", DialogueManager.Instance.Mai);
        SaveDict("TanInfo", DialogueManager.Instance.Tan);
        SaveDict("MayInfo", DialogueManager.Instance.May);

        // 5. LƯU TRẠNG THÁI TỪNG NPC TRONG SCENE
        NPC[] allNPCs = Object.FindObjectsByType<NPC>(FindObjectsSortMode.None);
        foreach (NPC npc in allNPCs)
        {
            PlayerPrefs.SetInt("NPCStage_" + npc.npcName, npc.dialogueStage);
            // Lưu trạng thái đã đọc của từng block hội thoại điều kiện
            for (int i = 0; i < npc.conditionalBlocks.Count; i++)
            {
                PlayerPrefs.SetInt("NPCCond_" + npc.npcName + "_" + i, npc.conditionalBlocks[i].hasRead ? 1 : 0);
            }
        }

        // Lưu tên phòng hiện tại
        string currentRoom = PlayerPrefs.GetString("CurrentRoomName", "LivingRoom");
        PlayerPrefs.SetString("SavedRoom", currentRoom);

        PlayerPrefs.SetInt("HasSavedGame", 1);
        PlayerPrefs.Save();
    }

    public static void LoadAll(GameObject player)
    {
        if (PlayerPrefs.GetInt("HasSavedGame", 0) == 0) return;

        // 1. Tải Manager
        GameManager.Instance.daysRemaining = PlayerPrefs.GetInt("SavedDays", 7);
        GameManager.Instance.currentNight = PlayerPrefs.GetInt("CurrentNight", 0);

        // 2. Tải Bằng chứng & Profile
        string evData = PlayerPrefs.GetString("SavedEvidence", "");
        EvidenceManager.Instance.collectedEvidence = string.IsNullOrEmpty(evData) ? new List<string>() : new List<string>(evData.Split(','));
        EvidenceManager.Instance.evidenceWeights.Clear();
        string weightsData = PlayerPrefs.GetString("SavedWeights", "");
        if (!string.IsNullOrEmpty(weightsData))
        {
            foreach (var pair in weightsData.Split('|'))
            {
                var parts = pair.Split(':');
                if (parts.Length == 2) 
                    EvidenceManager.Instance.evidenceWeights[parts[0]] = float.Parse(parts[1]);
            }
        }
        string unData = PlayerPrefs.GetString("SavedUnlocks", "");
        CharacterUnlockManager.unlockedIndices.Clear();
        if (!string.IsNullOrEmpty(unData)) {
            foreach (var s in unData.Split(',')) CharacterUnlockManager.unlockedIndices.Add(int.Parse(s));
        }

        // 3. Tải Vị trí
        if (player != null) {
            player.transform.position = new Vector2(PlayerPrefs.GetFloat("PlayerX"), PlayerPrefs.GetFloat("PlayerY"));
        }

        string savedRoom = PlayerPrefs.GetString("SavedRoom", "LivingRoom");
        // Tìm tất cả các MapTransition trong cảnh để lấy đúng mapBoundary của phòng đó
        MapTransition[] allTransitions = Object.FindObjectsByType<MapTransition>(FindObjectsSortMode.None);
        foreach (var tr in allTransitions)
        {
            if (tr.areaName == savedRoom)
            {
                var confiner = Object.FindFirstObjectByType<CinemachineConfiner2D>();
                if (confiner != null) {
                    confiner.BoundingShape2D = tr.mapBoundary; // Cập nhật lại khung giới hạn camera
                }
                break;
            }
        }

        // 4. Tải Dictionary hội thoại
        LoadDict("SangInfo", DialogueManager.Instance.Sang);
        LoadDict("MaiInfo", DialogueManager.Instance.Mai);
        LoadDict("TanInfo", DialogueManager.Instance.Tan);
        LoadDict("MayInfo", DialogueManager.Instance.May);

        SyncDictionaryToProfile(0, DialogueManager.Instance.Sang);
        SyncDictionaryToProfile(1, DialogueManager.Instance.Mai);
        SyncDictionaryToProfile(2, DialogueManager.Instance.Tan);
        SyncDictionaryToProfile(3, DialogueManager.Instance.May);

        // 5. TẢI TRẠNG THÁI TỪNG NPC
        NPC[] allNPCs = Object.FindObjectsByType<NPC>(FindObjectsSortMode.None);
        foreach (NPC npc in allNPCs)
        {
            if (npc.GetComponent<QuestIntroNPC>() == null || !npc.GetComponent<QuestIntroNPC>().enabled)
            {
                npc.dialogueStage = PlayerPrefs.GetInt("NPCStage_" + npc.npcName, 0);
            }
            npc.dialogueStage = PlayerPrefs.GetInt("NPCStage_" + npc.npcName, 0);
            for (int i = 0; i < npc.conditionalBlocks.Count; i++)
            {
                npc.conditionalBlocks[i].hasRead = PlayerPrefs.GetInt("NPCCond_" + npc.npcName + "_" + i, 0) == 1;
            }
        }
    }

    private static void SyncDictionaryToProfile(int charIndex, Dictionary<int, string> dict)
    {
        if (ProfileUI.Instance == null) return;
        foreach (var entry in dict)
        {
            ProfileUI.Instance.AddInfoToDescription(charIndex, entry.Value);
        }
    }

    private static void SaveDict(string key, Dictionary<int, string> dict) {
        string s = string.Join("|", dict.Select(x => x.Key + ":" + x.Value));
        PlayerPrefs.SetString(key, s);
    }

    private static void LoadDict(string key, Dictionary<int, string> dict) {
        dict.Clear();
        string s = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(s)) return;
        foreach (var pair in s.Split('|')) {
            var parts = pair.Split(':');
            if (parts.Length == 2) dict[int.Parse(parts[0])] = parts[1];
        }
    }
}