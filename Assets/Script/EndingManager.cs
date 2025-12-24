using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class EndingManager : MonoBehaviour
{
    public GameObject EndingBox;
    public TextMeshProUGUI EndingText;
    public Button RestartButton;
    private ChaseManager chase => ChaseManager.instance;

    private GameManager GameEnd => GameManager.Instance;

    private EvidenceManager EvidenceManager => EvidenceManager.Instance;

    private bool HalEvidence = false;
    private bool HalfConversation = false;
    private bool HalfEndingTriggered = false;
    private bool FullEvidence = false;
    private bool FullConversation = false;
    private bool FullEndingTriggered = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EndingBox.SetActive(false);
        EndingText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (chase.player.dead || GameEnd.gameEnded) 
    {
        // Cleanup tất cả dialogue UI
        CleanupState();

        // Show ending cho dead
        ShowEnding(playerDead: true);

        // Optional: dừng Update để không gọi nhiều lần
        enabled = false;
    }
    }
    
    public void CleanupState()
    {
        DialogueManager.Instance.dialogueBox.SetActive(false);
        DialogueManager.Instance.StartConversationButton.SetActive(false);
        DialogueManager.Instance.PointMurderButton.SetActive(false);
        DialogueManager.Instance.DialogueText.text = string.Empty;
        DialogueManager.Instance.NameText.text = string.Empty;
        DialogueManager.Instance.AvatarImage.gameObject.SetActive(false);
        
    }

    public void ShowEnding(bool playerDead = false)
    {
        if (playerDead)
        {
            EndingText.text = "You died.";
        }
        else
        {
            float totalWeight = EvidenceManager.Instance.CalculateTotalWeight();
            bool chooseRightMurderer = DialogueManager.Instance.ChooseRightMurderer();
        if (totalWeight <= 9 && chooseRightMurderer)
        {
            EndingText.text = "No one believes you.";
        }
        else if ((totalWeight <= 9 || totalWeight > 9 ) && !chooseRightMurderer)
        {
            EndingText.text = "You made a mistake.";
        }
        else if (totalWeight > 9 && chooseRightMurderer)
        {
            EndingText.text = "You are right, but the whole truth is still waiting for you.";
        }}
        EndingBox.SetActive(true);
        EndingText.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);
        Time.timeScale = 0f;

    }
    public void checkHalfEnding()
    {
        
        string[] halfEndingEvidence = new string[] { "Limit1", "Crack", "OpenWindow", "Rope" };
        string[] halfEndingConversation = new string[] {""};
        foreach (string evidenceTag in halfEndingEvidence)
        {
            if (!EvidenceManager.Instance.HasEvidence(evidenceTag))
            {
                Debug.Log("Chưa đủ evidence để trigger Half Ending. Thiếu: " + evidenceTag);
                HalEvidence = false;
                return;
            }
        }
        HalEvidence = true;
    }

    public void checkFullEnding()
    {
        
        string[] fullEndingEvidence = new string[] {"Limit1", "Limit2", "Limit3", "Limit4", "Limit5", " Limit6", "LivingCorner", "Ultimatum", "HangPhone", "HangNoteBook", "StrangeTable", "OpenWindow", "Rope", "Crack" };
        string[] fullEndingConversation = new string[] {""}; 
        foreach (string evidenceTag in fullEndingEvidence)
        {
            if (!EvidenceManager.Instance.HasEvidence(evidenceTag))
            {
                Debug.Log("Chưa đủ evidence để trigger Full Ending. Thiếu: " + evidenceTag);
                FullEvidence = false;
                return;
            }
        }
        FullEvidence = true;
    }
}
    