#nullable enable

using DialogueScripts;
using UnityEngine;
using Yarn.Unity;

namespace Storybook
{
    public class DialogueBoxV2LineAdvancerInput : MonoBehaviour, ILineAdvancerInput
    {
        [SerializeField] private LineAdvancer? lineAdvancer;

        public LineAdvancer? LineAdvancer
        {
            get => lineAdvancer;
            set => lineAdvancer = value;
        }

        private void Update()
        {
            if (!lineAdvancer || !DialogueBoxV2.HasInput())
            {
                return;
            }

            lineAdvancer.OnInputHurryUpLines();
            lineAdvancer.OnInputHurryUpOptions();
        }

        public void OnDialogueStarted()
        {
        }

        public void OnDialogueComplete()
        {
        }
    }
}