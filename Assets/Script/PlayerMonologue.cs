using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerMonologue : MonoBehaviour
{
    public static PlayerMonologue Instance;

    [Header("UI References")]
    public GameObject dialogueBox;
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Player Info")]
    public string playerName = "You";
    public Sprite playerPortrait;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float displayTime = 2f; // Thời gian hiện sau khi gõ xong

    private Coroutine currentCoroutine;
    private HashSet<string> spokenIds = new HashSet<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    /// <summary>
    /// Nói 1 dòng
    /// </summary>
    public void Say(string text, bool onceOnly = false, string id = "")
    {
        if (onceOnly && !string.IsNullOrEmpty(id))
        {
            if (spokenIds.Contains(id)) return;
            spokenIds.Add(id);
        }

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        
        currentCoroutine = StartCoroutine(ShowDialogue(text));
    }

    private IEnumerator ShowDialogue(string text)
    {
        dialogueBox.SetActive(true);
        // Set name và avatar
        if (nameText != null)
            nameText.text = playerName;
        
        if (avatarImage != null && playerPortrait != null)
            avatarImage.sprite = playerPortrait;
            avatarImage.gameObject.SetActive(true);
        dialogueText.text = "";

        // Gõ chữ
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Chờ rồi ẩn
        yield return new WaitForSeconds(displayTime);
        if (dialogueText != null)
            dialogueText.text = "";
        if (nameText != null)
            nameText.text = "";
        
        if (avatarImage != null)
            avatarImage.gameObject.SetActive(false);
        
        dialogueBox.SetActive(false);
    }

    /// <summary>
    /// Check đã nói chưa
    /// </summary>
    public bool HasSpoken(string id)
    {
        return spokenIds.Contains(id);
    }
}