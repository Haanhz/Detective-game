using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI dayRemainText;

    // Stamina UI
    public Slider staminaSlider;
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color lowColor = Color.red;
    private Image staminaFill;
    private bool isFlashing = false;

    // Lose + Replay
    //public TextMeshProUGUI loseText;
    public Button replayButton;

    private ChaseManager chase => ChaseManager.instance;
    private GameManager gm => GameManager.Instance;

    void Start()
    {
        staminaFill = staminaSlider.fillRect.GetComponent<Image>();

        // Ẩn Lose + Replay lúc đầu
        //if (loseText != null) loseText.gameObject.SetActive(false);
        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(false);
            replayButton.onClick.AddListener(ReplayScene);
        }
    }

    void Update()
    {
        UpdateDayRemain();
        UpdateStamina();
        CheckPlayerDeath();
    }

    // Hiển thị số ngày còn lại
    void UpdateDayRemain()
    {
        dayRemainText.text = $"days remain: {gm.daysRemaining}";
    }

    // Stamina UI
    void UpdateStamina()
    {
        staminaSlider.maxValue = chase.player.maxStamina;
        staminaSlider.value = chase.player.currentStamina;

        float pct = chase.player.currentStamina / chase.player.maxStamina;

        if (pct > 0.6f)
            staminaFill.color = fullColor;
        else if (pct > 0.3f)
            staminaFill.color = midColor;
        else
            staminaFill.color = lowColor;

        if (pct < 0.2f && !isFlashing)
            StartCoroutine(FlashStaminaBar());
    }

    IEnumerator FlashStaminaBar()
    {
        isFlashing = true;
        for (int i = 0; i < 6; i++)
        {
            staminaFill.enabled = false;
            yield return new WaitForSeconds(0.15f);
            staminaFill.enabled = true;
            yield return new WaitForSeconds(0.15f);
        }
        isFlashing = false;
    }

    // Khi player chết
    void CheckPlayerDeath()
    {
        if (chase.player.dead)
        {
            // if (loseText != null) loseText.gameObject.SetActive(true);
            if (replayButton != null) replayButton.gameObject.SetActive(true);
        }
    }

    // Replay scene
    void ReplayScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
}
