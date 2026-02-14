using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LeaderboardEntry
{
    public string player_name;
    public int score;
    public string created_at;
}

[Serializable]
public class LeaderboardEntryList
{
    public LeaderboardEntry[] items;
}

[Serializable]
public class ScoreSubmission
{
    public string player_name;
    public int score;
}

[Serializable]
public class SupabaseConfig
{
    public string supabaseUrl;
    public string supabaseAnonKey;
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private string supabaseUrl;
    private string supabaseAnonKey;

    [Header("Rate Limiting")]
    [SerializeField] private float fetchCooldownSeconds = 15f;

    private List<LeaderboardEntry> cachedEntries = new List<LeaderboardEntry>();
    private float lastFetchTime = -999f;
    private bool isFetching = false;
    private bool isSubmitting = false;

    public bool IsFetching => isFetching;
    public bool IsSubmitting => isSubmitting;
    public List<LeaderboardEntry> CachedEntries => cachedEntries;
    public float CooldownRemaining => Mathf.Max(0f, fetchCooldownSeconds - (Time.unscaledTime - lastFetchTime));
    public bool CanFetch => CooldownRemaining <= 0f && !isFetching;

    private string RestBaseUrl => supabaseUrl + "/rest/v1";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadConfig();
    }

    private void LoadConfig()
    {
        var configAsset = Resources.Load<TextAsset>("SupabaseConfig");
        if (configAsset == null)
        {
            Debug.LogError("[Leaderboard] SupabaseConfig.json not found in Resources! Leaderboard disabled.");
            return;
        }
        var config = JsonUtility.FromJson<SupabaseConfig>(configAsset.text);
        supabaseUrl = config.supabaseUrl;
        supabaseAnonKey = config.supabaseAnonKey;
    }

    private bool IsConfigured => !string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseAnonKey);

    public void SubmitScore(string playerName, int score, Action<bool> onComplete)
    {
        if (isSubmitting || !IsConfigured)
        {
            onComplete?.Invoke(false);
            return;
        }
        StartCoroutine(SubmitScoreCoroutine(playerName, score, onComplete));
    }

    public void FetchLeaderboard(Action<List<LeaderboardEntry>> onComplete)
    {
        if (!IsConfigured)
        {
            onComplete?.Invoke(cachedEntries);
            return;
        }
        if (!CanFetch)
        {
            onComplete?.Invoke(cachedEntries);
            return;
        }
        StartCoroutine(FetchLeaderboardCoroutine(onComplete));
    }

    private IEnumerator SubmitScoreCoroutine(string playerName, int score, Action<bool> onComplete)
    {
        isSubmitting = true;

        string url = RestBaseUrl + "/leaderboard";
        var submission = new ScoreSubmission
        {
            player_name = playerName,
            score = score
        };
        string jsonBody = JsonUtility.ToJson(submission);

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", "Bearer " + supabaseAnonKey);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Prefer", "return=minimal");

            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            if (!success)
                Debug.LogWarning($"[Leaderboard] Submit failed: {request.error} | {request.downloadHandler.text}");

            isSubmitting = false;
            onComplete?.Invoke(success);
        }
    }

    private IEnumerator FetchLeaderboardCoroutine(Action<List<LeaderboardEntry>> onComplete)
    {
        isFetching = true;

        string url = RestBaseUrl + "/leaderboard?select=player_name,score,created_at&order=score.desc&limit=20";

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("apikey", supabaseAnonKey);
            request.SetRequestHeader("Authorization", "Bearer " + supabaseAnonKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                try
                {
                    string wrapped = "{\"items\":" + json + "}";
                    var parsed = JsonUtility.FromJson<LeaderboardEntryList>(wrapped);
                    cachedEntries = new List<LeaderboardEntry>(parsed.items ?? new LeaderboardEntry[0]);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Leaderboard] Parse error: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[Leaderboard] Fetch failed: {request.error}");
            }

            lastFetchTime = Time.unscaledTime;
            isFetching = false;
            onComplete?.Invoke(cachedEntries);
        }
    }
}
