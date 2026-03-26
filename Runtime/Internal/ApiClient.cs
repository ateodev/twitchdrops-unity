using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Ateo.TwitchDrops.Internal
{
    internal static class ApiClient
    {
        /// <summary>
        /// Posts a JSON body to the given URL with the X-Game-Secret header.
        /// Calls onSuccess(responseJson) or onError(errorMessage).
        /// </summary>
        internal static IEnumerator Post(
            string url,
            string jsonBody,
            string gameApiSecret,
            Action<string> onSuccess,
            Action<string> onError)
        {
            var bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(bodyBytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Game-Secret", gameApiSecret);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(req.downloadHandler.text);
            }
            else
            {
                // Try to extract error message from JSON body
                string msg = req.downloadHandler.text;
                try
                {
                    var err = JsonUtility.FromJson<ErrorResponse>(msg);
                    if (!string.IsNullOrEmpty(err?.error)) msg = err.error;
                }
                catch { /* keep raw msg */ }

                onError?.Invoke(msg);
            }
        }

        [Serializable]
        private class ErrorResponse
        {
            public string error;
        }
    }
}
