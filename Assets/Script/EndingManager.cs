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
        if (chase.player.dead || GameEnd.gameEnded) 
        {
            CleanupState();
            ShowEnding(playerDead: chase.player.dead);
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
            StartCoroutine(PlayVideoThenShowUI(selectedClip));
        }
        else
        {
            DisplayEndingUI();
        }
    }

    private System.Collections.IEnumerator PlayVideoThenShowUI(VideoClip clip)
    {
        // 1. TẮT TẤT CẢ ÂM THANH TRONG GAME
        MuteAllGameAudio(true);

        // 2. ẨN CANVAS UI
        if (mainGameCanvas != null) mainGameCanvas.enabled = false;
        if (ui != null) ui.enabled = false;

        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // 3. PHÁT VIDEO
        videoPlayer.Play();
        yield return new WaitForSeconds(0.1f);

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        // 4. DỪNG VIDEO VÀ BẬT LẠI UI + ÂM THANH
        videoPlayer.Stop();
        if (mainGameCanvas != null) mainGameCanvas.enabled = true;
        if (ui != null) ui.enabled = true;
        
        // Mở lại âm thanh game trước khi hiện bảng thông báo
        MuteAllGameAudio(false);

        yield return new WaitForSeconds(0.5f);

        DisplayEndingUI();
    }

    private void MuteAllGameAudio(bool mute)
    {
        if (mute)
        {
            activeAudioSources.Clear();
            AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (AudioSource source in sources)
            {
                // Không tắt AudioSource của chính VideoPlayer
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

    private void DisplayEndingUI()
    {
        EndingBox.SetActive(true);
        EndingText.gameObject.SetActive(true);
        RestartButton.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    // --- Các hàm check giữ nguyên ---
    public void checkHalfEnding()
    {
        string[] halfEndingEvidence = new string[] { "Limit1", "Crack", "OpenWindow", "Rope" };
        bool hasEnoughEvidence = true;
        foreach (string evidenceTag in halfEndingEvidence)
        {
            if (!EvidenceManager.Instance.HasEvidence(evidenceTag)) { hasEnoughEvidence = false; break; }
        }
        if (hasEnoughEvidence && DialogueManager.Instance.CheckEndingConversation()) HalfEndingTriggered = true;
    }

    public void checkFullEnding()
    {
        string[] fullEndingEvidence = new string[] { "Limit1", "Limit2", "Limit3", "Limit4", "Limit5", "Limit6", "LivingCorner", "Ultimatum", "HangPhone", "HangNoteBook", "StrangeTable", "OpenWindow", "Rope", "Crack" };
        bool hasAllEvidence = true;
        foreach (string evidenceTag in fullEndingEvidence)
        {
            if (!EvidenceManager.Instance.HasEvidence(evidenceTag)) { hasAllEvidence = false; break; }
        }
        if (hasAllEvidence && DialogueManager.Instance.CheckEndingConversation()) FullEndingTriggered = true;
    }
}