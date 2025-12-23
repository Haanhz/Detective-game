using UnityEngine;
using UnityEngine.UI;

public class UIMenuManager : MonoBehaviour
{
    public static UIMenuManager Instance;

    [Header("Menu")]
    public GameObject menuPanel;

    [Header("Tabs")]
    public GameObject profileContent;
    public GameObject inventoryContent;

    [Header("Buttons")]
    public Button menuButton;       // nút MENU ngoài gameplay
    public Button closeButton;      // nút X trong menu
    public Button profileTabButton;
    public Button inventoryTabButton;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        menuPanel.SetActive(false);

        menuButton.onClick.AddListener(OpenMenu);
        closeButton.onClick.AddListener(CloseMenu);

        profileTabButton.onClick.AddListener(ShowProfile);
        inventoryTabButton.onClick.AddListener(ShowInventory);
    }

    void OpenMenu()
    {
        menuPanel.SetActive(true);
        ShowProfile();      // mở sẵn Profile
        Time.timeScale = 0f;
    }

    void CloseMenu()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
        UIInventoryManager.Instance.CloseInventory();
    }

    void ShowProfile()
    {
        profileContent.SetActive(true);
        inventoryContent.SetActive(false);
        // THÊM DÒNG NÀY: Ép Profile cập nhật dữ liệu ngay khi hiện lên
        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.OnOpenProfile();
        }
    }

    void ShowInventory()
    {
        profileContent.SetActive(false);
        inventoryContent.SetActive(true);
        UIInventoryManager.Instance.OpenInventory();
    }
}
