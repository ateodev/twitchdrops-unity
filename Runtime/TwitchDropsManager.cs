using System;
using System.Collections;
using UnityEngine;
using Ateo.TwitchDrops.Internal;

namespace Ateo.TwitchDrops
{
    /// <summary>
    /// Singleton MonoBehaviour. Add to a persistent GameObject and assign a TwitchDropsConfig.
    ///
    /// Usage:
    ///   TwitchDropsManager.Instance.OnDropGranted += (itemId, displayName) => { ... };
    ///   StartCoroutine(TwitchDropsManager.Instance.RedeemCode("XXXX-XXXX-XXXX-XXXX", onResult, onError));
    /// </summary>
    public class TwitchDropsManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────────

        private static TwitchDropsManager _instance;

        public static TwitchDropsManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("[TwitchDrops] No TwitchDropsManager found in scene. Add one to a persistent GameObject.");
                return _instance;
            }
        }

        // ── Inspector ──────────────────────────────────────────────────────────

        [Tooltip("Assign the TwitchDropsConfig asset created for this game.")]
        public TwitchDropsConfig config;

        // ── Events ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Fired after a code is successfully redeemed.
        /// Parameters: itemId (your internal ID), displayName (human-readable label).
        /// </summary>
        public event Action<string, string> OnDropGranted;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (config == null)
                Debug.LogError("[TwitchDrops] TwitchDropsConfig is not assigned on TwitchDropsManager.");
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to redeem a drop code.
        /// On success fires OnDropGranted and calls onSuccess.
        /// On failure calls onError with a user-facing message.
        /// </summary>
        public IEnumerator RedeemCode(
            string code,
            Action<string, string> onSuccess = null,
            Action<string> onError = null)
        {
            if (config == null)
            {
                onError?.Invoke("Configuration missing.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                onError?.Invoke("Please enter a code.");
                yield break;
            }

            var url = $"{config.apiBaseUrl.TrimEnd('/')}/redeemCode";

            var body = JsonUtility.ToJson(new RedeemRequest
            {
                code = code.Trim().ToUpperInvariant(),
                clientId = config.clientId,
                gameId = config.gameId
            });

            yield return ApiClient.Post(
                url, body, config.gameApiSecret,
                responseJson =>
                {
                    var result = JsonUtility.FromJson<RedeemResponse>(responseJson);
                    Debug.Log(
                        $"[TwitchDrops] Code redeemed successfully.\n" +
                        $"  Code:         {result.code}\n" +
                        $"  Code ID:      {result.codeId}\n" +
                        $"  Item ID:      {result.itemId}\n" +
                        $"  Display Name: {result.displayName}\n" +
                        $"  Twitch UID:   {result.twitchUserId}\n" +
                        $"  Campaign ID:  {result.campaignId}"
                    );
                    OnDropGranted?.Invoke(result.itemId, result.displayName);
                    onSuccess?.Invoke(result.itemId, result.displayName);
                },
                err => onError?.Invoke(err)
            );
        }

        // ── Internal types ─────────────────────────────────────────────────────

        [Serializable]
        private class RedeemRequest
        {
            public string code;
            public string clientId;
            public string gameId;
        }

        [Serializable]
        private class RedeemResponse
        {
            public bool success;
            public string codeId;
            public string code;
            public string itemId;
            public string displayName;
            public string twitchUserId;
            public string campaignId;
        }
    }
}
