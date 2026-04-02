using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Ateo.TwitchDrops;

namespace Ateo.TwitchDrops.Editor
{
    [CustomEditor(typeof(TwitchDropsConfig))]
    public class TwitchDropsConfigEditor : UnityEditor.Editor
    {
        private string _testStatus = "";
        private bool _testing = false;

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Setup Guide  (Window > Antibore > Twitch Drops Setup)"))
                TwitchDropsSetupWindow.Open();

            EditorGUILayout.Space(6);
            DrawDefaultInspector();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(_testing))
            {
                if (GUILayout.Button("Test Connection"))
                {
                    _testStatus = "Testing...";
                    _testing = true;
                    var cfg = (TwitchDropsConfig)target;
                    EditorCoroutine.Start(TestConnection(cfg));
                }
            }

            if (!string.IsNullOrEmpty(_testStatus))
            {
                EditorGUILayout.HelpBox(_testStatus, MessageType.Info);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Do not commit this asset to source control if it contains your Game API Secret.\nAdd it to .gitignore:\n  Assets/TwitchDrops/Resources/TwitchDropsConfig.asset",
                MessageType.Warning);
        }

        private IEnumerator TestConnection(TwitchDropsConfig cfg)
        {
            if (string.IsNullOrEmpty(cfg.apiBaseUrl))
            {
                _testStatus = "API Base URL is empty.";
                _testing = false;
                Repaint();
                yield break;
            }

            // Ping redeemCode with an intentionally bad code to verify the endpoint responds.
            var url = $"{cfg.apiBaseUrl.TrimEnd('/')}/redeemCode";
            var body = $"{{\"code\":\"TEST\",\"clientId\":\"{cfg.clientId}\",\"gameId\":\"{cfg.gameId}\"}}";
            var bodyBytes = Encoding.UTF8.GetBytes(body);

            using var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(bodyBytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Game-Secret", cfg.gameApiSecret ?? "");

            yield return req.SendWebRequest();

            // Always log full details to Console for debugging.
            Debug.Log($"[TwitchDrops] Test result={req.result} code={req.responseCode} error='{req.error}' body='{req.downloadHandler?.text}'");

            // 403 = bad secret (server reachable), 404 = code not found (authenticated OK)
            // Both confirm the endpoint is up and responding as expected.
            if (req.responseCode == 404 || req.responseCode == 403 || req.responseCode == 409)
                _testStatus = "Connection successful. Your credentials are correct.";
            else if (req.result == UnityWebRequest.Result.ConnectionError)
                _testStatus = $"Connection failed: {req.error}\n\nCheck that API Base URL is correct and reachable. See Console for details.";
            else if (req.result == UnityWebRequest.Result.ProtocolError)
                _testStatus = $"Server error {req.responseCode}: {req.downloadHandler?.text}\n\nSee Console for details.";
            else
                _testStatus = $"Unexpected: result={req.result} code={req.responseCode}\n{req.error}\nSee Console for details.";

            _testing = false;
            Repaint();
        }
    }

    // Minimal editor coroutine runner — waits for AsyncOperation yields.
    internal static class EditorCoroutine
    {
        public static void Start(IEnumerator routine)
        {
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                // If currently yielding an AsyncOperation, wait until it's done.
                if (routine.Current is AsyncOperation op && !op.isDone)
                    return;
                if (!routine.MoveNext())
                    EditorApplication.update -= tick;
            };
            EditorApplication.update += tick;
        }
    }
}
