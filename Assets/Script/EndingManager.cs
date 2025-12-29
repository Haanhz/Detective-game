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

    // FIX: chặn gọi ending nhiều lần
    private bool endingStarted = false;

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
        // FIX: không disable script, chỉ chặn chạy lại
        if (endingStarted) return;

        if (chase.player.dead || GameEnd.gameEnded) 
        {
            endingStarted = true;
            CleanupState();
            ShowEnding(playerDead: chase.player.dead);
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

        if (playerDead)
        {
            resultText = "You died.";
            selectedClip = killedEndingClip;
        }
        else
        {
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
                resultText = "Nobody will ever believe you.";
                selectedClip = NobodyEndingClip;
            }
        }

        EndingText.text = resultText;

        if (selectedClip != null && videoPlayer != null)
        {
            StartCoroutine(PlayVideoThenShowUI(selectedClip, playerDead)); // FIX
        }
        else
        {
            DisplayEndingUI(playerDead); // FIX
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
    public void checkHalfEnding()
    {
        string[] halfEndingEvidence = new string[] { "Limit1", "Crack", "OpenWindow", "Rope" };
        bool hasEnoughEvidence = true;
        foreach (string evidenceTag in halfEndingEvidence)
        {
            if (!EvidenceManager.Instance.HasEvidence(evidenceTag)) { hasEnoughEvidence = false; break; }
        }
        if (hasEnoughEvidence && DialogueManager.Instance.CheckEndingConversation())
            HalfEndingTriggered = true;
    }

    public void checkFullEnding()
    {
        string[] fullEndingEvidence = new string[]
        {
            "Limit1","Limit2","Limit3","Limit4","Limit5","Limit6",
            "LivingCorner","Ultimatum","HangPhone","HangNoteBook",
            "StrangeTable","OpenWindow","Rope","Crack"
        };

        bool hasAllEvidence = true;
        foreach (string evidenceTag in fullEndingEvidence)
        {
            if (!EvidenceManager.Instance.HasEvidence(evidenceTag))
            {
                hasAllEvidence = false;
                break;
            }
        }

        if (hasAllEvidence && DialogueManager.Instance.CheckEndingConversation())
            FullEndingTriggered = true;
    }
}
