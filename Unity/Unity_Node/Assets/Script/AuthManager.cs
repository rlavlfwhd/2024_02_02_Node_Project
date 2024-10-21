using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.Networking;
using Newtonsoft;
using Newtonsoft.Json;

public class AuthManager : MonoBehaviour
{
    private const string SERVER_URL = "http://localhost:4000";
    private const string ACCESS_TOKEN_PREFS_KEY = "AccessToken";
    private const string REFRESH_TOKEN_PREFS_KEY = "RefreshToken";
    private const string TOKEN_EXPIRY_PREFS_KEY = "TokenExpiry";

    private string accessToken;
    private string refreshToken;
    private DateTime tokenExpiryTime;

    void Start()
    {
        LoadTokenFromPrefs();
    }

    private void LoadTokenFromPrefs()
    {
        accessToken = PlayerPrefs.GetString(ACCESS_TOKEN_PREFS_KEY, "");
        refreshToken = PlayerPrefs.GetString(REFRESH_TOKEN_PREFS_KEY, "");
        long expiryTicks = Convert.ToInt64(PlayerPrefs.GetString(TOKEN_EXPIRY_PREFS_KEY, "0"));
        tokenExpiryTime = new DateTime(expiryTicks);
    }

    private void SaveTokenToPrefs(string accessToken, string refreshToken, DateTime expiryTime)
    {
        PlayerPrefs.SetString(ACCESS_TOKEN_PREFS_KEY, accessToken);
        PlayerPrefs.SetString(REFRESH_TOKEN_PREFS_KEY, refreshToken);
        PlayerPrefs.SetString(TOKEN_EXPIRY_PREFS_KEY, expiryTime.Ticks.ToString());
        PlayerPrefs.Save();

        this.accessToken = accessToken;
        this.refreshToken = refreshToken;
        this.tokenExpiryTime = expiryTime;
    }
    public IEnumerator Register(string username, string password)
    {
        var user = new { username, password };
        var jsonData = JsonConvert.SerializeObject(user);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm($"{SERVER_URL}/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Registration Error:{www.error}");
            }
            else
            {
                Debug.Log("Registration successful");
            }
        }
    }

    public IEnumerator Login(string username, string password)
    {
        var user = new { username, password };
        var jsonData = JsonConvert.SerializeObject(user);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm($"{SERVER_URL}/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Registration Error:{www.error}");
            }
            else
            {
                var response = JsonConvert.DeserializeObject<LoginResponse>(www.downloadHandler.text);
                Debug.Log(response);
                SaveTokenToPrefs(response.accessToken, response.refreshToken, DateTime.UtcNow.AddMinutes(15));
                Debug.Log("Login successful");
            }
        }
    }
    public IEnumerator Logout()
    {
        var logoutData = new { refreshToken };
        var jsonData = JsonConvert.SerializeObject(logoutData);
        using(UnityWebRequest www = UnityWebRequest.PostWwwForm($"{SERVER_URL}/logout", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Registration Error:{www.error}");
            }
            else
            {
                accessToken = "";
                refreshToken = "";
                tokenExpiryTime = DateTime.MinValue;
                PlayerPrefs.DeleteKey(ACCESS_TOKEN_PREFS_KEY);
                PlayerPrefs.DeleteKey(REFRESH_TOKEN_PREFS_KEY);
                PlayerPrefs.DeleteKey(TOKEN_EXPIRY_PREFS_KEY);
                PlayerPrefs.Save();
                Debug.Log("Logged out successfully");
            }
        }
    }

    public IEnumerator RefreshToken()
    {
        if(string.IsNullOrEmpty(refreshToken))
        {
            Debug.LogError("리프레시 토큰이 없으므로 다시 로그인한다.");
            yield break;
        }

        var refreshData = new { refreshToken };
        var jsonData = JsonConvert.SerializeObject(refreshData);
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm($"{SERVER_URL}/token", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Registration Error:{www.error}");
                yield return Login("username", "password");
            }
            else
            {
                var response = JsonConvert.DeserializeObject<RefreshTokenResponse>(www.downloadHandler.text);
                SaveTokenToPrefs(response.accessToken, refreshToken, DateTime.UtcNow.AddMinutes(15));
                Debug.Log("Token refreshed successfully");
            }
        }
    }

    public IEnumerator GetProtectedData()
    {
        if (string.IsNullOrEmpty(accessToken) || DateTime.UtcNow >= tokenExpiryTime)
        {
            Debug.Log("Access token is empty or expired Refreshing...");
        }

        using (UnityWebRequest www = UnityWebRequest.Get($"{SERVER_URL}/protected"))
        {
            www.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Registeration Error:{www.error}");
            }
            else
            {
                Debug.Log($"Protected Data : {www.downloadHandler.text}");
            }
        }
    }
}



[System.Serializable]
public class LoginResponse
{
    public string accessToken;
    public string refreshToken;
}

[System.Serializable]
public class RefreshTokenResponse
{
    public string accessToken;
}


