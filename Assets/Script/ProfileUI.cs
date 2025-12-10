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
        // Toggle bằng phím C
        if (Input.GetKeyDown(KeyCode.C))
        {
            isOpen = !isOpen;
            if (profilePanel != null) profilePanel.SetActive(isOpen);

            if (isOpen) UpdateUI();
        }
    }

    void UpdateUI()
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
        int max = characterNames != null && characterNames.Length > 0 ? characterNames.Length : (characterPortraits != null ? characterPortraits.Length : 0);
        if (max == 0) return;

        index++;
        if (index >= max) index = 0;
        UpdateUI();
    }

    void PrevProfile()
    {
        int max = characterNames != null && characterNames.Length > 0 ? characterNames.Length : (characterPortraits != null ? characterPortraits.Length : 0);
        if (max == 0) return;

        index--;
        if (index < 0) index = max - 1;
        UpdateUI();
    }
}
