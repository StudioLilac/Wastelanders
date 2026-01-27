using DialogueScripts;
using System.Collections;
using UnityEngine;

public class Epilogue_1 : MonoBehaviour
{
    [SerializeField] private UIFadeHandler fader;
    [SerializeField] private DialogueEntryWrapper wrapper;
    [SerializeField] private CanvasGroupFadeHandler comingSoon;

    private IEnumerator Start()
    {
        comingSoon.SetLightScreen();
        yield return fader.FadeInLightScreen(2f);
        yield return new WaitForSeconds(1f);
        yield return DialogueBoxV2.Instance.Play(wrapper);
        yield return fader.FadeInDarkScreen(2f);
        yield return comingSoon.FadeInDarkScreen(2f);
        GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.MainMenu>().SceneName);
    }


    [SerializeField]
    private float cycleScaling = 2f; // Higher the number, the faster one phase is 
    [SerializeField]
    private float bobbingAmount = 500f; //Amplitude
    private float timer = 0;
    private float verticalOffset = 0;

    //Makes the game over text bob up and down!
    void Update()
    {
        float previousOffset = verticalOffset;
        float waveslice = Mathf.Sin(cycleScaling * timer);
        timer += Time.deltaTime;
        if (timer > Mathf.PI * 2)
        {
            timer = timer - (Mathf.PI * 2);
        }

        verticalOffset = waveslice * bobbingAmount;
        float translateChange = verticalOffset - previousOffset;
        comingSoon.transform.position = new Vector3(comingSoon.transform.position.x,
        comingSoon.transform.position.y + translateChange, comingSoon.transform.position.z);
    }
}
