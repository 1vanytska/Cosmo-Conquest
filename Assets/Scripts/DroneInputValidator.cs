using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class DroneInputValidator : MonoBehaviour
{
    public TMP_InputField inputKronus;
    public TMP_InputField inputLyrion;
    public TMP_InputField inputMystara;
    public TMP_InputField inputEclipsia;
    public TMP_InputField inputFiora;

    public TMP_Text errorText;
    public TMP_Text resultText;
    public AudioSource clickSound;

    public Button submitButton;

    void Start()
    {
        submitButton.onClick.AddListener(ValidateInputs);
    }

    void ValidateInputs()
    {
        clickSound.Play();

        int kronus = ParseInput(inputKronus.text, "Kronus");
        int lyrion = ParseInput(inputLyrion.text, "Lyrion");
        int mystara = ParseInput(inputMystara.text, "Mystara");
        int eclipsia = ParseInput(inputEclipsia.text, "Eclipsia");
        int fiora = ParseInput(inputFiora.text, "Fiora");

        if (kronus == -1 || lyrion == -1 || mystara == -1 || eclipsia == -1 || fiora == -1)
        {
            errorText.text = "Please enter valid numbers!";
            return;
        }

        if (kronus < 0 || lyrion < 0 || mystara < 0 || eclipsia < 0 || fiora < 0 ||
            kronus > 1000 || lyrion > 1000 || mystara > 1000 || eclipsia > 1000 || fiora > 1000)
        {
            errorText.text = "Enter numbers between 0 and 1000!";
            return;
        }

        if (!(kronus >= lyrion && lyrion >= mystara && mystara >= eclipsia && eclipsia >= fiora))
        {
            errorText.text = "Condition violated: Kronus ≥ Lyrion ≥ Mystara ≥ Eclipsia ≥ Fiora.";
            return;
        }

        if (kronus + lyrion + mystara + eclipsia + fiora != 1000)
        {
            errorText.text = "The total must be exactly 1000!";
            return;
        }

        errorText.text = "Sending data...";

        MoveData data = new MoveData
        {
            player_id = PlayerPrefs.GetInt("PlayerID", 0),
            kronus = kronus,
            lyrion = lyrion,
            mystara = mystara,
            eclipsia = eclipsia,
            fiora = fiora
        };

        string jsonData = JsonUtility.ToJson(data);
        Debug.Log("Sending JSON: " + jsonData);
        StartCoroutine(SubmitData(jsonData));
    }

    IEnumerator SubmitData(string jsonData)
    {
        using (UnityWebRequest www = new UnityWebRequest("https://9954-93-175-201-90.ngrok-free.app/game_server/submit_move.php", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                errorText.text = "Data submitted successfully!";
                Debug.Log("Sending JSON: " + jsonData);
                StartCoroutine(GetResults());
            }
            else
            {
                errorText.text = $"Submission failed: {www.error}";
                Debug.LogError($"Submission failed: {www.error}");
            }
        }
    }

    IEnumerator GetResults()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://9954-93-175-201-90.ngrok-free.app/game_server/get_results.php"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string response = www.downloadHandler.text;

                resultText.text = "Results: " + response;
                Debug.Log($"Results: {response}");
            }
            else
            {
                resultText.text = "Failed to retrieve results.";
                Debug.LogError("Failed to retrieve results: " + www.error);
            }
        }
    }

    int ParseInput(string input, string fieldName)
    {
        int result;
        if (!int.TryParse(input, out result))
        {
            errorText.text = $"{fieldName} must be a valid number!";
            return -1;
        }
        return result;
    }
}

[System.Serializable]
public class MoveData
{
    public int player_id;
    public int kronus;
    public int lyrion;
    public int mystara;
    public int eclipsia;
    public int fiora;
}

