using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Score Display")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI waypointsText;
    public TextMeshProUGUI timerText;

    [Header("Game Messages")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public GameObject successPanel;

    [Header("UI Buttons")]
    public Button restartButton;
    public Button quitButton;
    public Button successRestartButton;
    public Button successQuitButton;

    private int currentScore = 0;
    private int totalWaypoints = 0;
    private int waypointsCollected = 0;
    private float timeRemaining;
    private bool timerIsRunning = false;
    private Coroutine timerCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Setup button listeners
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartButtonClick);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClick);
        if (successRestartButton != null) successRestartButton.onClick.AddListener(OnRestartButtonClick);
        if (successQuitButton != null) successQuitButton.onClick.AddListener(OnQuitButtonClick);

        UpdateScoreUI();
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(false);
        if (timerText) timerText.text = "";
    }

    // Button click handlers
    public void OnRestartButtonClick()
    {
        Debug.Log("Restart button clicked");
        RestartGame();
    }

    public void OnQuitButtonClick()
    {
        Debug.Log("Quit button clicked");
        QuitGame();
    }

    public void StartTimer(float timeLimit)
    {
        timeRemaining = timeLimit;
        timerIsRunning = true;
        timerCoroutine = StartCoroutine(TimerCountdown());
    }

    private IEnumerator TimerCountdown()
    {
        while (timerIsRunning && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
            yield return null;
        }

        // Time's up!
        timerIsRunning = false;
        ShowTimeUp();
    }

    void UpdateTimerDisplay()
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void AddScore(int points)
    {
        if (!timerIsRunning) return; // Don't add score if timer isn't running

        currentScore += points;
        waypointsCollected++;
        UpdateScoreUI();

        // Check if all waypoints are collected
        if (waypointsCollected >= totalWaypoints && totalWaypoints > 0)
        {
            StopTimer();
            ShowSuccess();
        }
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerIsRunning = false;
    }

    public void SetTotalWaypoints(int count)
    {
        totalWaypoints = count;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = $"Score: {currentScore}";
        if (waypointsText) waypointsText.text = $"Waypoints: {waypointsCollected}/{totalWaypoints}";
    }

    public void ShowTimeUp()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText) gameOverText.text = $"Time's Up!\nFinal Score: {currentScore}\nWaypoints Collected: {waypointsCollected}/{totalWaypoints}";
        }
    }

    public void ShowSuccess()
    {
        if (successPanel)
        {
            successPanel.SetActive(true);
            if (gameOverText) gameOverText.text = $"Success!\nFinal Score: {currentScore}\nTime Remaining: {Mathf.FloorToInt(timeRemaining)} seconds";
        }
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public bool IsTimerRunning()
    {
        return timerIsRunning;
    }
}