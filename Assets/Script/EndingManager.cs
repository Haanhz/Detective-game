using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Video;

public class EndingManager : MonoBehaviour
{
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
     private bool WrongEndingTriggered = false;


    private bool endingStarted = false;

    public static bool IsKilledByBlack = false;

    // Danh sách để lưu các AudioSource bị tắt để bật lại sau đó
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

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

        if (chase.player.killed) 
        {
            endingStarted = true;
            CleanupState();
            ShowEnding(playerDead: chase.player.killed);
        } else if (chase.player.exhausted)
        {
            endingStarted = true;
            CleanupState();
            ShowEnding(playerDead: chase.player.exhausted);
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
        VideoClip selectedClip = null;
        string resultText = "";

        if (playerDead && chase.player.killed)
        {
            IsKilledByBlack = true;
            resultText = "You died in the chase!";
            selectedClip = killedEndingClip;
        }
        else if (playerDead && chase.player.exhausted)
        {
            resultText = "You collapsed from exhaustion!";
            selectedClip = exhaustedEndingClip;
        }
        else
        {
            checkFullEnding();
            if (!FullEndingTriggered) checkHalfEnding();
            if (!FullEndingTriggered && !HalfEndingTriggered) checkWrongEnding();

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
            else if (WrongEndingTriggered || !chooseRightMurderer)
            {
                resultText = "Hmm...you're not much of a detective, are you?";
                selectedClip = WrongEndingClip;
            }
            else
            {
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

    private System.Collections.IEnumerator PlayVideoThenShowUI(VideoClip clip, bool pauseGame) // FIX
    {
        // 1. TẮT ÂM THANH
        MuteAllGameAudio(true);

        // 2. ẨN UI
        if (mainGameCanvas != null) mainGameCanvas.enabled = false;
        if (ui != null) ui.enabled = false;

        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        while (videoPlayer.isPlaying)
            yield return null;

        // 3. KẾT THÚC VIDEO
        videoPlayer.Stop();

        if (mainGameCanvas != null) mainGameCanvas.enabled = true;
        if (ui != null) ui.enabled = true;

        MuteAllGameAudio(false);

        yield return new WaitForSeconds(0.3f);

        DisplayEndingUI(pauseGame); // FIX
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

    // FIX: chỉ pause game khi chết
    private void DisplayEndingUI(bool pauseGame)
    {
        EndingBox.SetActive(true);
        EndingText.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);

        if (pauseGame)
            Time.timeScale = 0f;
    }

    // ===== GIỮ NGUYÊN LOGIC CŨ =====
public void checkWrongEnding()
{
    string[] wrongEndingEvidence = new string[] { "HangPhone", "HangNoteBook"};
    
    // Kiểm tra có ít nhất 1 evidence
    bool hasAnyEvidence = false;
    foreach (string evidenceTag in wrongEndingEvidence)
    {
        if (CaseFileUI.Instance.HasEvidence(evidenceTag))
        {
            hasAnyEvidence = true;
            break;
        }
    }
    
    // Kiểm tra conditions
    bool tanCondition = CaseFileUI.Instance.HasInformation("Tan", 0);
    bool maiCondition = CaseFileUI.Instance.HasInformation("Mai", 3);
    
    // Chỉ cần 1 trong các điều kiện là true
    if (hasAnyEvidence || tanCondition || maiCondition)
        WrongEndingTriggered = true;
}
    public void checkHalfEnding()
    {
        string[] halfEndingEvidence = new string[] { "Limit1", "Crack", "OpenWindow", "Rope" };
        bool hasEnoughEvidence = true;
        foreach (string evidenceTag in halfEndingEvidence)
        {
            //if (!EvidenceManager.Instance.HasEvidence(evidenceTag)) { hasEnoughEvidence = false; break; }
            if (!CaseFileUI.Instance.HasEvidence(evidenceTag)) { hasEnoughEvidence = false; break; }
        }
        bool tanCondition = CaseFileUI.Instance.HasInformation("Tan",1) && CaseFileUI.Instance.HasInformation("Tan",2);
        bool mayCondition = CaseFileUI.Instance.HasInformation("May",1);
        bool maiCondition = CaseFileUI.Instance.HasInformation("Mai",3);
        // if (hasEnoughEvidence && DialogueManager.Instance.CheckEndingConversation())
        //     HalfEndingTriggered = true;
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
        bool tanCondition = CaseFileUI.Instance.HasInformation("Tan",1) && CaseFileUI.Instance.HasInformation("Tan",2);
        bool mayCondition = CaseFileUI.Instance.HasInformation("May",1);
        bool maiCondition = CaseFileUI.Instance.HasInformation("Mai",3);

        if (hasAllEvidence && tanCondition && mayCondition && maiCondition)
            FullEndingTriggered = true;
    }
}
