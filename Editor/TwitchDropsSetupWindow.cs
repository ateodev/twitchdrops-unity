using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Ateo.TwitchDrops;

namespace Ateo.TwitchDrops.Editor
{
    public class TwitchDropsSetupWindow : EditorWindow
    {
        private Vector2 _scroll;
        private TwitchDropsConfig _config;
        private bool _step1Done;
        private bool _step2Done;
        private bool _step3Done;

        private static readonly GUIStyle _headerStyle = new GUIStyle();
        private static readonly GUIStyle _stepStyle = new GUIStyle();
        private static readonly GUIStyle _codeStyle = new GUIStyle();
        private bool _stylesInitialized;

        [MenuItem("Window/Antibore/Twitch Drops Setup")]
        public static void Open()
        {
            var w = GetWindow<TwitchDropsSetupWindow>("Twitch Drops Setup");
            w.minSize = new Vector2(460, 500);
            w.RefreshConfig();
        }

        private void OnEnable()
        {
            RefreshConfig();
        }

        private void RefreshConfig()
        {
            // Find any existing TwitchDropsConfig asset in the project
            var guids = AssetDatabase.FindAssets("t:TwitchDropsConfig");
            if (guids.Length > 0)
                _config = AssetDatabase.LoadAssetAtPath<TwitchDropsConfig>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            _headerStyle.fontSize = 15;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.9f, 0.85f, 1f)
                : new Color(0.25f, 0.1f, 0.45f);
            _headerStyle.margin = new RectOffset(0, 0, 6, 4);

            _stepStyle.fontSize = 12;
            _stepStyle.fontStyle = FontStyle.Bold;
            _stepStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.85f, 0.85f, 0.85f)
                : Color.black;
            _stepStyle.margin = new RectOffset(0, 0, 8, 2);

            _codeStyle.fontSize = 11;
            _codeStyle.wordWrap = true;
            _codeStyle.font = (Font)Resources.Load("Fonts/LiberationMono") ?? EditorStyles.label.font;
            _codeStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.7f, 0.9f, 0.7f)
                : new Color(0.1f, 0.35f, 0.1f);
            _codeStyle.normal.background = MakeTex(1, 1,
                EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.92f, 0.95f, 0.92f));
            _codeStyle.padding = new RectOffset(8, 8, 6, 6);
            _codeStyle.margin = new RectOffset(0, 0, 2, 6);
        }

        private void OnGUI()
        {
            InitStyles();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Twitch Drops SDK Setup", _headerStyle);
            EditorGUILayout.LabelField(
                "Follow these steps to integrate Twitch Drops rewards into your game.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(8);

            // ---- Step 1: Config asset ----
            DrawStep("1", "Create a Config Asset");

            if (_config == null)
            {
                EditorGUILayout.HelpBox(
                    "No TwitchDropsConfig asset found in this project. Create one to store your credentials.",
                    MessageType.Warning);

                if (GUILayout.Button("Create TwitchDropsConfig Asset"))
                {
                    CreateConfigAsset();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(_config, typeof(TwitchDropsConfig), false);
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = _config;
                    EditorGUIUtility.PingObject(_config);
                }
                EditorGUILayout.EndHorizontal();

                bool hasAll = !string.IsNullOrEmpty(_config.apiBaseUrl)
                    && !string.IsNullOrEmpty(_config.clientId)
                    && !string.IsNullOrEmpty(_config.gameId)
                    && !string.IsNullOrEmpty(_config.gameApiSecret);

                if (hasAll)
                    EditorGUILayout.HelpBox("Config looks complete.", MessageType.Info);
                else
                    EditorGUILayout.HelpBox(
                        "Fill in all four fields in the config asset:\n" +
                        "  - API Base URL\n  - Client ID\n  - Game ID\n  - Game API Secret\n\n" +
                        "Find all four values on the Unity SDK tab of your game in the Drops Platform dashboard.",
                        MessageType.Warning);
            }

            EditorGUILayout.Space(4);

            // ---- Step 2: Add manager to scene ----
            DrawStep("2", "Add TwitchDropsManager to Your Scene");
            EditorGUILayout.LabelField(
                "Create a persistent GameObject (e.g. named \"TwitchDrops\") and add the " +
                "TwitchDropsManager component. Assign your config asset to it.",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Create TwitchDropsManager in Scene"))
            {
                CreateManagerInScene();
            }

            EditorGUILayout.Space(4);

            // ---- Step 3: Redemption UI ----
            DrawStep("3", "Add Redemption UI — choose one option");
            EditorGUILayout.LabelField(
                "Option A — Ready-made UI: click below to add a panel (input field + Redeem button + " +
                "status label) directly to your scene. Handles everything automatically.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Create TwitchDropsUI in Scene"))
                CreateDropsUI();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(
                "Option B — Custom UI: skip the button above and call RedeemCode yourself. " +
                "Subscribe to OnDropGranted to receive the result:",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(2);

            EditorGUILayout.TextArea(
@"void Start() {
    TwitchDropsManager.Instance.OnDropGranted += OnDrop;
}

void OnDrop(string itemId, string displayName) {
    // itemId matches the Item ID set in your dashboard
    MyInventory.Add(itemId);
    Debug.Log(""Drop granted: "" + displayName);
}",
                _codeStyle);

            EditorGUILayout.Space(4);

            // ---- Step 4: Test ----
            DrawStep("4", "Test Before Going Live");
            EditorGUILayout.LabelField(
                "1. In your Drops Platform dashboard, open your game's Codes tab and click " +
                "\"Create Code Manually\".\n" +
                "2. Copy the generated code.\n" +
                "3. Enter it in the TwitchDropsUI or call RedeemCode() from a test script.\n" +
                "4. Confirm OnDropGranted fires and the item is granted.",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(4);

            // ---- Security reminder ----
            EditorGUILayout.HelpBox(
                "Security: Your Game API Secret is sensitive. Add your config asset to .gitignore " +
                "so it is never committed to source control.\n\n" +
                "Example .gitignore entry:\n  Assets/TwitchDrops/Resources/TwitchDropsConfig.asset",
                MessageType.Warning);

            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }

        private void DrawStep(string number, string title)
        {
            EditorGUILayout.LabelField($"Step {number} — {title}", _stepStyle);
        }

        private void CreateConfigAsset()
        {
            const string dir = "Assets/TwitchDrops/Resources";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            const string path = dir + "/TwitchDropsConfig.asset";
            var asset = CreateInstance<TwitchDropsConfig>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            _config = asset;
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private void CreateDropsUI()
        {
            // Find or create a Canvas.
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var cgo = new GameObject("Canvas");
                canvas = cgo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = cgo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 800);
                cgo.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(cgo, "Create Canvas");
            }

            // Ensure an EventSystem exists — without it UI clicks don't work.
            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                // Use new Input System module if available, otherwise fall back to legacy.
                var newInputModule = System.Type.GetType(
                    "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (newInputModule != null)
                    esGo.AddComponent(newInputModule);
                else
                    esGo.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
            }

            // Root panel
            var panel = new GameObject("TwitchDropsPanel");
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(400, 160);
            panelRect.anchoredPosition = Vector2.zero;
            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            // Code input field
            var inputGo = new GameObject("CodeInput");
            inputGo.transform.SetParent(panel.transform, false);
            var inputRect = inputGo.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0, 1);
            inputRect.anchorMax = new Vector2(1, 1);
            inputRect.pivot = new Vector2(0.5f, 1);
            inputRect.offsetMin = new Vector2(16, 0);
            inputRect.offsetMax = new Vector2(-16, 0);
            inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, 40);
            inputRect.anchoredPosition = new Vector2(0, -16);
            var inputBg = inputGo.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var inputField = inputGo.AddComponent<TMP_InputField>();

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputGo.transform, false);
            var taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero; taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(8, 0); taRect.offsetMax = new Vector2(-8, 0);
            textArea.AddComponent<RectMask2D>();

            var placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            var phRect = placeholder.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero; phRect.anchorMax = Vector2.one;
            phRect.sizeDelta = Vector2.zero;
            var phText = placeholder.AddComponent<TextMeshProUGUI>();
            phText.text = "Enter drop code (XXXX-XXXX-XXXX-XXXX)";
            phText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            phText.fontSize = 14;
            phText.alignment = TextAlignmentOptions.MidlineLeft;

            var inputText = new GameObject("Text");
            inputText.transform.SetParent(textArea.transform, false);
            var itRect = inputText.AddComponent<RectTransform>();
            itRect.anchorMin = Vector2.zero; itRect.anchorMax = Vector2.one;
            itRect.sizeDelta = Vector2.zero;
            var itTmp = inputText.AddComponent<TextMeshProUGUI>();
            itTmp.color = Color.white;
            itTmp.fontSize = 14;
            itTmp.alignment = TextAlignmentOptions.MidlineLeft;

            inputField.textViewport = taRect;
            inputField.placeholder = phText;
            inputField.textComponent = itTmp;
            inputField.characterValidation = TMP_InputField.CharacterValidation.None;

            // Redeem button
            var btnGo = new GameObject("RedeemButton");
            btnGo.transform.SetParent(panel.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(0.5f, 1);
            btnRect.offsetMin = new Vector2(16, 0);
            btnRect.offsetMax = new Vector2(-16, 0);
            btnRect.sizeDelta = new Vector2(btnRect.sizeDelta.x, 40);
            btnRect.anchoredPosition = new Vector2(0, -68);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.39f, 0.25f, 0.65f, 1f);
            var btn = btnGo.AddComponent<Button>();
            var btnColors = btn.colors;
            btnColors.highlightedColor = new Color(0.5f, 0.35f, 0.8f, 1f);
            btn.colors = btnColors;

            var btnLabel = new GameObject("Label");
            btnLabel.transform.SetParent(btnGo.transform, false);
            var blRect = btnLabel.AddComponent<RectTransform>();
            blRect.anchorMin = Vector2.zero; blRect.anchorMax = Vector2.one;
            blRect.sizeDelta = Vector2.zero;
            var blText = btnLabel.AddComponent<TextMeshProUGUI>();
            blText.text = "Redeem";
            blText.color = Color.white;
            blText.fontSize = 15;
            blText.fontStyle = FontStyles.Bold;
            blText.alignment = TextAlignmentOptions.Center;

            // Status text
            var statusGo = new GameObject("StatusText");
            statusGo.transform.SetParent(panel.transform, false);
            var stRect = statusGo.AddComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0, 1);
            stRect.anchorMax = new Vector2(1, 1);
            stRect.pivot = new Vector2(0.5f, 1);
            stRect.offsetMin = new Vector2(16, 0);
            stRect.offsetMax = new Vector2(-16, 0);
            stRect.sizeDelta = new Vector2(stRect.sizeDelta.x, 24);
            stRect.anchoredPosition = new Vector2(0, -120);
            var statusTmp = statusGo.AddComponent<TextMeshProUGUI>();
            statusTmp.text = "";
            statusTmp.color = new Color(0.7f, 0.9f, 0.7f, 1f);
            statusTmp.fontSize = 12;
            statusTmp.alignment = TextAlignmentOptions.Center;

            // Wire up TwitchDropsUI component
            var ui = panel.AddComponent<TwitchDropsUI>();
            ui.codeInput = inputField;
            ui.redeemBtn = btn;
            ui.statusText = statusTmp;

            Undo.RegisterCreatedObjectUndo(panel, "Create TwitchDropsUI");
            Selection.activeObject = panel;
            Debug.Log("[TwitchDrops] TwitchDropsUI created in scene. It is parented to the Canvas.");
        }

        private void CreateManagerInScene()
        {
            var go = new GameObject("TwitchDrops");
            var mgr = go.AddComponent<TwitchDropsManager>();
            if (_config != null)
            {
                var so = new SerializedObject(mgr);
                var prop = so.FindProperty("config");
                if (prop != null) { prop.objectReferenceValue = _config; so.ApplyModifiedProperties(); }
            }
            Selection.activeObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create TwitchDropsManager");
            Debug.Log("[TwitchDrops] TwitchDropsManager added to scene. Assign your config asset if it wasn't auto-assigned.");
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var t = new Texture2D(w, h);
            t.SetPixel(0, 0, col);
            t.Apply();
            return t;
        }
    }
}
