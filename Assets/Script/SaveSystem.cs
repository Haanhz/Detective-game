using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class SaveSystem
{
    public static void SaveAll(GameObject player)
    {
        // 1. Lưu thông số cơ bản
        PlayerPrefs.SetInt("SavedDays", GameManager.Instance.daysRemaining);
        PlayerPrefs.SetInt("CurrentNight", GameManager.Instance.currentNight);

        // 2. Lưu Bằng chứng & Profile
        string evidenceData = string.Join(",", EvidenceManager.Instance.collectedEvidence);
        PlayerPrefs.SetString("SavedEvidence", evidenceData);

        string unlockData = string.Join(",", CharacterUnlockManager.unlockedIndices);
        PlayerPrefs.SetString("SavedUnlocks", unlockData);

        // 3. Lưu Vị trí
        if (player != null) {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
        }

        // 4. Lưu Lời thoại (Dictionary)
        SaveDict("SangInfo", DialogueManager.Instance.Sang);
        SaveDict("MaiInfo", DialogueManager.Instance.Mai);
        SaveDict("TanInfo", DialogueManager.Instance.Tan);
        SaveDict("MayInfo", DialogueManager.Instance.May);

        PlayerPrefs.SetInt("HasSavedGame", 1);
        PlayerPrefs.Save();
    }

    public static void LoadAll(GameObject player)
    {
        if (PlayerPrefs.GetInt("HasSavedGame", 0) == 0) return;

        // 1. Tải thông số
        GameManager.Instance.daysRemaining = PlayerPrefs.GetInt("SavedDays", 7);
        GameManager.Instance.currentNight = PlayerPrefs.GetInt("CurrentNight", 0);

        // 2. Tải Bằng chứng
        string evData = PlayerPrefs.GetString("SavedEvidence", "");
        EvidenceManager.Instance.collectedEvidence = string.IsNullOrEmpty(evData) ? new List<string>() : new List<string>(evData.Split(','));

        // 3. Tải Profile
        string unData = PlayerPrefs.GetString("SavedUnlocks", "");
        CharacterUnlockManager.unlockedIndices.Clear();
        if (!string.IsNullOrEmpty(unData)) {
            foreach (var s in unData.Split(',')) CharacterUnlockManager.unlockedIndices.Add(int.Parse(s));
        }

        // 4. Tải Vị trí
        if (player != null) {
            player.transform.position = new Vector2(PlayerPrefs.GetFloat("PlayerX"), PlayerPrefs.GetFloat("PlayerY"));
        }

        // 5. TẢI LỜI THOẠI (Rất quan trọng)
        LoadDict("SangInfo", DialogueManager.Instance.Sang);
        LoadDict("MaiInfo", DialogueManager.Instance.Mai);
        LoadDict("TanInfo", DialogueManager.Instance.Tan);
        LoadDict("MayInfo", DialogueManager.Instance.May);
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