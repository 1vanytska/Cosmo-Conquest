using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAnimator : MonoBehaviour
{
    public GameObject[] dronePrefabs;
    public Transform kronusTarget;
    public Transform lyrionTarget;
    public Transform mystaraTarget;
    public Transform eclipsiaTarget;
    public Transform fioraTarget;

    public Transform spawnPoint;
    public float circleRadius = 1.5f;
    public float droneZ = -0.3f;

    private Dictionary<Transform, List<Vector3>> targetPoints = new();
    private Dictionary<Transform, List<bool>> pointOccupied = new();

    public void AnimateDrones(MoveData data)
    {
        Debug.Log("AnimateDrones called");

        if (data == null || dronePrefabs == null || dronePrefabs.Length == 0 || spawnPoint == null)
        {
            Debug.LogError("Missing required references.");
            return;
        }

        Transform[] targets = { kronusTarget, lyrionTarget, mystaraTarget, eclipsiaTarget, fioraTarget };
        string[] names = { "Kronus", "Lyrion", "Mystara", "Eclipsia", "Fiora" };
        float[] values = { data.kronus, data.lyrion, data.mystara, data.eclipsia, data.fiora };

        for (int i = 0; i < targets.Length; i++)
        {
            CheckTarget(targets[i], names[i]);
            PrepareTargetPoints(targets[i]);
            StartCoroutine(SendDrones(Mathf.RoundToInt(values[i] / 100f), targets[i]));
        }
    }

    void CheckTarget(Transform target, string name)
    {
        if (target == null)
        {
            Debug.LogError($"Target '{name}' is not assigned");
        }
    }

    void PrepareTargetPoints(Transform target)
    {
        if (target == null) return;

        if (!targetPoints.ContainsKey(target))
        {
            List<Vector3> points = new();
            List<bool> occupied = new();

            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.PI * 2f / 10f;
                float x = Mathf.Cos(angle) * circleRadius;
                float y = Mathf.Sin(angle) * circleRadius;
                points.Add(target.position + new Vector3(x, y, droneZ));
                occupied.Add(false);
            }

            targetPoints[target] = points;
            pointOccupied[target] = occupied;
        }
    }

    IEnumerator SendDrones(int count, Transform target)
    {
        if (target == null || count <= 0) yield break;

        for (int i = 0; i < count; i++)
        {
            int index = GetFreePointIndex(target);
            if (index == -1)
            {
                Debug.LogWarning($"All points occupied around {target.name}");
                yield break;
            }

            int randomIndex = Random.Range(0, dronePrefabs.Length);
            GameObject selectedPrefab = dronePrefabs[randomIndex];

            if (selectedPrefab == null)
            {
                Debug.LogError("Selected drone prefab is null");
                continue;
            }

            GameObject drone = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log($"Spawned drone {selectedPrefab.name} → {target.name} (point {index})");

            Vector3 point = targetPoints[target][index];
            pointOccupied[target][index] = true;

            StartCoroutine(MoveToTarget(drone, point));
            yield return new WaitForSeconds(0.2f);
        }
    }

    int GetFreePointIndex(Transform target)
    {
        List<bool> occupied = pointOccupied[target];
        List<int> free = new();

        for (int i = 0; i < occupied.Count; i++)
        {
            if (!occupied[i]) free.Add(i);
        }

        if (free.Count == 0) return -1;

        return free[Random.Range(0, free.Count)];
    }

    IEnumerator MoveToTarget(GameObject drone, Vector3 target)
    {
        if (drone == null) yield break;

        float duration = 3f;
        float elapsed = 0f;
        Vector3 start = drone.transform.position;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            drone.transform.position = Vector3.Lerp(start, target, smoothT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        drone.transform.position = target;
    }
}
