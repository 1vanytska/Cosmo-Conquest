using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class WaitingRoomManager : MonoBehaviour
{
    public TMP_Text timerText;
    public TMP_Text statusText;
    public string gameSceneName = "GameScene";

    public AudioSource waitingSound;
    public AudioSource startGameSound;

    private float countdownTime = 60f;
    private float checkInterval = 3f;
    private float nextCheckTime = 0f;
    private bool gameStarted = false;
    private bool requestedStart = false;

    void Start()
    {
        if (waitingSound != null)
            waitingSound.Play();

        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        while (!gameStarted)
        {
            if (countdownTime > 0)
            {
                countdownTime -= Time.deltaTime;
                countdownTime = Mathf.Max(countdownTime, 0);

                int minutes = Mathf.FloorToInt(countdownTime / 60);
                int seconds = Mathf.FloorToInt(countdownTime % 60);
                timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
            }

            if (countdownTime <= 0 && !requestedStart)
            {
                requestedStart = true;
                StartCoroutine(RequestGameStart());
            }

            if (Time.time >= nextCheckTime)
            {
                nextCheckTime = Time.time + checkInterval;
                StartCoroutine(CheckGameStatus());
            }

            yield return null;
        }
    }

    IEnumerator CheckGameStatus()
    {
        using (UnityWebRequest www = UnityWebRequest.Get($"{ServerConfig.BaseUrl}/start_game.php"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = www.downloadHandler.text;
                if (response.Contains("Game started"))
                {
                    if (!gameStarted)
                    {
                        gameStarted = true;
                        StartGameWithSound();
                    }
                }
            }
            else
            {
                statusText.text = "Error checking game status.";
            }
        }
    }

    IEnumerator RequestGameStart()
    {
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm($"{ServerConfig.BaseUrl}/start_game.php", ""))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = www.downloadHandler.text;
                if (response.Contains("Game started"))
                {
                    if (!gameStarted)
                    {
                        gameStarted = true;
                        StartGameWithSound();
                    }
                }
                else
                {
                    statusText.text = "Waiting for more players...";
                }
            }
            else
            {
                statusText.text = "Error trying to start the game.";
            }
        }
    }

    void StartGameWithSound()
    {
        StopAllCoroutines();

        if (waitingSound != null)
            waitingSound.Stop();

        if (startGameSound != null)
            startGameSound.Play();

        StartCoroutine(LoadSceneAfterSound());
    }

    IEnumerator LoadSceneAfterSound()
    {
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(gameSceneName);
    }
}