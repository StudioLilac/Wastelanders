using System;
using System.Collections;
using TMPro;
using UI_Toolkit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueScripts
{
    public class DialogueBoxV2 : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtView;
        [SerializeField] private TextMeshProUGUI whoView;
        [SerializeField] private Image imgView;
        [SerializeField] private RectTransform boxLayout;
        [SerializeField] private Canvas canvas;

        [SerializeField] private GameObject txt;
        [SerializeField] private GameObject who;
        [SerializeField] private GameObject img;
        [SerializeField] private ActorProfile eventProfile;

        [SerializeField] private int typewriterRate = 50;
        public static DialogueBoxV2 Instance { get; private set; }
#nullable enable

        private AutoAdvanceAfter? autoAdvanceAfter;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            this.Subscribe<AutoAdvanceAfter>(SetAutoAdvance);
            this.Subscribe<VerticalLayoutChange>(SetVerticalLayout);

            canvas.sortingOrder = UISortOrder.DialogueBox.GetOrder();
            gameObject.SetActive(false);
        }

        public IEnumerator Play(DialogueEntry[] entries)
        {
            gameObject.SetActive(true);

            foreach (var entry in entries)
            {
                if (SkipEntry(entry)) continue;
                WithEntry(entry);
                yield return TypewriteText();
                yield return WaitForContinuation();
                PlayTransitionSound(entry);
                yield return null;
            }

            gameObject.SetActive(false);
        }

        private bool SkipEntry(DialogueEntry entry)
        {
            if (entry.speaker == eventProfile)
            {
                entry.events.ForEach(it => it.Execute());
                return true;
            }

            return false;
        }

        private IEnumerator TypewriteText()
        {
            txtView.maxVisibleCharacters = 0;
            float elapsed = 0f;

            while (txtView.maxVisibleCharacters < txtView.text.Length)
            {
                if (HasInput())
                {
                    txtView.maxVisibleCharacters = txtView.text.Length;
                    yield break;
                }

                txtView.maxVisibleCharacters = Mathf.FloorToInt(elapsed * typewriterRate);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator WaitForContinuation()
        {
            // Clears the HasInput() from typewriting step
            yield return null;
            
            float timer = 0f;

            yield return new WaitUntil(() =>
                {
                    timer += Time.deltaTime;
                    return HasInput() || (autoAdvanceAfter is not null && timer >= autoAdvanceAfter.Time);
                }
            );

            autoAdvanceAfter = null;
        }

        private void PlayTransitionSound(DialogueEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.sfxName) || Input.GetKey(KeyCode.RightArrow))
                return;

            const string DEFAULT_SFX_NAME = "Page Flip";
            AudioManager.Instance.PlaySFX(DEFAULT_SFX_NAME);
        }

        private void SetAutoAdvance(AutoAdvanceAfter e)
        {
            autoAdvanceAfter = e;
        }

        private void SetVerticalLayout(VerticalLayoutChange e)
        {
            const float BOX_Y = 318.9375f;
            boxLayout.anchoredPosition = BOX_Y * e.Layout switch
            {
                Layout.Lower => Vector2.down,
                Layout.Upper => Vector2.up,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void WithEntry(DialogueEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.content))
            {
                txtView.text = entry.content;
                txt.SetActive(true);
            }
            else
            {
                txt.SetActive(false);
            }

            if (entry.speaker != null && !string.IsNullOrEmpty(entry.speaker.characterName))
            {
                whoView.text = entry.speaker.characterName.ToUpper();
                new SetSpeaker
                {
                    actor = entry.speaker
                }.Invoke();

                who.SetActive(true);
            }
            else
            {
                who.SetActive(false);
            }

            if (!string.IsNullOrEmpty(entry.sfxName))
            {
                AudioManager.Instance.PlaySFX(entry.sfxName);
            }

            if (entry.picture)
            {
                imgView.sprite = entry.picture;
                img.SetActive(true);
            }
            else
            {
                img.SetActive(false);
            }

            entry.events.ForEach(it => it.Execute());

            DialogueManager.Instance.AddDialogueEntryToHistory(entry);
        }

        private static bool HasInput() => !PauseMenuV2.IsPaused && (Input.GetKey(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space));
    }
}
