using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EvidenceData
{
    public string tagName;
    public Sprite icon;
    public Sprite bigPortrait;
    [TextArea]
    public string description;
}

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager Instance;
    public AudioSource audioSource;
    public AudioClip pickSound;

    [Header("Data Database")]
    public List<EvidenceData> evidenceDatabase = new List<EvidenceData>();
    public List<string> collectedEvidence = new List<string>();
    public List<string> nightlyEvidenceTags = new List<string>();
    public List<string> permanentlyRemovedEvidence = new List<string>();
    public Dictionary<string, float> evidenceWeights = new Dictionary<string, float>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddEvidence(string tagName, float weight)
    {
        if (collectedEvidence.Contains(tagName)) return;
        if (audioSource != null && pickSound != null)
        {
            audioSource.PlayOneShot(pickSound);
        }
        collectedEvidence.Add(tagName);
        evidenceWeights[tagName] = weight;
        if (GameManager.Instance != null && GameManager.Instance.isNight)
        {
            // Chỉ thêm vào danh sách "có thể bị mất" nếu nhặt vào ban đêm
            if (!nightlyEvidenceTags.Contains(tagName)) 
            {
                nightlyEvidenceTags.Add(tagName);
            }
        }
        if (ChaseManager.instance != null) 
        SaveSystem.SaveAll(ChaseManager.instance.player.gameObject);

        Debug.Log($"You found: {tagName} with weight {weight}");

        // In list
        string listStr = "Collected Evidence: ";
        foreach (var ev in collectedEvidence)
            listStr += ev + ", ";

        Debug.Log(listStr);

        // Tính tổng weight
        float totalWeight = CalculateTotalWeight();
        Debug.Log("Total Weight = " + totalWeight);
    }

    public float CalculateTotalWeight()
    {
        float total = 0f;
        foreach (var kv in evidenceWeights)
            total += kv.Value;

        return total;
    }

    // Hàm để xóa dữ liệu buổi đêm khi Replay
    public void RevertNightlyEvidence()
    {
        // DANH SÁCH MIỄN TRỪ: Những tag này sẽ KHÔNG bị mất khi chết
        HashSet<string> exemptTags = new HashSet<string> { 
            "LivingCorner", "Crack", "StrangeTable", "OpenWindow", "Rope" 
        };
        Debug.Log($"[RevertNightly] Before revert - Nightly: {nightlyEvidenceTags.Count}, Permanent: {permanentlyRemovedEvidence.Count}");
        
        foreach (string tag in nightlyEvidenceTags)
        {
            // Nếu thuộc danh sách miễn trừ -> Bỏ qua, giữ nguyên trong Inventory
            if (exemptTags.Contains(tag)) 
            {
                Debug.Log($"[RevertNightly] Exempted: {tag} (Stays in inventory)");
                continue; 
            }
            // ← THÊM: Thêm vào danh sách permanently removed
            if (!permanentlyRemovedEvidence.Contains(tag))
            {
                permanentlyRemovedEvidence.Add(tag);
                Debug.Log($"[RevertNightly] Added to permanent removal: {tag}");
            }
            
            // Xóa khỏi danh sách chính để SaveSystem.SaveAll không lưu nó lại
            if (collectedEvidence.Contains(tag))
                collectedEvidence.Remove(tag);
            
            // Xóa trọng số để tránh cộng dồn sai
            if (evidenceWeights.ContainsKey(tag))
                evidenceWeights.Remove(tag);
        }
        
        // Sau khi xóa xong phải làm trống danh sách tạm
        nightlyEvidenceTags.Clear(); 
        Debug.Log($"[RevertNightly] After revert - Permanent: {permanentlyRemovedEvidence.Count}");
        Debug.Log($"[RevertNightly] Items permanently removed: {string.Join(", ", permanentlyRemovedEvidence)}");
    }

    public void CleanUpPermanentlyRemovedEvidence()
    {
        Evidence[] allItems = Object.FindObjectsByType<Evidence>(FindObjectsSortMode.None);

        foreach (Evidence item in allItems)
        {
            // Kiểm tra xem evidence này có trong permanentlyRemovedEvidence không
            if (permanentlyRemovedEvidence.Contains(item.evidenceTag))
            {
                // Sử dụng Reflection để gọi hàm ShouldHide
                var method = item.GetType().GetMethod("ShouldHide", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (method != null)
                {
                    bool shouldHide = (bool)method.Invoke(item, new object[] { item.evidenceTag });
                    
                    if (shouldHide)
                    {
                        // Nếu ShouldHide = true → DESTROY (Limit1, HangNoteBook, SangStuff...)
                        Debug.Log($"[CleanUpPermanent] Destroying (ShouldHide=true): {item.evidenceTag}");
                        Destroy(item.gameObject);
                    }
                    else
                    {
                        // Nếu ShouldHide = false → LOCK (LivingCorner, Ultimatum...)
                        // Khóa biến 'collected' = true để không thể nhặt lại
                        var field = item.GetType().GetField("collected", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field != null)
                        {
                            field.SetValue(item, true);
                            Debug.Log($"[CleanUpPermanent] Locked (ShouldHide=false): {item.evidenceTag}");
                        }
                    }
                }
            }
        }
    }

    public Dictionary<string, string> evidenceDescriptions = new Dictionary<string, string>()
    {
        { "HangPhone", "There’s a new message from Tấn that hasn’t been read: Why did you let May look at the document but not me? Do you hate me that much?" },
        { "LivingCorner", "The living corner in the shed includes: an old blanket, a pillow, a torn food bag, a water bottle, and a piece of clothing." },
        { "Ultimatum", "YOU ARE BEING WATCHED" },
        { "HangNoteBook", "Page 1: It seems the house has been broken into, Page 2: Mai knows the house too well, is she hiding something?" },
        { "Limit1", "Footprint in the attic, beneath the open window." },
        { "Limit2", "Hang the Ghost:the intruder... the one....10 years ago.....why me?" },
        { "Crack", "Why there is crack outside this room?"},
        {  "StrangeTable", "Four chairs around the table, three of them fall, one of them stand."},
        { "OpenWindow", "The attic is locked for long time, who open the window? It is opened all the time?"},
        { "Rope", "No rope at the scene, but rope in the attic..."},
        { "Limit3", "Mai's Diary: It’s definitely her—there’s no way I could be mistaken… I can’t believe she’s still alive. But why doesn’t she recognize me? Could it be that she’s hiding something… or doesn’t want to talk about the past? Poor thing. Back then, she had to grow up in a family like that… Maybe I should keep quiet too. I promise I won’t ever leave you alone again."},
        {"Limit4", "Photo of a family of four. There is a little girl...so familiar...wait, is that May???"},
        {"Limit5", "That past—I don’t want to remember it, no, I didn’t do that, I didn’t mean to, but Dad, Mom and my sister, Mom was wrong, Dad was wrong too, AAAAAAAAAAAAA, Hang… she has to d..."},
        {"Limit6", "My child… she is not evil……She was gentle… once…But her mind……was broken.…Not by strangers…Not by ghosts…By the man… she called father...Love became fear…Fear became obedience…She guards the house…Because she believes…If the truth escapes……she deserves to disappear"}

    };

    // Hàm lấy Sprite theo tên
    public Sprite GetEvidenceSprite(string name)
    {
        EvidenceData data = evidenceDatabase.Find(x => x.tagName == name);
        return data != null ? data.icon : null;
    }

    public Sprite GetEvidenceBigPortrait(string name)
    {
        EvidenceData data = evidenceDatabase.Find(x => x.tagName == name);
        return data != null ? data.bigPortrait : null;
    }

    // Cập nhật hàm lấy Description từ database mới (tốt hơn dùng Dictionary thủ công)
    public string GetEvidenceDescription(string name)
    {
        EvidenceData data = evidenceDatabase.Find(x => x.tagName == name);
        return data != null ? data.description : "No description available.";
    }

    public bool HasEvidence(string tagName)
    {
        return collectedEvidence.Contains(tagName);
    }


    // Thêm hàm này vào EvidenceManager.cs
    public void LockCollectedItemsInScene()
    {
        // 1. Tìm tất cả các script Evidence đang có mặt trong cảnh
        Evidence[] allItems = Object.FindObjectsByType<Evidence>(FindObjectsSortMode.None);

        foreach (Evidence item in allItems)
        {
            // 2. Kiểm tra xem Tag của vật phẩm này đã nằm trong danh sách đã nhặt chưa
            if (collectedEvidence.Contains(item.evidenceTag))
            {
                // 3. Sử dụng Reflection hoặc truy cập trực tiếp để khóa biến 'collected'
                // Việc này chặn người chơi nhấn "F" nhưng giữ nguyên Collider và Tag
                var field = item.GetType().GetField("collected", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null) field.SetValue(item, true);
                
                Debug.Log($"[System] Đã khóa tương tác vật phẩm: {item.evidenceTag}. Collider vẫn giữ nguyên.");
            }
        }
    }

    public void CleanUpCollectedItemsInScene()
    {
        // Tìm tất cả các object có gắn script Evidence trong Scene
        Evidence[] allItems = Object.FindObjectsByType<Evidence>(FindObjectsSortMode.None);

        foreach (Evidence item in allItems)
        {
            // Kiểm tra xem món này đã có trong danh sách đã nhặt chưa
            if (collectedEvidence.Contains(item.evidenceTag))
            {
                // Sử dụng Reflection để gọi hàm ShouldHide từ script Evidence của vật phẩm đó
                var method = item.GetType().GetMethod("ShouldHide", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (method != null)
                {
                    // Thực thi hàm ShouldHide(item.evidenceTag)
                    bool shouldHide = (bool)method.Invoke(item, new object[] { item.evidenceTag });
                    
                    if (shouldHide)
                    {
                        // Nếu ShouldHide trả về true (như HangNoteBook, Limit1...) -> Xóa hẳn
                        Destroy(item.gameObject);
                    }
                    else
                    {
                        // Nếu trả về false (như LivingCorner) -> Giữ nguyên nhưng khóa nhặt
                        // Chúng ta ép biến 'collected' thành true để hàm Update của Evidence không chạy
                        var field = item.GetType().GetField("collected", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field != null) field.SetValue(item, true);
                    }
                }
            }
        }
    }
}
