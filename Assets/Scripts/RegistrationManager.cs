using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class RegistrationManager : MonoBehaviour
{
    public TMPro.TMP_InputField nameInput;
    public AudioSource clickSound;
    public TMPro.TMP_Text messageText;
    public string waitingRoomSceneName = "WaitingRoomScene";

    [System.Serializable]
    public class PlayerData
    {
        public string username;
    }

    [System.Serializable]
    public class RegisterResponse
    {
        public int player_id;
        public string error;
    }

    public void OnRegisterClick()
    {
        if (clickSound != null)
        {
            clickSound.Play();
        }

        string playerName = nameInput.text.Trim();
        if (!string.IsNullOrEmpty(playerName))
        {
            messageText.text = "Registering " + playerName + "...";
            StartCoroutine(Register(playerName));
        }
        else
        {
            messageText.text = "Please enter a name!";
        }
    }

    IEnumerator Register(string playerName)
    {
        PlayerData data = new PlayerData { username = playerName };
        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest("https://d962-93-175-201-90.ngrok-free.app/game_server/register.php", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);

                if (!string.IsNullOrEmpty(response.error))
                {
                    messageText.text = "Error: " + response.error;
                    Debug.LogWarning("Registration error: " + response.error);
                    yield break;
                }

                PlayerPrefs.SetInt("PlayerID", response.player_id);

                messageText.text = $"Registered successfully as {playerName}";
                Debug.Log($"Registered as {playerName} with ID {response.player_id}");

                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene(waitingRoomSceneName);
            }
            else
            {
                messageText.text = $"Registration failed: {www.error}";
                Debug.LogError("Registration failed: " + www.error);
            }
        }
    }
}
