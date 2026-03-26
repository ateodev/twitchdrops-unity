using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ateo.TwitchDrops
{
    /// <summary>
    /// Optional drop-in UI component. Attach to a Canvas.
    /// Requires TwitchDropsManager in the scene.
    ///
    /// Wire up in the Inspector:
    ///   codeInput  -> TMP_InputField where the player types their code
    ///   redeemBtn  -> Button that triggers redemption
    ///   statusText -> TMP_Text that shows feedback
    /// </summary>
    [AddComponentMenu("TwitchDrops/Twitch Drops UI")]
    public class TwitchDropsUI : MonoBehaviour
    {
        [Header("References")]
        public TMP_InputField codeInput;
        public Button redeemBtn;
        public TMP_Text statusText;

        [Header("Messages")]
        public string msgRedeeming = "Redeeming...";
        public string msgSuccess = "Success! You received: {displayName}";
        public string msgAlreadyRedeemed = "This code has already been redeemed.";
        public string msgNotFound = "Code not found. Please check and try again.";

        private void Start()
        {
            if (redeemBtn != null)
                redeemBtn.onClick.AddListener(OnRedeemClicked);

            SetStatus("");
        }

        private void OnRedeemClicked()
        {
            if (codeInput == null) return;
            StartCoroutine(DoRedeem(codeInput.text));
        }

        private IEnumerator DoRedeem(string code)
        {
            SetInteractable(false);
            SetStatus(msgRedeeming);

            if (TwitchDropsManager.Instance == null)
            {
                SetStatus("TwitchDropsManager not found in scene.");
                SetInteractable(true);
                yield break;
            }

            yield return TwitchDropsManager.Instance.RedeemCode(
                code,
                onSuccess: (itemId, displayName) =>
                {
                    var msg = msgSuccess.Replace("{itemId}", itemId).Replace("{displayName}", displayName);
                    SetStatus(msg);
                    if (codeInput != null) codeInput.text = "";
                    SetInteractable(true);
                },
                onError: err =>
                {
                    string msg = err switch
                    {
                        "already_redeemed" => msgAlreadyRedeemed,
                        "not_found" => msgNotFound,
                        _ => err
                    };
                    SetStatus(msg);
                    SetInteractable(true);
                }
            );
        }

        private void SetStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
        }

        private void SetInteractable(bool value)
        {
            if (redeemBtn != null) redeemBtn.interactable = value;
            if (codeInput != null) codeInput.interactable = value;
        }
    }
}
