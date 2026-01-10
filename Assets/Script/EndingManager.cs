using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Video;
using NUnit.Framework;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }
    
    [Header("UI References")]
    public Canvas mainGameCanvas;   
    public Canvas ui;
    public GameObject EndingBox;
    public TextMeshProUGUI EndingText;
    public Button RestartButton;

    [Header("Cutscene Settings")]
    public VideoPlayer videoPlayer; 
    public VideoClip killedEndingClip;  
    public VideoClip exhaustedEndingClip;   
    public VideoClip fullEndingClip; 
    public VideoClip halfEndingClip; 
    public VideoClip WrongEndingClip;  
    public VideoClip NobodyEndingClip;

    private ChaseManager chase => ChaseManager.instance;
    private GameManager GameEnd => GameManager.Instance;

    private EvidenceManager EvidenceManager => EvidenceManager.Instance;
    private DialogueManager DialogueManager => DialogueManager.Instance;

    private bool HalfEndingTriggered = false;
    private bool FullEndingTriggered = false;
    // WrongEndingTriggered đã xóa - CaseFileUI tự handle
    
    private bool endingStarted = false;
    private bool isTimeOut = false;

    public static bool IsKilledByBlack = false;
    public static bool IsDetectiveEnding = false;

    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        EndingBox.SetActive(false);
        EndingText.gameObject.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.clip = null;
        }
    }

    void Update()
    {
        if (endingStarted) return;
        if (GameManager.Instance.daysRemaining <= 0)
        {
            isTimeOut = true;
            endingStarted = true;
            CleanupState();
            ShowEnding(playerDead: isTimeOut);
        } 
        else 
        {
            if(chase.player.killed) 
            {
                endingStarted = true;
                CleanupState();
                ShowEnding(playerDead: chase.player.killed);
            } 
            else if (chase.player.exhausted)
            {
                endingStarted = true;
                CleanupState();
                ShowEnding(playerDead: chase.player.exhausted);
            }
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

    // ===== MAIN ENDING METHOD =====
    public void ShowEnding(bool playerDead = false)
    {
        IsDetectiveEnding = false;
        VideoClip selectedClip = null;
        string resultText = "";

        if (playerDead) 
        {
            if (chase.player.killed) IsKilledByBlack = true;
        }
        else 
        {
            IsDetectiveEnding = true;
        }

        if (playerDead && chase.player.killed)
        {
            IsKilledByBlack = true;
            resultText = "You died in the chase! The killer took all the items you collected during the night!";
            selectedClip = killedEndingClip;
        }
        else if (playerDead && chase.player.exhausted)
        {
            resultText = "You collapsed from exhaustion!";
            selectedClip = exhaustedEndingClip;
        }
        else if (playerDead && isTimeOut)
        {
            resultText = "Out of time. Now you are the next victim";
            selectedClip = killedEndingClip;
        }
        else
        {
            // Chỉ check FULL và HALF thôi (WRONG và NOBODY đã được handle bởi CaseFileUI)
            checkFullEnding();
            if (!FullEndingTriggered) checkHalfEnding();

            bool chooseRightMurderer = DialogueManager.Instance.ChooseRightMurderer();

            if (FullEndingTriggered && chooseRightMurderer)
            {
                resultText = "Congratulations! You've uncovered the full truth.";
                selectedClip = fullEndingClip;
            }
            else if (HalfEndingTriggered && chooseRightMurderer)
            {
                resultText = "You've found a piece, but the whole truth remains hidden.";
                selectedClip = halfEndingClip;
            }
            else 
            {
                // Fallback - không đủ evidence cho HALF
                resultText = "Nobody will ever believe you.";
                selectedClip = NobodyEndingClip;
            }
        }

        EndingText.text = resultText;

        if (selectedClip != null && videoPlayer != null)
        {
            StartCoroutine(PlayVideoThenShowUI(selectedClip, playerDead)); 
        }
        else
        {
            DisplayEndingUI(playerDead); 
        }
    }

    // ===== FORCED ENDING METHODS (Called from CaseFileUI) =====
    
    public void ShowWrongEnding()
    {
        IsDetectiveEnding = true;
        endingStarted = true;
        
        string resultText = "Hmm...you're not much of a detective, are you?";
        VideoClip selectedClip = WrongEndingClip;

        EndingText.text = resultText;

        if (selectedClip != null && videoPlayer != null)
        {
            StartCoroutine(PlayVideoThenShowUI(selectedClip, false));
        }
        else
        {
            DisplayEndingUI(false);
        }
    }

    public void ShowNobodyEnding()
    {
        IsDetectiveEnding = true;
        endingStarted = true;
        
        string resultText = "Nobody will ever believe you.";
        VideoClip selectedClip = NobodyEndingClip;

        EndingText.text = resultText;

        if (selectedClip != null && videoPlayer != null)
        {
            StartCoroutine(PlayVideoThenShowUI(selectedClip, false));
        }
        else
        {
            DisplayEndingUI(false);
        }
    }

    // ===== VIDEO & UI HANDLING =====
    
    private System.Collections.IEnumerator PlayVideoThenShowUI(VideoClip clip, bool pauseGame)
    {
        MuteAllGameAudio(true);

        if (mainGameCanvas != null) mainGameCanvas.enabled = false;
        if (ui != null) ui.enabled = false;

        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        while (videoPlayer.isPlaying)
            yield return null;

        videoPlayer.Stop();

        if (mainGameCanvas != null) mainGameCanvas.enabled = true;
        if (ui != null) ui.enabled = true;

        MuteAllGameAudio(false);

        yield return new WaitForSeconds(0.3f);

        DisplayEndingUI(pauseGame);
    }

    private void MuteAllGameAudio(bool mute)
    {
        if (mute)
        {
            activeAudioSources.Clear();
            AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (AudioSource source in sources)
            {
                if (source.gameObject != videoPlayer.gameObject && source.isPlaying)
                {
                    source.Pause();
                    activeAudioSources.Add(source);
                }
            }
        }
        else
        {
            foreach (AudioSource source in activeAudioSources)
            {
                if (source != null) source.UnPause();
            }
            activeAudioSources.Clear();
        }
    }

    private void DisplayEndingUI(bool pauseGame)
    {
        EndingBox.SetActive(true);
        EndingText.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);

        if (pauseGame)
            Time.timeScale = 0f;
    }

    // ===== ENDING CHECKS =====
    // Note: checkWrongEnding() đã bị xóa vì CaseFileUI tự handle WRONG ending
    
    public void checkHalfEnding()
    {
        string[] halfEndingEvidence = new string[] { "Crack", "OpenWindow", "Rope" };
        
        bool hasEnoughEvidence = true;
        foreach (string evidenceTag in halfEndingEvidence)
        {
            if (!CaseFileUI.Instance.HasEvidence(evidenceTag)) 
            { 
                hasEnoughEvidence = false; 
                break; 
            }
        }
        
        bool tanCondition = CaseFileUI.Instance.HasInformation("Tan",1) 
                            && CaseFileUI.Instance.HasInformation("Tan",2);
        bool mayCondition = CaseFileUI.Instance.HasInformation("May",1);
        bool maiCondition = CaseFileUI.Instance.HasInformation("Mai",3);
        
        if (hasEnoughEvidence && tanCondition && mayCondition && maiCondition)
            HalfEndingTriggered = true;
    }

    public void checkFullEnding()
    {
        string[] fullEndingEvidence = new string[]
        {
            "Limit1","Limit2","Limit3","Limit4","Limit5","Limit6","OpenWindow","Rope","Crack"
        };

        bool hasAllEvidence = true;
        foreach (string evidenceTag in fullEndingEvidence)
        {
            if (!CaseFileUI.Instance.HasEvidence(evidenceTag))
            {
                hasAllEvidence = false;
                break;
            }
        }
        
        bool tanCondition = CaseFileUI.Instance.HasInformation("Tan",1) 
                            && CaseFileUI.Instance.HasInformation("Tan",2);
        bool mayCondition = CaseFileUI.Instance.HasInformation("May",1);
        bool maiCondition = CaseFileUI.Instance.HasInformation("Mai",3);

        if (hasAllEvidence && tanCondition && mayCondition && maiCondition)
            FullEndingTriggered = true;
    }
}