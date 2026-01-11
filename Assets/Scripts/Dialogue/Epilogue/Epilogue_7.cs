using System.Collections;
using Cinemachine;
using DialogueScripts;
using UnityEngine;
using UnityEngine.Serialization;

public class Epilogue_7 : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] defaultBackground;
    [SerializeField] private SpriteRenderer[] deadCreatureBackground;
    [SerializeField] private SpriteRenderer[] blackScreen; // Used for a black screen, keeping dialogue visible
    [SerializeField] private SpriteRenderer[] wakeUpBackground;
    [SerializeField] private SpriteRenderer[] reversedCaveBackground;
    [SerializeField] private SpriteRenderer kadeSprite;
    [SerializeField] private Color kadeInitialColor;
    [SerializeField] private Color kadeFinalColor;
    [SerializeField] private ScreenShakeHandler screenShakeHandler;
    [SerializeField] private CinemachineVirtualCamera dynamicCamera;
    [SerializeField] private DialogueEntryInUnityEditor[] preFightDialogue;
    [SerializeField] private DialogueEntryInUnityEditor[] defaultBackgroundDialogue;
    [SerializeField] private DialogueEntryInUnityEditor[] deadCreatureDialogue;
    [SerializeField] private DialogueEntryInUnityEditor[] blackingOut1;
    [SerializeField] private DialogueEntryInUnityEditor[] blackingOut2;
    [SerializeField] private DialogueEntryInUnityEditor[] bonePopping;
    [SerializeField] private DialogueEntryInUnityEditor[] preKadeFadeDialogue;
    [SerializeField] private DialogueEntryInUnityEditor[] postKadeFadeDialogue;
    [SerializeField] private DialogueEntryInUnityEditor[] kadeAndOthersDialogue;
    [SerializeField] private DialogueEntryInUnityEditor[] everyoneDialogue;

    private IEnumerator Start()
    {
        screenShakeHandler.DynamicCamera = dynamicCamera; // Suboptimal
        UIFadeScreenManager.Instance.SetDarkScreen();
        yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(preFightDialogue.Into());
        // TODO: Boulder audio and roaring
        yield return DialogueBoxV2.Instance.Play(defaultBackgroundDialogue.Into());
        // TODO: Shake and roar
        
        // [Fade into Dead Creature background with Jackie and creature]
        yield return FadeOutSpriteRenderer(2, defaultBackground);
        yield return DialogueBoxV2.Instance.Play(deadCreatureDialogue.Into());
        // TODO: Start blackening edges of the screen
        yield return DialogueBoxV2.Instance.Play(blackingOut1.Into());
        // TODO: Blackening gets worse
        yield return DialogueBoxV2.Instance.Play(blackingOut2.Into());
        yield return FadeOutSpriteRenderer(2, deadCreatureBackground); // Fade into black screen without hiding dialogue
        yield return new WaitForSeconds(1f); // Small delay between black screen and next dialogue
        
        yield return DialogueBoxV2.Instance.Play(bonePopping.Into());
        yield return FadeOutSpriteRenderer(2, blackScreen);
        yield return new WaitForSeconds(1f);
        
        yield return DialogueBoxV2.Instance.Play(preKadeFadeDialogue.Into());
        yield return DialogueBoxV2.Instance.Play(postKadeFadeDialogue.Into());
        
        yield return FadeOutSpriteRenderer(2, wakeUpBackground);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(kadeAndOthersDialogue.Into());
        
        yield return FadeOutSpriteRenderer(2, reversedCaveBackground);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(everyoneDialogue.Into());
        yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
    }

    private IEnumerator FadeOutSpriteRenderer(float time, SpriteRenderer[] spriteRenderers) {
        float curTime = 0;
        while (curTime < time)
        {
            curTime += Time.deltaTime;
            foreach (SpriteRenderer sr in spriteRenderers) 
                sr.color = new Color(sr.color.r,sr.color.g,sr.color.b, 1 - curTime / time);
            yield return null;
        }
        foreach (SpriteRenderer sr in spriteRenderers) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
    }
}
