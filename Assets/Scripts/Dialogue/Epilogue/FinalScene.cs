using System.Collections;
using System.Collections.Generic;
using Particles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Epilogue {
    public class FinalScene : MonoBehaviour {
        private class CaptionNarration {
            [SerializeField] [TextArea(1, 5)] private string content;
        }
        
        [SerializeField] private List<CaptionNarration> narrations;
        [SerializeField] private TextMeshProUGUI captionTextMesh;
        [SerializeField] private Image whiteOverlay;
        [SerializeField] private POVBlizzard povBlizzard;
        [SerializeField] private FogVolume2D fogVolume2D;
        [SerializeField] private ParticleSystem fog;

        private void Start() {
            int n = narrations.Count;
            UIFadeScreenManager.Instance.SetDarkScreen();
            
            StartCoroutine(PlayScene());
        }

        private IEnumerator PlayScene() {
            // we'll fade in from black from the previous scene (boss fight). However, we will load this scene
            // and wait x amount of time, before fading in. The x amount of time will be determined by FMOD; we
            // want this scene to sync with music meaning that the fade in should only happen once the music is 
            // at the right point.
            // TODO: make this wait until fmod flips to the ending track
            yield return new WaitForSeconds(0.5f);
            
            // now the scene starts, with the music synced up
            
        }
    }
}