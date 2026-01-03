using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIInventoryManager : MonoBehaviour
{
    public static UIInventoryManager Instance;

    [Header("Panel Reference")]
    public GameObject inventoryPanel;

    [Header("Top Details")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image bigPortrait;
    public ScrollRect descScrollRect;

    [Header("Bottom Grid")]
    public Transform gridContent;
    public GameObject iconPrefab; // Prefab chỉ gồm 1 Image và 1 Button

    private List<GameObject> spawnedIcons = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Mặc định ẩn khi bắt đầu game
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    /// <summary>
    /// Được gọi từ UIMenuManager khi nhấn tab Inventory
    /// </summary>
    public void OpenInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        RefreshUI();
    }

    /// <summary>
    /// Được gọi từ UIMenuManager khi chuyển tab hoặc đóng Menu
    /// </summary>
    public void CloseInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    public void RefreshUI()
    {
        if (EvidenceManager.Instance == null) return;

        // 1. Xóa sạch icon cũ và danh sách quản lý
        foreach (Transform child in gridContent) Destroy(child.gameObject);
        spawnedIcons.Clear();

        // KIỂM TRA TRỐNG: Nếu chưa có bằng chứng nào
        if (EvidenceManager.Instance.collectedEvidence == null || EvidenceManager.Instance.collectedEvidence.Count == 0)
        {
            nameText.text = "???";
            descText.text = "You didn't get anything yet.";
            if (bigPortrait != null) bigPortrait.enabled = false;
            return; // Thoát hàm sớm, không chạy tiếp logic sinh icon
        }

        bool firstItem = true;

        foreach (string evName in EvidenceManager.Instance.collectedEvidence)
        {
            if (evName == "Hide") continue;

            // 2. Tạo ô vật phẩm
            GameObject item = Instantiate(iconPrefab, gridContent);
            spawnedIcons.Add(item);

            // Đảm bảo bật lại ảnh chân dung lớn nếu trước đó nó bị tắt
            if (bigPortrait != null) bigPortrait.enabled = true;

            // 3. Tìm chính xác Component Image của Icon
            Transform iconTransform = item.transform.Find("Icon");
            if (iconTransform != null)
            {
                iconTransform.GetComponent<Image>().sprite = EvidenceManager.Instance.GetEvidenceSprite(evName);
            }

            // 4. Mặc định tắt Highlight
            Transform highlight = item.transform.Find("HighlightFrame");
            if (highlight != null) highlight.gameObject.SetActive(false);

            // 5. Gán sự kiện Click
            Button btn = item.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => DisplayDetails(evName, item));

            if (firstItem)
            {
                DisplayDetails(evName, item);
                firstItem = false;
            }
        }
    }

    void DisplayDetails(string evName, GameObject selectedItem)
    {
        // Cập nhật thông tin phía trên
        nameText.text = GetEvidenceName(evName);
        descText.text = EvidenceManager.Instance.GetEvidenceDescription(evName);
        bigPortrait.sprite = EvidenceManager.Instance.GetEvidenceBigPortrait(evName);

        // XỬ LÝ HIGHLIGHT: Tắt tất cả các ô khác, chỉ bật ô được chọn
        foreach (GameObject iconObj in spawnedIcons)
        {
            Transform h = iconObj.transform.Find("HighlightFrame");
            if (h != null) h.gameObject.SetActive(false);
        }

        Transform selectedH = selectedItem.transform.Find("HighlightFrame");
        if (selectedH != null) selectedH.gameObject.SetActive(true);

        // Reset Scroll
        Canvas.ForceUpdateCanvases();
        if (descScrollRect != null) descScrollRect.verticalNormalizedPosition = 1f;
    }

        string GetEvidenceName(string evidenceTag)
    {
        switch (evidenceTag)
        {
            case "LivingCorner": return "Living Corner";
            case "Ultimatum": return "Ultimatum";
            case "HangPhone": return "Hang's Phone";
            case "HangNoteBook": return "Hang's Notebook";
            case "Crack": return "Crack outside the attic";
            case "StrangeTable": return "Four chairs around the table, three of them fall";
            case "OpenWindow": return "Open window in the attic";
            case "Rope": return "Rope in the attic";
            case "Limit1": return "Footprint in the attic";
            case "Limit2": return "Hang the Ghost";
            case "Limit3": return "Mai's Diary";
            case "Limit4": return "Family Photo";
            case "Limit5": return "May's Diary";
            case "Limit6": return "The ghost mom";
            case "Hide": return "Hide";
            case "SangStuff": return "Mr.Sang precious duck";
            default: return "Unknown Evidence";
        }
    }
}