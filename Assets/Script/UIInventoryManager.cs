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
        // Toggle inventory bằng I hoặc Tab
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
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

        // Tạo lại từng dòng
        foreach (string evidenceName in EvidenceManager.Instance.collectedEvidence)
        {
            if(evidenceName == "Hide") 
                continue;
            GameObject entry = Instantiate(entryPrefab, content);
            entry.GetComponent<TextMeshProUGUI>().text = evidenceName;
        }
    }
}
