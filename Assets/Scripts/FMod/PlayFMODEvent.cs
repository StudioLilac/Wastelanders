using FMODUnity;
using UnityEngine;

namespace FMOD {
    public class PlayFMODEvent : MonoBehaviour {
        [SerializeField]
        public EventReference eventReference;

        void Start() {
            RuntimeManager.PlayOneShot(eventReference, transform.position);
        }
    }
}