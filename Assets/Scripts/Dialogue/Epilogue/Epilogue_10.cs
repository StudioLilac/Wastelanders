using System.Collections;
using DialogueScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Epilogue
{
    public class Epilogue_10 : MonoBehaviour
    {
        [SerializeField] private GameObject background1;
        [SerializeField] private GameObject background2;

        [SerializeField] private Image caveFlickerLayer;
        private bool shouldFlicker = false;

        [SerializeField] private DialogueEntryInUnityEditor[] jayOpeningDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] preBonfireDialogue;

        private IEnumerator Start()
        {
            yield return null;
        }

        private void Update()
        {
            if (!shouldFlicker) { return; }
            caveFlickerLayer.color = new Color(1f, 1f, 1f, 0.5f + 0.5f * Mathf.Sin(Time.time));
        }
    }
}