using UnityEngine;
using Ateo.TwitchDrops;

/// <summary>
/// Minimal example showing how to subscribe to drop events and handle them.
/// Attach to any GameObject in your scene.
/// </summary>
public class BasicRedemptionExample : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to the drop event.
        // OnDropGranted fires whenever a code is successfully redeemed.
        if (TwitchDropsManager.Instance != null)
            TwitchDropsManager.Instance.OnDropGranted += HandleDrop;
    }

    private void OnDestroy()
    {
        if (TwitchDropsManager.Instance != null)
            TwitchDropsManager.Instance.OnDropGranted -= HandleDrop;
    }

    private void HandleDrop(string itemId, string displayName)
    {
        // itemId matches what you configured in the Drops Platform dashboard (e.g. "golden_claw").
        // Grant the item in your game here.
        Debug.Log($"[TwitchDrops] Drop granted: {itemId} ({displayName})");

        switch (itemId)
        {
            case "golden_claw":
                // MyInventory.Add("golden_claw");
                // UI.ShowNotification($"You received: {displayName}!");
                break;

            default:
                Debug.LogWarning($"[TwitchDrops] Unhandled itemId: {itemId}");
                break;
        }
    }

    // ── Manual code entry example (call this from a UI button) ──────────────

    public void OnRedeemButtonClicked(string code)
    {
        StartCoroutine(TwitchDropsManager.Instance.RedeemCode(
            code,
            onSuccess: (itemId, displayName) =>
            {
                Debug.Log($"Redeemed: {itemId}");
                // Show success UI
            },
            onError: err =>
            {
                Debug.LogWarning($"Redeem failed: {err}");
                // Show error UI
            }
        ));
    }
}
