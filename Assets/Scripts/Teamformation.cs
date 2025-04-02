using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TeamComparison : MonoBehaviour
{
    public TMP_Text resultText;
    private Dictionary<string, int[]> teamData = new Dictionary<string, int[]>();
    private Dictionary<string, int> teamScores = new Dictionary<string, int>();

    void Start()
    {
        InitializeTeams();
        StartCoroutine(AnimateTeams());
    }

    void InitializeTeams()
    {
        teamData["Team 1"] = new int[] { 350, 200, 200, 150, 100 };
        teamData["Team 2"] = new int[] { 400, 300, 200, 100, 0 };
        teamData["Team 3"] = new int[] { 250, 250, 250, 200, 50 };
        teamData["Team 4"] = new int[] { 300, 300, 300, 50, 50 };
        teamData["Team 5"] = new int[] { 400, 200, 200, 100, 100 };

        foreach (var team in teamData.Keys)
        {
            teamScores[team] = 0;
        }
    }

    IEnumerator AnimateTeams()
    {
        float animationDuration = 5f; // Трохи довша анімація
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // Оновлення випадкових цифр
            string newResult = "Teams:\n";
            foreach (var team in teamData)
            {
                string randomDigits = $"{Random.Range(0, 10)}";
                newResult += $"{team.Key}: {randomDigits} points\n";
            }

            resultText.text = newResult;

            // Зміна кольору команд по черзі
            int teamIndex = (int)(elapsedTime / 0.3f) % teamData.Count;
            string highlightedTeam = teamData.Keys.ElementAt(teamIndex);
            string coloredResult = "Teams:\n";

            foreach (var team in teamData.Keys)
            {
                string randomDigits = $"{Random.Range(0, 10)}";
                
                if (team == highlightedTeam)
                {
                    coloredResult += $"<color=#FFB588>{team}: {randomDigits} points</color>\n"; // Повний рядок іншим кольором
                }
                else
                {
                    coloredResult += $"{team}: {randomDigits} points\n";
                }
            }

            resultText.text = coloredResult;

            elapsedTime += 0.05f; // Цифри змінюються швидше, ніж колір команд
            yield return new WaitForSeconds(0.05f);
        }

        // Після анімації виконуємо обчислення результатів
        CompareTeams();
        DisplayResults();
    }

    void CompareTeams()
    {
        foreach (var teamA in teamData)
        {
            foreach (var teamB in teamData)
            {
                if (teamA.Key == teamB.Key) continue;

                int scoreA = 0, scoreB = 0;

                for (int i = 0; i < teamA.Value.Length; i++)
                {
                    if (teamA.Value[i] > teamB.Value[i]) scoreA++;
                    else if (teamA.Value[i] < teamB.Value[i]) scoreB++;
                }

                if (scoreA > scoreB) teamScores[teamA.Key] += 2;
                else if (scoreA == scoreB) teamScores[teamA.Key] += 1;
            }
        }
    }

    void DisplayResults()
    {
        int maxScore = teamScores.Values.Max();
        List<string> winningTeams = teamScores.Where(t => t.Value == maxScore).Select(t => t.Key).ToList();

        string result = "Final Scores:\n";

        foreach (var team in teamScores)
        {
            if (winningTeams.Contains(team.Key))
            {
                result += $"<color=#FF9B62>{team.Key}: {team.Value} points</color>\n"; // Виділяємо переможця
            }
            else
            {
                result += $"{team.Key}: {team.Value} points\n";
            }
        }

        resultText.text = result;
    }
}
