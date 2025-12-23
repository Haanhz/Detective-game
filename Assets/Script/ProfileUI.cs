using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileUI : MonoBehaviour
{
    // Singleton
    public static ProfileUI Instance;

    [Header("UI References")]
    public GameObject profilePanel;     // Panel tổng
    public TextMeshProUGUI nameText;    // Tên nhân vật
    public TextMeshProUGUI descText;    // Mô tả
    public Image portraitImage;         // Ảnh chân dung
    public Button nextBtn;              // Nút tới
    public Button prevBtn;              // Nút lùi

    [Header("Profiles")]
    public Sprite[] characterPortraits; // mảng portrait (index tương ứng)
    [TextArea(2, 4)]
    public string[] characterNames;     // tên các NV
    [TextArea(4, 8)]
    public string[] characterDescriptions;  // mô tả NV

    private int index = 0;
    private bool isOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (profilePanel != null)
            profilePanel.SetActive(false);
    }

    void Start()
    {
        // Gán event cho nút (null-safe)
        if (nextBtn != null) nextBtn.onClick.AddListener(NextProfile);
        if (prevBtn != null) prevBtn.onClick.AddListener(PrevProfile);
    }

    void Update()
    {
        
    }

    public void UpdateUI()
    {
        // Safety checks to avoid IndexOutOfRange / null refs
        if (characterNames == null || characterNames.Length == 0)
        {
            if (nameText != null) nameText.text = "No name";
        }
        else
        {
            if (nameText != null)
                nameText.text = characterNames[Mathf.Clamp(index, 0, characterNames.Length - 1)];
        }

        if (characterDescriptions == null || characterDescriptions.Length == 0)
        {
            if (descText != null) descText.text = "";
        }
        else
        {
            if (descText != null)
                descText.text = characterDescriptions[Mathf.Clamp(index, 0, characterDescriptions.Length - 1)];
        }

        // Portrait (optional)
        if (portraitImage != null && characterPortraits != null && characterPortraits.Length > 0)
        {
            if (index >= 0 && index < characterPortraits.Length && characterPortraits[index] != null)
            {
                portraitImage.sprite = characterPortraits[index];
                portraitImage.enabled = true;
            }
            else
            {
                // nếu không có sprite tương ứng thì ẩn image hoặc để null
                portraitImage.sprite = null;
                portraitImage.enabled = false;
            }
        }
    }

    void NextProfile()
    {
        // Giữ nguyên cách lấy 'max' an toàn của bạn
        int max = characterNames != null && characterNames.Length > 0 ? characterNames.Length : (characterPortraits != null ? characterPortraits.Length : 0);
        if (max <= 1) return; // Nếu có 0 hoặc 1 nhân vật thì không cần chuyển

        int startSearchIndex = index; // Lưu lại điểm bắt đầu để tránh lặp vô tận
        do {
            index++;
            if (index >= max) index = 0;

            // Kiểm tra xem đã mở khóa chưa (CharacterUnlockManager là script tôi bảo bạn tạo thêm)
            if (CharacterUnlockManager.IsUnlocked(index)) {
                UpdateUI();
                return;
            }
        } while (index != startSearchIndex); 
    }

    void PrevProfile()
    {
        int max = characterNames != null && characterNames.Length > 0 ? characterNames.Length : (characterPortraits != null ? characterPortraits.Length : 0);
        if (max <= 1) return;

        int startSearchIndex = index;
        do {
            index--;
            if (index < 0) index = max - 1;

            if (CharacterUnlockManager.IsUnlocked(index)) {
                UpdateUI();
                return;
            }
        } while (index != startSearchIndex);
    }

    // Thêm hàm này vào ProfileUI.cs
    public void OnOpenProfile()
    {
        int max = characterNames != null ? characterNames.Length : 0;
        if (max == 0) return;

        // Tìm nhân vật đầu tiên đã được mở khóa để hiển thị ngay khi mở menu
        bool found = false;
        for (int i = 0; i < max; i++)
        {
            if (CharacterUnlockManager.IsUnlocked(i))
            {
                index = i;
                found = true;
                break;
            }
        }

        if (found)
        {
            UpdateUI();
        }
        else
        {
            // Nếu chưa gặp bất kỳ ai, hiển thị trạng thái trống
            nameText.text = "???";
            descText.text = "You didn't meet anyone yet.";
            if (portraitImage != null) portraitImage.enabled = false;
        }
    }
}
