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

        [Header("Item Mappings")]
        [Tooltip("Optional: map item IDs to display names or local data if you need lookups without a network call.")]
        public ItemMapping[] itemMappings;

        /// <summary>
        /// Returns the item mapping for the given itemId, or null if not found.
        /// </summary>
        public ItemMapping GetMapping(string itemId)
        {
            foreach (var m in itemMappings)
                if (m.itemId == itemId) return m;
            return null;
        }
    }

    [Serializable]
    public class ItemMapping
    {
        [Tooltip("Must match the Item ID set in the Drops Platform dashboard.")]
        public string itemId;

        [Tooltip("Human-readable display name (e.g. Golden Claw).")]
        public string displayName;
    }
}
