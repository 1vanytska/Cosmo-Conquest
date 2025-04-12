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

    private float countdownTime = 10f;
    private float checkInterval = 5f;
    private float nextCheckTime = 0f;

    void Start()
    {
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        while (countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;

            if (countdownTime < 0)
                countdownTime = 0;

            int minutes = Mathf.FloorToInt(countdownTime / 60);
            int seconds = Mathf.FloorToInt(countdownTime % 60);

            timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);

            if (Time.time >= nextCheckTime)
            {
                nextCheckTime = Time.time + checkInterval;
                StartCoroutine(CheckPlayerCount());
            }

            yield return null;
        }

        StartCoroutine(FinalCheck());
    }

    IEnumerator CheckPlayerCount()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://50d6-93-175-201-90.ngrok-free.app/game_server/start_game.php"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = www.downloadHandler.text;
                if (response.Contains("Game started"))
                {
                    SceneManager.LoadScene(gameSceneName);
                }
            }
            else
            {
                statusText.text = "Error checking player count.";
            }
        }
    }

    IEnumerator FinalCheck()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://50d6-93-175-201-90.ngrok-free.app/game_server/start_game.php"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = www.downloadHandler.text;
                if (response.Contains("Game started"))
                {
                    SceneManager.LoadScene(gameSceneName);
                }
                else
                {
                    statusText.text = "Not enough players to start the game.";
                }
            }
            else
            {
                statusText.text = "Error starting the game.";
            }
        }
    }
}
