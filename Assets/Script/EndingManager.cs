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

    private bool HalfEvidence = false;
    private bool HalfConversation = false;
    private bool HalfEndingTriggered = false;
    private bool FullEvidence = false;
    private bool FullConversation = false;
    private bool FullEndingTriggered = false;

    private DialogueManager DialogueManager => DialogueManager.Instance;


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
            checkHalfEnding();
            checkFullEnding();
            bool chooseRightMurderer = DialogueManager.Instance.ChooseRightMurderer();
            if (!HalfEndingTriggered && !FullEndingTriggered && (chooseRightMurderer || !chooseRightMurderer))
            {
                EndingText.text = "Nobody will ever believe you.";
            }
            else if (HalfEndingTriggered && !FullEndingTriggered && !chooseRightMurderer)
            {
                EndingText.text = "Hmm, you're not much of a detective, are you?";
            }
            else if (HalfEndingTriggered && !FullEndingTriggered && chooseRightMurderer)
            {
                EndingText.text = "You've found a piece, but the whole truth remains hidden in the shadows.";
            }
            else if (FullEndingTriggered && chooseRightMurderer)
            {
                EndingText.text = "Congratulations! You've uncovered the full truth and brought justice to light.";
            }
            else if (FullEndingTriggered && !chooseRightMurderer)
            {
                EndingText.text = "Hmm, you're not much of a detective, are you?";
            }
        }
        EndingBox.SetActive(true);
        EndingText.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);
        Time.timeScale = 0f;

    }
    public void checkHalfEnding()
    {
        //string[] halfEndingEvidence = new string[] { "Limit1", "Crack", "OpenWindow", "Rope" };
        string[] halfEndingEvidence = new string[] { "OpenWindow", "Rope" };
        foreach (string evidenceTag in halfEndingEvidence)
        {
            if (!EvidenceManager.Instance.HasEvidence(evidenceTag))
            {
                Debug.Log("Chưa đủ evidence để trigger Half Ending. Thiếu: " + evidenceTag);
                HalfEvidence = false;
                return;
            }
        }
        HalfEvidence = true;
        HalfConversation = DialogueManager.Instance.CheckEndingConversation();
        if (HalfEvidence && HalfConversation)
        {
            HalfEndingTriggered = true;
            Debug.Log("Half Ending đã đủ điều kiện trigger!");
        }

    }

    public void checkFullEnding()
    {
        
        string[] fullEndingEvidence = new string[] {"Limit1", "Limit2", "Limit3", "Limit4", "Limit5", " Limit6", "LivingCorner", "Ultimatum", "HangPhone", "HangNoteBook", "StrangeTable", "OpenWindow", "Rope", "Crack" };
        
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
        FullConversation = DialogueManager.Instance.CheckEndingConversation();
        if (FullEvidence && FullConversation)
        {
            FullEndingTriggered = true;
            Debug.Log("Full Ending đã đủ điều kiện trigger!");
        }
    }
}
    