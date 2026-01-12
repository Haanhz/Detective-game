using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Video;
using NUnit.Framework;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }
    [Header("Ending Music")]
    public AudioSource endingAudioSource;
    public AudioClip sadEndingMusic;    // Nhạc buồn
    public AudioClip happyEndingMusic;

    [Header("UI References")]
    public Canvas mainGameCanvas;
    public Canvas ui;
    public GameObject EndingBox;
    public TextMeshProUGUI EndingText;
    public Button RestartButton;

    [Header("Cutscene Settings")]
    public VideoPlayer videoPlayer;

    [Header("Video File Names (Include .mp4)")]
    public string killedEndingName = "Killed.mp4";
    public string exhaustedEndingName = "Exhausted.mp4";
    public string fullEndingName = "True_End.004.mp4";
    public string halfEndingName = "HE_03.mp4";
    public string wrongEndingName = "Wrong_02.mp4";
    public string nobodyEndingName = "Noone.mp4";

    private ChaseManager chase => ChaseManager.instance;
    private GameManager GameEnd => GameManager.Instance;

    private EvidenceManager EvidenceManager => EvidenceManager.Instance;
    private DialogueManager DialogueManager => DialogueManager.Instance;

    private bool HalfEndingTriggered = false;
    private bool FullEndingTriggered = false;

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
            videoPlayer.source = VideoSource.Url; // Thiết lập mặc định dùng URL cho WebGL
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
            if (chase.player.killed)
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
        string selectedVideoName = ""; // Thay đổi từ Clip sang Name
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
            selectedVideoName = killedEndingName;
        }
        else if (playerDead && chase.player.exhausted)
        {
            resultText = "You collapsed from exhaustion!";
            selectedVideoName = exhaustedEndingName;
        }
        else if (playerDead && isTimeOut)
        {
            resultText = "Out of time. Now you are the next victim";
            selectedVideoName = killedEndingName;
        }
        else
        {
            checkFullEnding();
            if (!FullEndingTriggered) checkHalfEnding();

            bool chooseRightMurderer = DialogueManager.Instance.ChooseRightMurderer();

            if (FullEndingTriggered && chooseRightMurderer)
            {
                resultText = "Congratulations! You've uncovered the full truth.";
                selectedVideoName = fullEndingName;
            }
            else if (HalfEndingTriggered && chooseRightMurderer)
            {
                resultText = "You've found a piece, but the whole truth remains hidden.";
                selectedVideoName = halfEndingName;
            }
            else
            {
                resultText = "Nobody will ever believe you.";
                selectedVideoName = nobodyEndingName;
            }
        }

        EndingText.text = resultText;

        if (!string.IsNullOrEmpty(selectedVideoName) && videoPlayer != null)
        {
            StartCoroutine(PlayVideoThenShowUI(selectedVideoName, playerDead));
        }
        else
        {
            DisplayEndingUI(playerDead);
        }
    }

    public void ShowWrongEnding()
    {
        IsDetectiveEnding = true;
        endingStarted = true;

        string resultText = "Hmm...you're not much of a detective, are you?";
        string selectedVideoName = wrongEndingName;

        EndingText.text = resultText;

        if (!string.IsNullOrEmpty(selectedVideoName) && videoPlayer != null)
        {
            StartCoroutine(PlayVideoThenShowUI(selectedVideoName, false));
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
        string selectedVideoName = nobodyEndingName;

        EndingText.text = resultText;

        if (!string.IsNullOrEmpty(selectedVideoName) && videoPlayer != null)
        {
            StartCoroutine(PlayVideoThenShowUI(selectedVideoName, false));
        }
        else
        {
            DisplayEndingUI(false);
        }
    }

    // ===== VIDEO & UI HANDLING =====

    private System.Collections.IEnumerator PlayVideoThenShowUI(string fileName, bool pauseGame)
    {
        MuteAllGameAudio(true);

        if (mainGameCanvas != null) mainGameCanvas.enabled = false;
        if (ui != null) ui.enabled = false;

        // Xử lý đường dẫn cho WebGL
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        // Chờ một frame để isPlaying cập nhật đúng
        yield return null;

        while (videoPlayer.isPlaying)
            yield return null;

        videoPlayer.Stop();

        if (mainGameCanvas != null) mainGameCanvas.enabled = true;
        if (ui != null) ui.enabled = true;

        MuteAllGameAudio(false);
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
                if (source.gameObject != videoPlayer.gameObject && source.isPlaying && source != endingAudioSource)
                {
                    source.Stop();
                    activeAudioSources.Add(source);
                }
            }
        }
        else
        {
            activeAudioSources.Clear();
        }
    }

    private void DisplayEndingUI(bool pauseGame)
    {
        EndingBox.SetActive(true);
        EndingText.gameObject.SetActive(true);
        MuteAllGameAudio(true);
        PlayEndingMusic();

        if (!FullEndingTriggered && !isTimeOut)
        {
            RestartButton.gameObject.SetActive(true);
        }
        else
        {
            RestartButton.gameObject.SetActive(false);
        }

        if (pauseGame)
            Time.timeScale = 0f;
    }

    private void PlayEndingMusic()
    {
        if (endingAudioSource == null) return;
        AudioClip musicToPlay = FullEndingTriggered ? happyEndingMusic : sadEndingMusic;

        if (musicToPlay != null)
        {
            endingAudioSource.clip = musicToPlay;
            endingAudioSource.loop = true;
            endingAudioSource.Play();
        }
    }

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

        bool tanCondition = CaseFileUI.Instance.HasInformation("Tan", 1) && CaseFileUI.Instance.HasInformation("Tan", 2);
        bool mayCondition = CaseFileUI.Instance.HasInformation("May", 1);
        bool maiCondition = CaseFileUI.Instance.HasInformation("Mai", 3);

        if (hasEnoughEvidence && tanCondition && mayCondition && maiCondition)
            HalfEndingTriggered = true;
    }

    public void checkFullEnding()
    {
        string[] fullEndingEvidence = new string[] { "Limit2","Limit3","Limit4","Limit5","Limit6","OpenWindow","Rope","Crack" };
        bool hasAllEvidence = true;
        foreach (string evidenceTag in fullEndingEvidence)
        {
            if (!CaseFileUI.Instance.HasEvidence(evidenceTag))
            {
                hasAllEvidence = false;
                break;
            }
        }

        bool tanCondition = CaseFileUI.Instance.HasInformation("Tan", 1) && CaseFileUI.Instance.HasInformation("Tan", 2);
        bool mayCondition = CaseFileUI.Instance.HasInformation("May", 1);
        bool maiCondition = CaseFileUI.Instance.HasInformation("Mai", 3);

        if (hasAllEvidence && tanCondition && mayCondition && maiCondition)
            FullEndingTriggered = true;
    }
}