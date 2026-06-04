using System;
using System.Collections;
using System.Collections.Generic;
using Context;
using Managers;
using Systems.Persistence;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class PauseMenuV2 : MonoBehaviour
    {
        public UIDocument rootDocument;
        public Canvas inputBlockCanvas;

        private VisualElement rootElem;
        private VisualElement dialogue;
        private VisualElement glossary;
        private VisualElement pauseMenuPanel;
        private Button pauseIconButton;

        private Toggle autoRollToggle;
        private Toggle doubleSpeedToggle;
        private Button dialogueLogButton;
        private Button skipDialogueButton;

        private State state;
        private State previousState;

        // Retained legacy cruft for compat.
        public static bool IsPaused;
        public static event Action DidPause;
        
        // is the cursor over an element that should "block/consume" click events?
        // needed for compatability between PauseMenuV2 and Unity native UI elements.
        public static bool IsOverBlockingElement { get; private set; }

        public void Awake()
        {
            rootElem = rootDocument.rootVisualElement.Q<VisualElement>("pause-menu-root") ?? throw new Exception($"{nameof(rootDocument)} unset");
            dialogue = rootElem.Q<VisualElement>("dialogue");
            glossary = rootElem.Q<VisualElement>("glossary");
            pauseMenuPanel = rootElem.Q<VisualElement>("pause-menu-panel");
            pauseIconButton = rootDocument.rootVisualElement.Q<Button>("pause-icon-button");
            autoRollToggle = rootDocument.rootVisualElement.Q<Toggle>("auto-roll-toggle");
            doubleSpeedToggle = rootDocument.rootVisualElement.Q<Toggle>("2x-speed-toggle");
            dialogueLogButton = rootDocument.rootVisualElement.Q<Button>("log-button");
            skipDialogueButton = rootDocument.rootVisualElement.Q<Button>("skip-dialogue-button");
            
            rootDocument.panelSettings.sortingOrder = UISortOrder.PauseMenu.GetOrder();

            pauseIconButton.clicked += DoPause;

            RegisterCallbacks();
            LoadInitialValues();
            
            this.Subscribe<UIContextChangedEvent>(HandleUIContextChanged);
            
            RegisterBlockingElement(pauseIconButton);
            RegisterBlockingElement(autoRollToggle);
            RegisterBlockingElement(doubleSpeedToggle);
            RegisterBlockingElement(dialogueLogButton);
            RegisterBlockingElement(skipDialogueButton);

            RegisterIconTooltipMouseEvents();
            
            SetState(State.Unpaused);
        }
        
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
            {
                if (state != State.Unpaused) DoStart();
                else DoPause();
            }
        }

        private void DoPause()
        {
            SetState(State.PauseMenuPanel);
            new PauseStateChangedEvent(true).Invoke();
            DidPause?.Invoke();
        }

        private void DoStart()
        {
            SetState(State.Unpaused);
            new PauseStateChangedEvent(false).Invoke();
            SaveLoadSystem.Instance.SavePreferences();
        }

        private void SetState(State to)
        {
            IsPaused = inputBlockCanvas.enabled = to != State.Unpaused;
            previousState = state;
            state = to;

            rootElem.style.display = to != State.Unpaused ? DisplayStyle.Flex : DisplayStyle.None;
            dialogue.style.display = to == State.Dialogue ? DisplayStyle.Flex : DisplayStyle.None;
            glossary.style.display = to == State.Glossary ? DisplayStyle.Flex : DisplayStyle.None;
            pauseMenuPanel.style.display = to == State.PauseMenuPanel ? DisplayStyle.Flex : DisplayStyle.None;
            pauseIconButton.style.display = to == State.Unpaused ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void HandleUIContextChanged(UIContextChangedEvent e) {
            var flags = e.context switch {
                UIContext.Combat   => UIContextCustomFlags.DialogueLog | UIContextCustomFlags.AutoRoll | UIContextCustomFlags.DoubleSpeed,
                UIContext.Dialogue => UIContextCustomFlags.DialogueLog | UIContextCustomFlags.SkipDialogue,
                UIContext.Custom data  => data.Flags,
                _                  => UIContextCustomFlags.None,
            };

            autoRollToggle.Display(flags.HasFlag(UIContextCustomFlags.AutoRoll));
            doubleSpeedToggle.Display(flags.HasFlag(UIContextCustomFlags.DoubleSpeed));
            dialogueLogButton.Display(flags.HasFlag(UIContextCustomFlags.DialogueLog));
            skipDialogueButton.Display(flags.HasFlag(UIContextCustomFlags.SkipDialogue));
        }
        
        private void RegisterBlockingElement(VisualElement element)
        {
            element.RegisterCallback<MouseEnterEvent>(_ => IsOverBlockingElement = true);
            element.RegisterCallback<MouseLeaveEvent>(_ => IsOverBlockingElement = false);
    
            element.RegisterCallback<MouseDownEvent>(evt => 
            {
                evt.StopPropagation();
                evt.StopImmediatePropagation();
            });
        }

        private void RegisterIconTooltipMouseEvents()
        {
            autoRollToggle.RegisterCallback<MouseEnterEvent>(evt => ShowTooltip("Toggle Auto Roll"));
            doubleSpeedToggle.RegisterCallback<MouseEnterEvent>(evt => ShowTooltip("Toggle Double Speed"));
            dialogueLogButton.RegisterCallback<MouseEnterEvent>(evt => ShowTooltip("View Dialogue Log"));
            skipDialogueButton.RegisterCallback<MouseEnterEvent>(evt => ShowTooltip("Skip Dialogue"));
            
            autoRollToggle.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
            doubleSpeedToggle.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
            dialogueLogButton.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
            skipDialogueButton.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());
        }

        private void ShowTooltip(string tooltip)
        {
            new TooltipEvent(TextTipDisplayStyle.Display, "", tooltip).Invoke();
        }

        private void HideTooltip()
        {
            new TooltipEvent(TextTipDisplayStyle.None).Invoke();
        }

        private void OnRsmClicked()
        {
            DoStart();
        }

        private void OnRstClicked()
        {
            DoStart();
            GameStateManager.Instance.Restart();
        }

        private void OnDckClicked()
        {
            DoStart();
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.SelectionScreen>().SceneName);
        }

        private void OnLvlClicked()
        {
            DoStart();
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.LevelSelect>().SceneName);
        }

        private void OnMnuClicked()
        {
            DoStart();
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.MainMenu>().SceneName);
        }

        private void OnGlsClicked()
        {
            var gameState = new GetGameState().Query();
            var skipStory = gameState switch { 
                GameState.GAME_START => true,
                GameState.OUT_OF_COMBAT => true,
                GameState.GAME_LOSE => true,
                _ => false,
            };
            if (skipStory)
            {
                GameStateManager.Instance.JumpToCombat = true;
                OnRstClicked();
            }
        }

        private void OnDlgClicked()
        {
            if (state != State.Dialogue) {
                SetState(State.Dialogue);
                var scroll = dialogue.Q<ScrollView>("scroll-dlg");
                StartCoroutine(DoScrollToBottomWithDelay(scroll));
                scroll.Clear();
                foreach (var it in CreateLabels()) scroll.Add(it);
            } else {
                SetState(previousState);
            }
        }

        private void OnSkpClicked()
        {
            OnGlsClicked();
        }

        private void OnClsClicked()
        {
            SetState(previousState);
        }
        
        private static void OnAutoRollChanged(bool value) {
            PreferencesManager.Instance.SetAutoRoll(value);
        }
        
        private static void OnDoubleSpeedChanged(bool value) {
            new DoubleSpeedChangedEvent(value).Invoke();
        }

        private static void OnMusChanged(float value)
        {
            PreferencesManager.Instance.SetMusicVolume(value);
        }

        private static void OnSfxChanged(float value)
        {
            PreferencesManager.Instance.SetSFXVolume(value);
        }

        private static void OnMusChecked(bool state)
        {
            PreferencesManager.Instance.SetMusicMuted(state);
        }

        private static void OnSfxChecked(bool state)
        {
            PreferencesManager.Instance.SetSFXMuted(state);
        }

        private static void OnVfxChecked(bool value)
        {
            PreferencesManager.Instance.SetScreenShakeEnabled(value);
        }
        
        private void RegisterCallbacks()
        {
            pauseMenuPanel.Q<Button>("button-rsm").clicked += OnRsmClicked;
            pauseMenuPanel.Q<Button>("button-rst").clicked += OnRstClicked;
            pauseMenuPanel.Q<Button>("button-dck").clicked += OnDckClicked;
            pauseMenuPanel.Q<Button>("button-lvl").clicked += OnLvlClicked;
            pauseMenuPanel.Q<Button>("button-mnu").clicked += OnMnuClicked;

            dialogue.Q<Button>("button-cls").clicked += OnClsClicked;
            glossary.Q<Button>("button-cls").clicked += OnClsClicked;

            autoRollToggle.RegisterValueChangedCallback(e => OnAutoRollChanged(e.newValue));
            doubleSpeedToggle.RegisterValueChangedCallback(e => OnDoubleSpeedChanged(e.newValue));
            dialogueLogButton.clicked += OnDlgClicked;
            skipDialogueButton.clicked += OnSkpClicked;

            pauseMenuPanel.Q<Slider>("slider-mus").RegisterValueChangedCallback(e => OnMusChanged(e.newValue));
            pauseMenuPanel.Q<Slider>("slider-sfx").RegisterValueChangedCallback(e => OnSfxChanged(e.newValue));
            pauseMenuPanel.Q<Toggle>("toggle-mus").RegisterValueChangedCallback(e => OnMusChecked(e.newValue));
            pauseMenuPanel.Q<Toggle>("toggle-sfx").RegisterValueChangedCallback(e => OnSfxChecked(e.newValue));
            pauseMenuPanel.Q<Toggle>("toggle-vfx").RegisterValueChangedCallback(e => OnVfxChecked(e.newValue));
        }

        private void LoadInitialValues() {
            UserPreferences preferences = SaveLoadSystem.Instance.GetUserPreferences();
            AudioPreferences a = preferences.audioPreferences;
            pauseMenuPanel.Q<Slider>("slider-mus").value = a.BackgroundMusicVolume;
            pauseMenuPanel.Q<Slider>("slider-sfx").value = a.SFXVolume;
            pauseMenuPanel.Q<Toggle>("toggle-mus").value = a.MusicMuted;
            pauseMenuPanel.Q<Toggle>("toggle-sfx").value = a.SFXMuted;
            pauseMenuPanel.Q<Toggle>("toggle-vfx").value = ScreenShakeHandler.IsScreenShakeEnabled;

            autoRollToggle.value = preferences.AutoRollEnabled;
            doubleSpeedToggle.value = TimeManager.Instance.IsDoubleSpeed;
        }

        private enum State
        {
            Unpaused,
            Dialogue,
            Glossary,
            PauseMenuPanel
        }

        private static IEnumerator DoScrollToBottomWithDelay(ScrollView scroll)
        {
            yield return null;
            scroll.scrollOffset = scroll.contentContainer.layout.max - scroll.contentViewport.layout.size;
        }

        // Hideous.
        private static IEnumerable<VisualElement> CreateLabels()
        {
            var history = DialogueManager.Instance.GetHistory();
            if (history.Count == 0) yield break;
            for (var i = 0; i < history.Count; i++)
            {
                if (history[i].SpeakerName != "" && (i == 0 || history[i].SpeakerName != history[i - 1].SpeakerName))
                {
                    var label1 = new Label($"<b>{history[i].SpeakerName.Trim()}</b>")
                    { style = { marginTop = i == 0 ? 0 : 32 } };
                    label1.AddToClassList("dynamic-dialogue-name");
                    yield return label1;
                }

                var label2 = new Label(history[i].BodyText.Trim()) { style = { marginTop = i == 0 ? 0 : 16 } };
                label2.AddToClassList("dynamic-dialogue-text");
                yield return label2;
            }
        }
    }

    public record DoubleSpeedChangedEvent(bool enabled) : IEvent;
    public record PauseStateChangedEvent(bool paused) : IEvent;
}

public static class VisualElementExtensions {
    public static void Display(this VisualElement el, bool visible) {
        el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}