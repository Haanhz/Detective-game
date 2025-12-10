using UnityEngine;
using TMPro;

public class UIInventoryManager : MonoBehaviour
{
    public static UIInventoryManager Instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform content;
    public GameObject entryPrefab;

    private bool isOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Toggle inventory bằng B
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);

            if (isOpen) RefreshUI();
        }
    }

    public void RefreshUI()
    {
        // Xóa tất cả dòng cũ
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // Sinh lại từng entry
        foreach (string evidenceName in EvidenceManager.Instance.collectedEvidence)
        {
            if (evidenceName == "Hide")
                continue;

            GameObject entry = Instantiate(entryPrefab, content);

            // Tìm các text con
            TextMeshProUGUI nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = entry.transform.Find("DescText").GetComponent<TextMeshProUGUI>();

            nameText.text = evidenceName;
            descText.text = EvidenceManager.Instance.GetEvidenceDescription(evidenceName);
        }
    }
}
