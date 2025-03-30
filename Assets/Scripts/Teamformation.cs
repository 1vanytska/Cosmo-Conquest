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
        CompareTeams();
        DisplayResults();
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
                result += $"<color=#FFD700>{team.Key}: {team.Value} points</color>\n";
            }
            else
            {
                result += $"{team.Key}: {team.Value} points\n";
            }
        }

        Debug.Log(result);
        if (resultText != null)
        {
            resultText.text = result;
            if (winningTeams.Count == 1)
            {
                StartCoroutine(AnimateWinner(winningTeams[0]));
            }
        }
    }

    IEnumerator AnimateWinner(string winner)
    {
        TMP_Text text = resultText;
        string originalText = text.text;
        string winnerLine = originalText.Split('\n').FirstOrDefault(line => line.Contains(winner));

        while (true)
        {
            string flashingText = winnerLine.Replace("<color=#FFD700>", "<color=#FFFFFF>").Replace("</color>", "<color=#FFD700>");
            string newText = originalText.Replace(winnerLine, flashingText);

            text.text = newText;

            yield return new WaitForSeconds(0.5f);
            text.text = originalText;
            yield return new WaitForSeconds(0.5f);
        }
    }
}





