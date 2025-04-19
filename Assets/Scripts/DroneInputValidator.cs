using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public DroneAnimator droneAnimator;

    public ParticleSystem kronusParticles;
    public ParticleSystem lyrionParticles;
    public ParticleSystem mystaraParticles;
    public ParticleSystem eclipsiaParticles;
    public ParticleSystem fioraParticles;

    void Start()
    {
        submitButton.onClick.AddListener(ValidateInputs);
    }

    void ValidateInputs()
    {
        clickSound.Play();
        errorText.text = "";

        int kronus = ParseInput(inputKronus.text, "Kronus");
        if (kronus == -1) return;

        int lyrion = ParseInput(inputLyrion.text, "Lyrion");
        if (lyrion == -1) return;

        int mystara = ParseInput(inputMystara.text, "Mystara");
        if (mystara == -1) return;

        int eclipsia = ParseInput(inputEclipsia.text, "Eclipsia");
        if (eclipsia == -1) return;

        int fiora = ParseInput(inputFiora.text, "Fiora");
        if (fiora == -1) return;

        if (!IsInRange(kronus, "Kronus") || !IsInRange(lyrion, "Lyrion") ||
            !IsInRange(mystara, "Mystara") || !IsInRange(eclipsia, "Eclipsia") ||
            !IsInRange(fiora, "Fiora"))
        {
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
        StartCoroutine(SubmitData(jsonData, data));
    }

    IEnumerator SubmitData(string jsonData, MoveData data)
    {
        using (UnityWebRequest www = new UnityWebRequest("https://11f5-93-175-201-90.ngrok-free.app/game_server/submit_move.php", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                errorText.text = "Data submitted successfully!";
                Debug.Log("Response: " + www.downloadHandler.text);

                droneAnimator.AnimateDrones(data);

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
        using (UnityWebRequest www = UnityWebRequest.Get("https://11f5-93-175-201-90.ngrok-free.app/game_server/get_results.php"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string response = www.downloadHandler.text;
                Debug.Log($"Results: {response}");

                ResultData resultData = JsonUtility.FromJson<ResultData>(response);
                if (resultData != null && resultData.results.Length > 0)
                {
                    Dictionary<string, ResultEntry> finalResults = new Dictionary<string, ResultEntry>();
                    foreach (var item in resultData.results)
                    {
                        finalResults[item.username] = item;
                    }

                    StartCoroutine(AnimateFinalResults(finalResults));
                }
                else
                {
                    resultText.text = "No results to display.";
                }
            }
            else
            {
                resultText.text = "Failed to retrieve results.";
                Debug.LogError("Failed to retrieve results: " + www.error);
            }
        }
    }

    IEnumerator AnimateFinalResults(Dictionary<string, ResultEntry> finalResults)
    {
        float animationDuration = 5f;
        float elapsedTime = 0f;
        List<string> teams = finalResults.Keys.ToList();

        while (elapsedTime < animationDuration)
        {
            string coloredResult = "Teams:\n";

            int teamIndex = (int)(elapsedTime / 0.3f) % teams.Count;
            string highlightedTeam = teams[teamIndex];

            foreach (var team in teams)
            {
                string randomDigits = $"{Random.Range(0, 8)}";
                if (team == highlightedTeam)
                    coloredResult += $"<color=#FFB588>{team}: {randomDigits} points</color>\n";
                else
                    coloredResult += $"{team}: {randomDigits} points\n";
            }

            resultText.text = coloredResult;
            elapsedTime += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        ResultEntry winnerEntry = finalResults.Values.Aggregate((x, y) => x.score > y.score ? x : y);
        string winnerPlanet = GetTopPlanet(winnerEntry);

        TriggerWinnerParticleEffect(winnerPlanet);

        string finalResult = "";
        foreach (var kvp in finalResults.OrderByDescending(k => k.Value.score))
        {
            if (kvp.Key == winnerEntry.username)
                finalResult += $"<color=#FFD700><b>{kvp.Key}</b>: {kvp.Value.score} points</color>\n";
            else
                finalResult += $"{kvp.Key}: {kvp.Value.score} points\n";
        }

        resultText.text = finalResult;
    }

    string GetTopPlanet(ResultEntry entry)
    {
        Dictionary<string, int> planetPoints = new Dictionary<string, int>
        {
            { "Kronus", entry.kronus },
            { "Lyrion", entry.lyrion },
            { "Mystara", entry.mystara },
            { "Eclipsia", entry.eclipsia },
            { "Fiora", entry.fiora }
        };

        return planetPoints.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
    }


    void TriggerWinnerParticleEffect(string winner)
    {
        switch (winner)
        {
            case "Kronus":
                kronusParticles.Play();
                break;
            case "Lyrion":
                lyrionParticles.Play();
                break;
            case "Mystara":
                mystaraParticles.Play();
                break;
            case "Eclipsia":
                eclipsiaParticles.Play();
                break;
            case "Fiora":
                fioraParticles.Play();
                break;
            default:
                Debug.LogError("Winner planet not found.");
                break;
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

    bool IsInRange(int value, string fieldName)
    {
        if (value < 0 || value > 1000)
        {
            errorText.text = $"{fieldName} must be between 0 and 1000!";
            return false;
        }
        return true;
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

[System.Serializable]
public class ResultEntry
{
    public string username;
    public int score;
    public int kronus;
    public int lyrion;
    public int mystara;
    public int eclipsia;
    public int fiora;
}

[System.Serializable]
public class ResultData
{
    public ResultEntry[] results;
}
