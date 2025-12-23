using UnityEngine;
using TMPro;

public class UIInventoryManager : MonoBehaviour
{
    public static UIInventoryManager Instance;

    [Header("UI References")]
    public GameObject inventoryPanel;   // InventoryContent (tab)
    public Transform content;           // Content của ScrollView
    public GameObject entryPrefab;       // PrefabEntry

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Được gọi khi mở tab Inventory
    /// </summary>
    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        RefreshUI();
    }

    /// <summary>
    /// Được gọi khi đóng menu hoặc chuyển tab
    /// </summary>
    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    public void RefreshUI()
    {
        if (EvidenceManager.Instance == null) return;

        // Xóa entry cũ
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // Sinh entry mới
        foreach (string evidenceName in EvidenceManager.Instance.collectedEvidence)
        {
            if (evidenceName == "Hide")
                continue;

            GameObject entry = Instantiate(entryPrefab, content);

            TextMeshProUGUI nameText =
                entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();

            TextMeshProUGUI descText =
                entry.transform.Find("DescText").GetComponent<TextMeshProUGUI>();

            nameText.text = evidenceName;
            descText.text = EvidenceManager.Instance.GetEvidenceDescription(evidenceName);
        }
    }
}
