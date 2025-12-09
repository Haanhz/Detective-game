using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class EndingManager : MonoBehaviour
{
    public GameObject EndingBox;
    public TextMeshProUGUI EndingText;
    public Button RestartButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EndingBox.SetActive(false);
        EndingText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void ShowEnding()
    {
        float totalWeight = EvidenceManager.Instance.CalculateTotalWeight();
        bool chooseRightMurderer = DialogueManager.Instance.ChooseRightMurderer();
        if (totalWeight <= 9 && chooseRightMurderer)
        {
            EndingText.text = "You are right, but no one believes you.";
        }
        else if ((totalWeight <= 9 || totalWeight > 9 ) && !chooseRightMurderer)
        {
            EndingText.text = "You made a mistake.";
        }
        else if (totalWeight > 9 && chooseRightMurderer)
        {
            EndingText.text = "You are right, but the whole truth is still waiting for you.";
        }
        EndingBox.SetActive(true);
        EndingText.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);
      

    }
}
