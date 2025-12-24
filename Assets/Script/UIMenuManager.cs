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
    public GameObject noteContent; // THÊM MỚI: Content cho tab Note

    [Header("Buttons")]
    public Button menuButton;
    public Button closeButton;
    public Button profileTabButton;
    public Button inventoryTabButton;
    public Button noteTabButton;    // THÊM MỚI: Nút chuyển sang tab Note

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
        noteTabButton.onClick.AddListener(ShowNote); // THÊM MỚI: Lắng nghe sự kiện nút Note
    }

    void OpenMenu()
    {
        menuPanel.SetActive(true);
        ShowProfile(); // Mặc định mở Profile
        Time.timeScale = 0f;
    }

    void CloseMenu()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
        if(UIInventoryManager.Instance != null) UIInventoryManager.Instance.CloseInventory();
    }

    void ShowProfile()
    {
        profileContent.SetActive(true);
        inventoryContent.SetActive(false);
        noteContent.SetActive(false); // Ẩn tab Note

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.OnOpenProfile();
        }
    }

    void ShowInventory()
    {
        profileContent.SetActive(false);
        inventoryContent.SetActive(true);
        noteContent.SetActive(false); // Ẩn tab Note
        
        if(UIInventoryManager.Instance != null) UIInventoryManager.Instance.OpenInventory();
    }

    // THÊM MỚI: Hàm hiển thị Tab Note
    void ShowNote()
    {
        profileContent.SetActive(false);
        inventoryContent.SetActive(false);
        noteContent.SetActive(true);  // Hiện tab Note

        // Nếu bạn có Script quản lý Note (ví dụ NoteUI), hãy gọi hàm cập nhật tại đây tương tự Profile
        // if (NoteUI.Instance != null) NoteUI.Instance.UpdateNoteList();
    }
}