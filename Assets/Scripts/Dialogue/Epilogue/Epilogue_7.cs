using System.Collections;
using Cinemachine;
using DialogueScripts;
using UnityEngine;
using UnityEngine.Serialization;

public class Epilogue_7 : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] defaultBackground;
    [SerializeField] private SpriteRenderer[] deadCreatureBackground;

    [SerializeField] private SpriteRenderer blackingOutSr1;
    [SerializeField] private SpriteRenderer blackingOutSr2;
    [SerializeField] private SpriteRenderer[] blackScreen; // Used for a black screen, keeping dialogue visible
    [SerializeField] private SpriteRenderer[] wakeUpBackground;
    [SerializeField] private SpriteRenderer[] reversedCaveBackground;
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
#nullable enable

    private void Awake()
    {
        this.Answer<GetActiveCamera, CinemachineVirtualCamera?>(evt => dynamicCamera);}

    private IEnumerator Start()
    {
        UIFadeScreenManager.Instance.SetDarkScreen();
        yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(preFightDialogue.Into());
        // TODO: Boulder audio and roaring
        yield return DialogueBoxV2.Instance.Play(defaultBackgroundDialogue.Into());
        // TODO: Shake and roar
        new ShakeScreen(Intensity: 0.8f).Invoke();
        
        // [Fade into Dead Creature background with Jackie and creature]
        yield return FadeOutSpriteRenderers(2, defaultBackground);
        yield return DialogueBoxV2.Instance.Play(deadCreatureDialogue.Into());
        blackingOutSr1.gameObject.SetActive(true);
        yield return FadeInSpriteRenderer(1f, blackingOutSr1);
        yield return DialogueBoxV2.Instance.Play(blackingOut1.Into());
        blackingOutSr2.gameObject.SetActive(true);
        yield return FadeInSpriteRenderer(1f, blackingOutSr2);
        yield return DialogueBoxV2.Instance.Play(blackingOut2.Into());
        yield return FadeOutSpriteRenderers(2, deadCreatureBackground); // Fade into black screen without hiding dialogue
        blackingOutSr1.gameObject.SetActive(false);
        blackingOutSr2.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f); // Small delay between black screen and next dialogue
        
        // TODO: Sound of a bone popping in place
        yield return DialogueBoxV2.Instance.Play(bonePopping.Into());
        yield return FadeOutSpriteRenderers(2, blackScreen);
        yield return new WaitForSeconds(1f);
        
        yield return DialogueBoxV2.Instance.Play(preKadeFadeDialogue.Into());
        yield return DialogueBoxV2.Instance.Play(postKadeFadeDialogue.Into());
        
        yield return FadeOutSpriteRenderers(2, wakeUpBackground);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(kadeAndOthersDialogue.Into());
        
        yield return FadeOutSpriteRenderers(2, reversedCaveBackground);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(everyoneDialogue.Into());
        yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
    }



    private IEnumerator FadeInSpriteRenderer(float time, SpriteRenderer sr) {
        float curTime = 0;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        while (curTime < time)
        {
            curTime += Time.deltaTime;
            sr.color = new Color(sr.color.r,sr.color.g,sr.color.b, curTime / time);
            yield return null;
        }
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
    }

    private IEnumerator FadeOutSpriteRenderers(float time, SpriteRenderer[] spriteRenderers) {
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
