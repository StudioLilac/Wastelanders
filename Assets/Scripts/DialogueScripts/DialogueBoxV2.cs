using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI_Toolkit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueScripts
{
#nullable enable
    public record GetActorDatabase() : IQuery<ActorDatabase?>;
    public class DialogueBoxV2 : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI txtView = null!;
        [SerializeField] private TextMeshProUGUI whoView = null!;
        [SerializeField] private Image imgView = null!;
        [SerializeField] private RectTransform boxLayout = null!;
        [SerializeField] private Canvas canvas = null!;

        [SerializeField] private GameObject txt = null!;
        [SerializeField] private GameObject who = null!;
        [SerializeField] private GameObject img = null!;
        [SerializeField] private ActorDatabase actorDatabase = null!;

        [SerializeField] private int typewriterRate = 50;
        public static DialogueBoxV2 Instance { get; private set; } = null!;
        public bool IsActive => isProcessingQueue;
        private AutoAdvanceAfter? autoAdvanceAfter;
        private readonly Queue<DialogueBatch> _dialogueQueue = new();
        private bool isProcessingQueue = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            this.Subscribe<AutoAdvanceAfter>(SetAutoAdvance);
            this.Subscribe<VerticalLayoutChange>(SetVerticalLayout);
            this.Answer<GetActorDatabase, ActorDatabase?>(_ => actorDatabase);

            ChangeDialogueBoxOrder(UISortOrder.DialogueBox.GetOrder());
            boxLayout.gameObject.SetActive(false);
        }

        public void ChangeDialogueBoxOrder(int order)
        {
            canvas.sortingOrder = order;
        }

        public IEnumerator Play(DialogueEntry[] entries)
        {
            var batch = new DialogueBatch(entries);

            _dialogueQueue.Enqueue(batch);

            if (!isProcessingQueue)
            {
                StartCoroutine(ProcessQueue());
            }

            yield return new WaitUntil(() => batch.IsFinished);
        }

        private IEnumerator ProcessQueue()
        {
            isProcessingQueue = true;

            while (_dialogueQueue.Count > 0)
            {
                var currentBatch = _dialogueQueue.Peek();

                boxLayout.gameObject.SetActive(true);
                yield return RunDialogueRoutine(currentBatch.Entries);
                boxLayout.gameObject.SetActive(false);
                currentBatch.IsFinished = true;

                _dialogueQueue.Dequeue();
            }

            isProcessingQueue = false;
        }

        private IEnumerator RunDialogueRoutine(DialogueEntry[] entries)
        {
            foreach (var entry in entries)
            {
                if (SkipEntry(entry)) continue;

                WithEntry(entry);
                yield return TypewriteText();
                yield return WaitForContinuation();
                PlayTransitionSound(entry);
                yield return null;
            }
        }

        private bool SkipEntry(DialogueEntry entry)
        {
            if (entry.speaker == actorDatabase.Event)
            {
                entry.events.ForEach(it => it.Execute());
                return true;
            }

            return false;
        }

        private IEnumerator TypewriteText()
        {
            txtView.ForceMeshUpdate();
            int totalVisibleCharacters = txtView.textInfo.characterCount;
            txtView.maxVisibleCharacters = 0;

            float elapsed = 0f;

            while (txtView.maxVisibleCharacters < totalVisibleCharacters)
            {
                if (HasInput())
                {
                    txtView.maxVisibleCharacters = totalVisibleCharacters;
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
            if (entry.sfxId != SoundID.None || Input.GetKey(KeyCode.RightArrow))
                return;

            SoundID.VN_page_flip.Play();
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
                txtView.text = DialogueManager.SanitizeText(entry.content);
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

            if (entry.sfxId != SoundID.None)
            {
                entry.sfxId.Play();
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

        private static bool HasInput() => !PauseMenuV2.IsPaused && !PauseMenuV2.IsOverBlockingElement && (Input.GetKey(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space));
        private class DialogueBatch
        {
            public DialogueEntry[] Entries { get; }
            public bool IsFinished { get; set; } = false;
            public DialogueBatch(DialogueEntry[] entries) => Entries = entries;
        }
    }
}
