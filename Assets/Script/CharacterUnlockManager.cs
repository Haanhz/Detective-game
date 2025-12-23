using System.Collections.Generic;
using UnityEngine;

public static class CharacterUnlockManager
{
    // Lưu trữ Index của những nhân vật đã mở khóa
    public static HashSet<int> unlockedIndices = new HashSet<int>();

    public static void UnlockCharacter(int index)
    {
        if (!unlockedIndices.Contains(index))
        {
            unlockedIndices.Add(index);
            Debug.Log("Đã mở khóa Profile cho nhân vật số: " + index);
        }
    }

    public static bool IsUnlocked(int index)
    {
        return unlockedIndices.Contains(index);
    }
}