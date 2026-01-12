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
    public GameObject noteNotiImage;

    [Header("Buttons")]
    public Button menuButton;
    public Button closeButton;
    public Button profileTabButton;
    public Button inventoryTabButton;
    public Button noteTabButton;    // THÊM MỚI: Nút chuyển sang tab Note

    [Header("Extra Buttons")]
    public Button backToTitleButton;
    private bool hasReadNote = false;

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

        if (backToTitleButton != null)
        backToTitleButton.onClick.AddListener(ReturnToStartMenu);
        // THÊM ĐOẠN NÀY
        if (noteNotiImage != null)
        {
            hasReadNote = PlayerPrefs.GetInt("HasReadNote", 0) == 1;
            noteNotiImage.SetActive(!hasReadNote);
        }
    }

    void OpenMenu()
    {
        menuPanel.SetActive(true);
        ShowInventory(); // Mặc định mở Profile
    }

    void CloseMenu()
    {
        menuPanel.SetActive(false);
        if(UIInventoryManager.Instance != null) UIInventoryManager.Instance.CloseInventory();
    }

    void ShowProfile()
    {
        profileContent.SetActive(true);
        inventoryContent.SetActive(false);
        noteContent.SetActive(false); // Ẩn tab Note

        if (UIInventoryManager.Instance != null) UIInventoryManager.Instance.CloseInventory();

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
        noteContent.SetActive(true);

        if (UIInventoryManager.Instance != null) UIInventoryManager.Instance.CloseInventory();
        
        // THÊM ĐOẠN NÀY
        if (!hasReadNote)
        {
            hasReadNote = true;
            PlayerPrefs.SetInt("HasReadNote", 1);
            PlayerPrefs.Save();
            
            if (noteNotiImage != null)
                noteNotiImage.SetActive(false);
        }
    }
    public void ReturnToStartMenu()
    {
        // LƯU TRƯỚC KHI THOÁT
        SaveSystem.SaveAll(ChaseManager.instance.player.gameObject);

        menuPanel.SetActive(false);
        if (UIManager.Instance != null) {
            UIManager.Instance.ShowStartMenuCustom();
            UIManager.Instance.startPanel.SetActive(true);
            UIManager.Instance.dayRemainText.gameObject.SetActive(false);
            UIManager.Instance.staminaSlider.gameObject.SetActive(false);
            UIManager.Instance.menuButtonObject.SetActive(false);
        }
        Time.timeScale = 0f;
    }
}