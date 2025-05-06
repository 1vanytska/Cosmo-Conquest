using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DroneInputValidator : MonoBehaviour
{
    public TMP_InputField inputKronus, inputLyrion, inputMystara, inputEclipsia, inputFiora;
    public TMP_Text errorText, resultText;
    public AudioSource clickSound;
    public Button submitButton;
    public DroneAnimator droneAnimator;
    public ParticleSystem kronusParticles, lyrionParticles, mystaraParticles, eclipsiaParticles, fioraParticles;

    void Start()
    {
        submitButton.onClick.AddListener(ValidateInputs);
    }

    void ValidateInputs()
    {
        clickSound.Play();
        errorText.text = "";

        int kronus = ParseInput(inputKronus.text, "Kronus");
        int lyrion = ParseInput(inputLyrion.text, "Lyrion");
        int mystara = ParseInput(inputMystara.text, "Mystara");
        int eclipsia = ParseInput(inputEclipsia.text, "Eclipsia");
        int fiora = ParseInput(inputFiora.text, "Fiora");

        if (new[] { kronus, lyrion, mystara, eclipsia, fiora }.Any(x => x == -1)) return;

        if (!IsInRange(kronus, "Kronus") || !IsInRange(lyrion, "Lyrion") ||
            !IsInRange(mystara, "Mystara") || !IsInRange(eclipsia, "Eclipsia") ||
            !IsInRange(fiora, "Fiora")) return;

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

        MoveData data = new MoveData
        {
            player_id = PlayerPrefs.GetInt("PlayerID", 0),
            kronus = kronus,
            lyrion = lyrion,
            mystara = mystara,
            eclipsia = eclipsia,
            fiora = fiora
        };

        StartCoroutine(SubmitMoveCoroutine(data));
    }

    IEnumerator SubmitMoveCoroutine(MoveData data)
    {
        submitButton.interactable = false;
        errorText.text = "Sending move...";

        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest www = new UnityWebRequest("https://2295-93-175-201-90.ngrok-free.app/game_server/submit_move.php", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                errorText.text = "Move saved! Animating drones...";
                droneAnimator.AnimateDrones(data);
                yield return new WaitForSeconds(3f);
                StartCoroutine(CheckAllSubmittedCoroutine());
            }
            else
            {
                errorText.text = $"Error during sending: {www.error}";
                submitButton.interactable = true;
            }
        }
    }

    IEnumerator CheckAllSubmittedCoroutine()
    {
        bool allMovesSubmitted = false;

        while (!allMovesSubmitted)
        {
            errorText.text = "Waiting for other players...";
            using (UnityWebRequest checkRequest = UnityWebRequest.Get("https://2295-93-175-201-90.ngrok-free.app/game_server/check_all_submitted.php"))
            {
                yield return checkRequest.SendWebRequest();

                if (checkRequest.result == UnityWebRequest.Result.Success)
                {
                    string response = checkRequest.downloadHandler.text;
                    CheckResponse jsonResponse = JsonUtility.FromJson<CheckResponse>(response);

                    if (jsonResponse.status == "ready")
                    {
                        allMovesSubmitted = true;
                        StartCoroutine(GetResults());
                    }
                }
                else
                {
                    errorText.text = "Error checking game state.";
                }
            }

            if (!allMovesSubmitted)
                yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator GetResults()
    {
        errorText.text = "";
        using (UnityWebRequest www = UnityWebRequest.Get("https://2295-93-175-201-90.ngrok-free.app/game_server/get_results.php"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string response = www.downloadHandler.text.Trim();

                if (response.StartsWith("{") && response.EndsWith("}"))
                {
                    ResultData resultData;
                    try
                    {
                        resultData = JsonUtility.FromJson<ResultData>(response);
                    }
                    catch (System.Exception ex)
                    {
                        resultText.text = "Invalid result format received.";
                        Debug.LogError($"JSON parse error: {ex.Message}\nResponse: {response}");
                        yield break;
                    }

                    if (resultData != null && resultData.results != null && resultData.results.Length > 0)
                    {
                        Dictionary<string, ResultEntry> finalResults = resultData.results.ToDictionary(r => r.username);
                        StartCoroutine(AnimateFinalResults(finalResults));
                    }
                    else
                    {
                        resultText.text = "No results to display.";
                    }
                }
                else
                {
                    resultText.text = "Invalid response format.";
                    Debug.LogError("Unexpected JSON format:\n" + response);
                }
            }
            else
            {
                resultText.text = "Failed to retrieve results.";
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
            string coloredResult = "";

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

        yield return new WaitForSeconds(0.5f);

        var winnerEntry = finalResults.Values.Aggregate((x, y) => x.score > y.score ? x : y);
        
        int localPlayerId = PlayerPrefs.GetInt("PlayerID");

        string finalResult = "";
        foreach (var kvp in finalResults.OrderByDescending(k => k.Value.score))
        {
            if (kvp.Key == winnerEntry.username)
                finalResult += $"<color=#FFD700><b>{kvp.Key}</b>: {kvp.Value.score} points</color>\n";
            else
                finalResult += $"{kvp.Key}: {kvp.Value.score} points\n";
        }

        resultText.text = finalResult;

        if (winnerEntry.player_id == localPlayerId)
        {
            HighlightWinnerPlanets();
            StartCoroutine(ClearGameData());
        }
    }

    IEnumerator ClearGameData()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://2295-93-175-201-90.ngrok-free.app/game_server/clear_game_data.php"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Game data cleared from database.");
            }
            else
            {
                Debug.LogError("Failed to clear game data: " + www.error);
            }
        }
    }

    void HighlightWinnerPlanets()
    {
        kronusParticles.Play();
        lyrionParticles.Play();
        mystaraParticles.Play();
        eclipsiaParticles.Play();
        fioraParticles.Play();
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
    public int kronus, lyrion, mystara, eclipsia, fiora;
}

[System.Serializable]
public class ResultEntry
{
    public int player_id;
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

[System.Serializable]
public class CheckResponse
{
    public string status;
}
    