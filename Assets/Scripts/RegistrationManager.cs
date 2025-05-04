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
        public string uuid;
    }

    [System.Serializable]
    public class RegisterResponse
    {
        public int player_id;
    }

    public void OnRegisterClick()
    {
        if (clickSound != null)
        {
            clickSound.Play();
        }

        string playerName = nameInput.text;
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

    private string GetOrGenerateUUID()
    {
        if (!PlayerPrefs.HasKey("uuid"))
        {
            string uuid = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("uuid", uuid);
        }
        return PlayerPrefs.GetString("uuid");
    }

    IEnumerator Register(string playerName)
    {
        string uuid = GetOrGenerateUUID();
        PlayerData data = new PlayerData { username = playerName, uuid = uuid };
        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest("https://aa8d-93-175-201-90.ngrok-free.app/game_server/register.php", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
                int playerId = response.player_id;

                PlayerPrefs.SetInt("PlayerID", playerId);

                messageText.text = $"Registered successfully as {playerName}";
                Debug.Log($"Registered as {playerName} with ID {playerId}");

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
