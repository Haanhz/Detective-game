using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class EndingManager : MonoBehaviour
{
    public GameObject EndingBox;
    public TextMeshProUGUI EndingText;
    public Button RestartButton;
    private ChaseManager chase => ChaseManager.instance;

    private GameManager GameEnd => GameManager.Instance;

    private EvidenceManager EvidenceManager => EvidenceManager.Instance;

    private bool HalfEndingTriggered = false;
    private bool FullEndingTriggered = false;

    private DialogueManager DialogueManager => DialogueManager.Instance;

    void Start()
    {
        EndingBox.SetActive(false);
        EndingText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (chase.player.dead || GameEnd.gameEnded) 
        {
            CleanupState();
            ShowEnding(playerDead: true);
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
            checkFullEnding();
            if (!FullEndingTriggered) checkHalfEnding();

            bool chooseRightMurderer = DialogueManager.Instance.ChooseRightMurderer();

            if (FullEndingTriggered && chooseRightMurderer)
            {
                EndingText.text = "Congratulations! You've uncovered the full truth and brought justice to light.";
            }
            else if (FullEndingTriggered && !chooseRightMurderer)
            {
                EndingText.text = "Hmm, you're not much of a detective, are you?";
            }
            else if (HalfEndingTriggered && chooseRightMurderer)
            {
                EndingText.text = "You've found a piece, but the whole truth remains hidden in the shadows.";
            }
            else if (HalfEndingTriggered && !chooseRightMurderer)
            {
                EndingText.text = "Hmm, you're not much of a detective, are you?";
            }
            else
            {
                EndingText.text = "Nobody will ever believe you.";
            }
        }
        EndingBox.SetActive(true);
        EndingText.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void checkHalfEnding()
    {
        string[] halfEndingEvidence = new string[] { "Limit1", "Crack", "OpenWindow", "Rope" };
        //string[] halfEndingEvidence = new string[] { "OpenWindow", "Rope" };
        bool hasEnoughEvidence = true;

        foreach (string evidenceTag in halfEndingEvidence)
        {
            if (!EvidenceManager.Instance.HasEvidence(evidenceTag))
            {
                hasEnoughEvidence = false;
                break;
            }
        }

        bool halfConversation = DialogueManager.Instance.CheckEndingConversation();

        if (hasEnoughEvidence && halfConversation)
        {
            HalfEndingTriggered = true;
        }
    }

    public void checkFullEnding()
    {
        string[] fullEndingEvidence = new string[] { 
            "Limit1", "Limit2", "Limit3", "Limit4", "Limit5", "Limit6", 
            "LivingCorner", "Ultimatum", "HangPhone", "HangNoteBook", 
            "StrangeTable", "OpenWindow", "Rope", "Crack" 
        };
        // string[] fullEndingEvidence = new string[] { 
             
        //     "OpenWindow", "Rope", "Crack"
        // };
        
        bool hasAllEvidence = true;

        foreach (string evidenceTag in fullEndingEvidence)
        {
            if (!EvidenceManager.Instance.HasEvidence(evidenceTag))
            {
                hasAllEvidence = false;
                break;
            }
        }

        bool fullConversation = DialogueManager.Instance.CheckEndingConversation();

        if (hasAllEvidence && fullConversation)
        {
            FullEndingTriggered = true;
        }
    }
}