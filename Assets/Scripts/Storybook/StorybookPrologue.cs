using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Dialogue;
using DialogueScripts;

namespace Storybook
{
    public class StorybookPrologue : MonoBehaviour
    {
        public UIFadeHandler scrim;

        private IEnumerator Start()
        {
            scrim.SetDarkScreen();
            yield return StartCoroutine(scrim.FadeInLightScreen(1.5f));

            yield return DialogueBoxV2.Instance.Play(StorybookPrologue_Dialogue.Flashback);

            yield return StartCoroutine(scrim.FadeInDarkScreen(1.5f));
            GameStateManager.Instance.StorybookEntryNode = StorybookSceneEnum.Storybook1;
            GameStateManager.Instance.LoadScene("StorybookScene");
        }
    }

    public static class StorybookPrologue_Dialogue
    {
        public static DialogueAsCode Flashback => new DialogueAsCode()
            .Line(DialogueCharacter.Jackie, "I am ready. It’s time for me to stop being so useless and go out and fight.")
            .Line(DialogueCharacter.Ives, "Feeling ready and being ready ain’t the same. You still need more training.")
            .Line(DialogueCharacter.Jackie, "More training? No, no more training! You’re not my mom, you can’t tell me what to do!")
            .Line(DialogueCharacter.Ives, "No. But your mom ain’t here. It’s just me.")
            .Narrate("<i>In a whirl of frustration, Jackie storms off to her bedroom, slamming the door behind her.</i>")
            .Narrate("<i>Ives exhales, shaking her head. She catches her reflection in the window, seeing her tattoo banded across her arm.</i>")
            .Narrate("<i>Ives tries the handle of Jackie’s door, finding it locked. Her hand falls, and she turns to leave, but her feet feel stuck in place. She rests her forehead on the door.</i>")
            .Line(DialogueCharacter.Ives, "You gotta let me in.")
            .Line(DialogueCharacter.Jackie, "Why?")
            .Narrate("<i>Ives can’t think of a good reason.</i>")
            .Line(DialogueCharacter.Ives, "Cause your mom... No. Cause I...")
            .Narrate("<i>Ives folds her arms. Her fingers land on the vines that sprawl over her skin. She lets out a breath.</i>")
            .Line(DialogueCharacter.Ives, "Did I ever tell you the story of my tattoo?")
            .Line(DialogueCharacter.Ives, "I was raised by soldiers. A small platoon manning the west wall, who were willing to spare some rations and patience for a rough and tumble orphan... And there was this story.")
            .Narrate("<i>The door cracks open. Jackie stands there, bundled in blankets so that just a little bit of her face is visible. Her cheeks are red and streaked with tears.</i>")
            .Line(DialogueCharacter.Jackie, "*sniffling* I like stories.")
            .Line(DialogueCharacter.Ives, "I- right, it goes something like this...")
            ;
    }
}