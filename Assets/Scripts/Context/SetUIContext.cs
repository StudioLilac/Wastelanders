namespace Context {
    using UnityEngine;

    public class SetUIContext : MonoBehaviour {
        [SerializeField] private UIContext context;
        [SerializeField] private UIContextCustomFlags customFlags;

        private void Awake() {
            UIContextManager.Set(context, customFlags);
        }

        private void OnDestroy() {
            UIContextManager.Set(UIContext.None);
        }
    }
}