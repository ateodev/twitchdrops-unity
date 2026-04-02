using System;
using UnityEngine;

namespace Ateo.TwitchDrops
{
    /// <summary>
    /// ScriptableObject that holds per-game configuration for the Twitch Drops Platform.
    /// Create via: Right-click in Project > Create > TwitchDrops > Config
    /// </summary>
    [CreateAssetMenu(fileName = "TwitchDropsConfig", menuName = "TwitchDrops/Config")]
    public class TwitchDropsConfig : ScriptableObject
    {
        [Header("Platform")]
        [Tooltip("Base URL of the Cloud Functions, e.g. https://us-central1-my-project.cloudfunctions.net")]
        public string apiBaseUrl;

        [Tooltip("Your client ID as shown in the Drops Platform dashboard.")]
        public string clientId;

        [Tooltip("Your game ID as shown in the Drops Platform dashboard.")]
        public string gameId;

        [Header("Security")]
        [Tooltip("The Game API Secret shown once when the game was created. Do NOT commit this to source control — add the config asset to .gitignore.")]
        public string gameApiSecret;
    }
}
