using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventoryManager : MonoBehaviour
{
    public static UIInventoryManager Instance;

    [Header("Top Details")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image bigPortrait;
    public ScrollRect descScrollRect;

    [Header("Bottom Grid")]
    public Transform gridContent;
    public GameObject iconPrefab; // Prefab chỉ gồm 1 Image và 1 Button

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Mặc định ẩn khi bắt đầu game
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Được gọi từ UIMenuManager khi nhấn tab Inventory
    /// </summary>
    public void OpenInventory()
    {
        gameObject.SetActive(true);
        RefreshUI();
    }

    /// <summary>
    /// Được gọi từ UIMenuManager khi chuyển tab hoặc đóng Menu
    /// </summary>
    public void CloseInventory()
    {
        gameObject.SetActive(false);
    }

    public void RefreshUI()
    {
        if (EvidenceManager.Instance == null) return;

        // Xóa icon cũ
        foreach (Transform child in gridContent) 
            Destroy(child.gameObject);

        bool firstItem = true;

        foreach (string evName in EvidenceManager.Instance.collectedEvidence)
        {
            if (evName == "Hide") continue;

            GameObject item = Instantiate(iconPrefab, gridContent);
            
            // Gán Icon từ Database của EvidenceManager
            Image img = item.GetComponent<Image>();
            if (img != null) 
                img.sprite = EvidenceManager.Instance.GetEvidenceSprite(evName);

            // Gán sự kiện Click để hiển thị chi tiết lên nửa trên
            Button btn = item.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => DisplayDetails(evName));

            // Mặc định hiển thị thông tin món đầu tiên để tránh để trống nửa trên
            if (firstItem) {
                DisplayDetails(evName);
                firstItem = false;
            }
        }
    }

    void DisplayDetails(string evName)
    {
        nameText.text = evName;
        descText.text = EvidenceManager.Instance.GetEvidenceDescription(evName);
        bigPortrait.sprite = EvidenceManager.Instance.GetEvidenceBigPortrait(evName);

        // Reset thanh cuộn mô tả về vị trí trên cùng
        Canvas.ForceUpdateCanvases();
        if (descScrollRect != null) 
            descScrollRect.verticalNormalizedPosition = 1f;
    }
}