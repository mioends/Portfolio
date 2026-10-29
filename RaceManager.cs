using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;

public class RaceManager : MonoBehaviour
{
    [System.Serializable]
    public class IntroCameraData
    {
        public CinemachineCamera camera;
        public float duration = 2f;
    }

    [Header("Race")]
    [SerializeField] private RaceTrigger[] flags;
    [SerializeField] private int requiredLaps = 2;
    public RaceTrigger[] Flags => flags;
    [SerializeField] private WrongWayManager wrongWayManager;

    [Header("Intro Cameras")]
    [SerializeField] private IntroCameraData[] introCameras;
    [SerializeField] private CinemachineCamera playerCamera;
    public GameObject playerDefaultCamera;
    WaitForSeconds ws05 = new WaitForSeconds(0.5f);

    [Header("Countdown")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float countdownStartDelay = 2f;
    [SerializeField] private float countdownTime = 1f;
    public FlagTrigger flagTrigger;

    [Header("UI")]
    [SerializeField] private TMP_Text lapText;
    [SerializeField] private TMP_Text flagText;
    [SerializeField] private GameObject mapInfo;
    [SerializeField] private TMP_Text lapCompleteText;

    [Header("Player")]
    [SerializeField] private PlayerCarWay playerCar;

    [Header("AI")]
    [SerializeField] private RaceAICar aiCar;

    [Header("Ending")]
    [SerializeField] private EndingManager endingManager;

    [SerializeField] private AudioClip countdownClip;
    [SerializeField] private AudioClip goClip;

    [SerializeField] private FlagSkipManager flagSkipManager;

    private AudioSource audioSource;

    private int currentFlagIndex;
    public int currentLap;

    private bool raceStarted;
    private bool raceCompleted;

    private bool playerWon;
    private bool aiWon;

    public int CurrentFlagIndex => currentFlagIndex;
    public bool RaceStarted => raceStarted;
    public bool RaceCompleted => raceCompleted;
    public bool PlayerWon => playerWon;
    public bool AIWon => aiWon;

    public int CurrentLap => currentLap;
    public int RequiredLaps => requiredLaps;

    private void Awake()
    {
        if (requiredLaps < 1)
            requiredLaps = 1;

        if (lapText != null)
            lapText.enabled = false;

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (flagText != null)
            flagText.transform.parent.gameObject.SetActive(false);

        if (lapCompleteText != null)
            lapCompleteText.gameObject.SetActive(false);

        audioSource = GetComponent<AudioSource>();

        if (introCameras != null)
        {
            for (int i = 0; i < introCameras.Length; i++)
            {
                if (introCameras[i].camera != null)
                    introCameras[i].camera.gameObject.SetActive(false);
            }
        }

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if(playerDefaultCamera != null) playerDefaultCamera.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (mapInfo != null)
            mapInfo.SetActive(true);

        StartCoroutine(StartRaceSequence());
    }

    private IEnumerator StartRaceSequence()
    {
        currentFlagIndex = 0;
        currentLap = 0;

        raceStarted = false;
        raceCompleted = false;

        playerWon = false;
        aiWon = false;

        ResetFlags();
        UpdateLapText();
         
        yield return StartIntroCameras();

        SetPlayerCamera();

        if (mapInfo != null)
            mapInfo.SetActive(false);

        if (playerDefaultCamera != null) playerDefaultCamera.gameObject.SetActive(true);

        yield return new WaitForSeconds(countdownStartDelay);

        yield return ShowCountdown("3");
        yield return ShowCountdown("2");
        yield return ShowCountdown("1");

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "GO!";
        }

        if (audioSource != null && goClip != null)
            audioSource.PlayOneShot(goClip);

        raceStarted = true;

        if (lapText != null)
            lapText.enabled = true;

        if (flagText != null)
            flagText.transform.parent.gameObject.SetActive(true);

        UpdateFlagText();
        UpdateLapText();

        yield return ws05;

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private IEnumerator StartIntroCameras()
    {
        if (introCameras == null || introCameras.Length == 0)
            yield break;

        for (int i = 0; i < introCameras.Length; i++)
        {
            IntroCameraData cameraData = introCameras[i];

            if (cameraData.camera == null)
                continue;

            DisableAllIntroCameras();

            cameraData.camera.gameObject.SetActive(true);

            yield return new WaitForSeconds(cameraData.duration);
        }

        DisableAllIntroCameras();
    }

    private void DisableAllIntroCameras()
    {
        if (introCameras == null)
            return;

        for (int i = 0; i < introCameras.Length; i++)
        {
            if (introCameras[i].camera != null)
                introCameras[i].camera.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowLapCompleteText()
    {
        if (lapCompleteText == null)
            yield break;

        bool visible = true;
        float elapsed = 0f;

        lapCompleteText.gameObject.SetActive(visible);

        while (elapsed < 5f)
        {
            yield return ws05;

            visible = !visible;
            lapCompleteText.gameObject.SetActive(visible);

            elapsed += 0.5f;
        }

        lapCompleteText.gameObject.SetActive(false);
    }

    private void SetPlayerCamera()
    {
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);
    }

    private IEnumerator ShowCountdown(string text)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = text;
        }

        if (audioSource != null && countdownClip != null)
            audioSource.PlayOneShot(countdownClip);

        yield return new WaitForSeconds(countdownTime);
    }

    public void CheckFlag(RaceTrigger flag)
    {
        if (!raceStarted || raceCompleted)
            return;

        int flagOrder = System.Array.IndexOf(flags, flag);

        bool returningToSkippedFlag =
            flagSkipManager != null &&
            flagSkipManager.IsSkippedFlag(flag);

        bool isWrongWay = false;

        if (wrongWayManager != null && flagOrder >= 0)
        {
            isWrongWay = wrongWayManager.CheckFlagPassed(
                flagOrder,
                returningToSkippedFlag
            );
        }

        if (flag.FlagIndex != currentFlagIndex)
        {
            if (flagSkipManager != null)
            {
                if (flag.FlagIndex < currentFlagIndex)
                {
                    flagSkipManager.CheckFlagSkip(flag);
                }
                else if (flag.FlagIndex > currentFlagIndex && !isWrongWay)
                {
                    flagSkipManager.CheckFlagSkip(flag);
                }
            }

            return;
        }

        if (flagSkipManager != null)
            flagSkipManager.CheckFlagSkip(flag);

        flag.PassFlag();

        currentFlagIndex++;

        UpdateFlagText();
    }

    public void CheckFinish()
    {
        if (!raceStarted || raceCompleted)
            return;

        if (currentFlagIndex < flags.Length)
            return;

        currentLap++;

        UpdateLapText();

        if (currentLap == 1)
            StartCoroutine(ShowLapCompleteText());

        if (currentLap >= requiredLaps)
        {
            PlayerCompleteRace();
            return;
        }

        if (flagTrigger != null)
            flagTrigger.PlayTrackSound();

        currentFlagIndex = 0;

        if (wrongWayManager != null)
            wrongWayManager.ResetWrongWay();

        ResetFlags();
        UpdateFlagText();
    }

    private void UpdateFlagText()
    {
        if (flagText == null)
            return;

        flagText.text = $"{currentFlagIndex} / {flags.Length}";
    }
    private void PlayerCompleteRace()
    {
        if (raceCompleted)
            return;

        raceCompleted = true;
        raceStarted = false;

        playerWon = true;
        aiWon = false;

        if (flagText != null)
            flagText.transform.parent.gameObject.SetActive(false);

        if (lapText != null)
            lapText.enabled = false;

        if (endingManager != null)
            endingManager.PlayerWin();
    }

    public void AICompleteRace()
    {
        if (raceCompleted)
            return;

        raceCompleted = true;
        raceStarted = false;

        playerWon = false;
        aiWon = true;

        if (flagText != null)
            flagText.transform.parent.gameObject.SetActive(false);

        if (lapText != null)
            lapText.enabled = false;

        if (endingManager != null)
            endingManager.AIWin();
    }

    private void ResetFlags()
    {
        if (flags == null)
            return;

        for (int i = 0; i < flags.Length; i++)
        {
            if (flags[i] != null)
                flags[i].ResetFlag();
        }
    }

    private void UpdateLapText()
    {
        if (lapText == null)
            return;

        int displayLap = Mathf.Clamp(
            currentLap + 1,
            1,
            requiredLaps
        );

        lapText.text = $"{displayLap} / {requiredLaps}";
    }
}