using Codice.CM.Triggers;
using DialogueScripts;
using Entities;
using FMOD.Studio;
using FMODUnity;
using NUnit.Framework.Constraints;
using Particles;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattleIntroEnum;

namespace Dialogue.Epilogue
{
    public class Epilogue_10 : DialogueClasses
    {
        private static Color PURPLE = new Color(210f / 255f, 175f / 255f, 1f, 1f);
        private static Color TRANSPARENT_PURPLE = new Color(210f / 255f, 175f / 255f, 1f, 0f);

        [SerializeField] private bool jumpToCombat;
        [SerializeField] private bool instakill;

        [SerializeField] private GameObject background1;
        [SerializeField] private GameObject background2;
        [SerializeField] private GameObject world;
        [SerializeField] private UIFadeHandler scrim;
        [SerializeField] private UIFadeHandler fireplace;
        [SerializeField] private ScalingLerpHandler zoomer;

        [SerializeField] private Image purpleFlash;
        [SerializeField] private Image tundraWithNoise;
        [SerializeField] private Image caveFlickerLayer;
        private bool shouldFlicker = false;

        [SerializeField] private Jackie jackie;
        [SerializeField] private EnemyIves ives;
        [SerializeField] private PrincessFrog princess;

        [SerializeField] private DialogueEntryInUnityEditor[] jayOpeningDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] preBonfireDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] postVisionDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] postBonfireDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] momStoryFlashbackDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] weiseDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] spottedPFrogDialogue;
        [SerializeField] private DialogueEntryInUnityEditor[] battleStartDialogue;

        [SerializeField] private Transform jackieReturnPosition;
        [SerializeField] private Transform ivesReturnPosition;
        [SerializeField] private Transform princessReturnPosition;
        
        [SerializeField] private AudioClip campfireBg;
        [SerializeField] private AudioClip tundraBg;
        [SerializeField] private AudioClip ivesSignature;
        [SerializeField] private AudioClip crystalHum;

        [SerializeField] private StudioEventEmitter bossfightTrackEmitter;
        [SerializeField] private Blizzard blizzardParticles;
        [SerializeField] private SmokeScreenOverlay smokeScreen;
        
        [SerializeField] private List<GameObject> ivesActions;

        private float justStartedFlickering = 0f;
        
        private void Update()
        {
            if (!shouldFlicker) { return; }
            if (justStartedFlickering == 0f)    
                justStartedFlickering = Time.time;
            caveFlickerLayer.color = new Color(1f, 1f, 1f, 0.5f + 0.3f * Mathf.Sin(Time.time - justStartedFlickering));
        }

        protected override void GameStateChange(GameState gameState)
        {
            if (gameState == GameState.GAME_START)
            {
                StartCoroutine(
                    ExecuteSceneStart());
            }
        }

        void Setup() {
            jackie.OutOfCombat();
            ives.OutOfCombat();
            princess.OutOfCombat();
            this.Subscribe<PrincessFrog.PrincessFrogHurtEvent>(OnPrincessFrogHurt);
        }

        private IEnumerator ExecuteSceneStart()
        {
            Setup();
            CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
            scrim.SetDarkScreen();
            purpleFlash.color = TRANSPARENT_PURPLE;
            caveFlickerLayer.color = new Color(1f, 1f, 1f, 0f);
            jackie.SetReturnPosition(jackieReturnPosition.position);
            ives.SetReturnPosition(ivesReturnPosition.position);
            princess.SetReturnPosition(princessReturnPosition.position);
            blizzardParticles.SetIntensity(0.25f);

            if (!GameStateManager.Instance.JumpToCombat && !jumpToCombat)
            {
                ControllableAudioChannel backgroundWind = AudioManager.Instance.CreateChannel(tundraBg, AudioCategory.Music, level: 0f);
                backgroundWind.Play();
                ControllableAudioChannel ivesBlips = AudioManager.Instance.CreateChannel(ivesSignature, AudioCategory.Music, level: 0f);
                ivesBlips.Play();
                { 
                    StartCoroutine(backgroundWind.FadeTo(0.3f, 1.5f));
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.Opening);
                }
                {
                    StartCoroutine(backgroundWind.FadeTo(1f, 1f));
                    yield return scrim.FadeInLightScreen(1.5f);
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.Approach(this, ivesBlips));
                    yield return new WaitForSeconds(1f);
                    StartCoroutine(fireplace.FadeToAlpha(0.5f, 1.5f));
                    yield return zoomer.FadeInLightScreen(1.5f);
                    shouldFlicker = true;
                    StartCoroutine(backgroundWind.FadeTo(0f, 1.5f));
                    StartCoroutine(ivesBlips.FadeTo(0f, 1.5f));
                    yield return new WaitForSeconds(1f);
                }

                ControllableAudioChannel campfireBackground = AudioManager.Instance.CreateChannel(campfireBg, AudioCategory.Music, level: 0f);
                campfireBackground.Play();
                {
                    StartCoroutine(campfireBackground.FadeTo(1f, 1.5f));
                    yield return new WaitForSeconds(2f);
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.Bonfire);
                    yield return new WaitForSeconds(0.5f);
                }

                {
                    yield return scrim.FadeInDarkScreen(1.5f);
                    yield return new WaitForSeconds(0.5f);
                    yield return new DialogueAsCode().Exit(0f, DialogueCharacter.Jackie, DialogueCharacter.Rocky).Play(); // Will cause flicker. 
                    background2.SetActive(true);
                    yield return new WaitForSeconds(0.5f);
                    yield return scrim.FadeToAlpha(0f, 1.5f);
                    StartCoroutine(backgroundWind.FadeTo(1f, 1.5f));
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.AilinStory(this, crystalHum, smokeScreen));
                    StartCoroutine(backgroundWind.FadeTo(0f, 1f));
                    yield return scrim.FadeInDarkScreen(1.5f);
                    yield return new WaitForSeconds(0.5f);
                    yield return new DialogueAsCode().Enter(DialogueCharacter.Jackie, CharacterActions.SetLeft, fadeDuration: 0).Enter(DialogueCharacter.Rocky, CharacterActions.SetRight, fadeDuration: 0).Play();
                    background2.SetActive(false);
                    yield return new WaitForSeconds(0.5f);
                    yield return scrim.FadeToAlpha(0f, 1.5f);
                }

                {
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.AfterAilinStory);
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.JayReport);
                    yield return new WaitForSeconds(1f);
                    yield return scrim.FadeInDarkScreen(1.5f);
                    yield return new WaitForSeconds(0.5f);
                    SoundID.VN_purple_pulse.Play();
                    yield return new WaitForSeconds(1.5f);
                    yield return scrim.FadeToAlpha(0f, 1.5f);
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.NitesArrival);
                }



                // After deduction work.
                yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
                background1.SetActive(false);
                purpleFlash.color = TRANSPARENT_PURPLE;
                AudioManager.Instance.FadeInBackgroundTrack(2f, tundraBg, true);
                ives.FaceLeft();
                princess.FaceLeft();
                yield return UIFadeScreenManager.Instance.FadeInLightScreen(2f);
                yield return DialogueBoxV2.Instance.Play(spottedPFrogDialogue.Into());
                StartCoroutine(jackie.MoveToPosition(new Vector3(2.75f, 0.4f, jackie.transform.position.z), 0f, 2f));

                yield return new WaitForSeconds(2);
                ives.AttackAnimation("IsPunching");
                AudioManager.Instance.PlaySFX(SoundID.CB_fist_hit);
                yield return StartCoroutine(jackie.StaggerEntities(ives, jackie, 0.4f));
                yield return DialogueBoxV2.Instance.Play(battleStartDialogue.Into());
            } else {
                ives.FaceLeft();
                princess.FaceLeft();
                background2.SetActive(false);
                background1.SetActive(false);
                purpleFlash.color = TRANSPARENT_PURPLE;
                GameStateManager.Instance.JumpToCombat = false;
                yield return UIFadeScreenManager.Instance.FadeInLightScreen(0.5f);
            }

            new BattleIntroEvent(Get<ClashIntro>()).Invoke();
            
            CombatManager.PlayersWinEvent += PlayersWin;
            CombatManager.EnemiesWinEvent += EnemiesWin;

            princess.OwnedMinions.Add(ives);
            ives.InjectDeck(ivesActions);
            bossfightTrackEmitter.Play();

            if (instakill) {
                jackie.AddStacks(Accuracy.buffName, 5);
                jackie.AddStacks(Resonate.buffName, 5);
            }
            
            CombatManager.Instance.BeginCombat();
            
            yield return new WaitUntil(() => new GetGameState().Query() == GameState.GAME_WIN);
            EventInstance instance = bossfightTrackEmitter.EventInstance;
            instance.setParameterByNameWithLabel("BossState", "Defeated");
            
            CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
            
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
        }

        private void PlayersWin()
        {
            CombatManager.EnemiesWinEvent -= EnemiesWin;
            CombatManager.PlayersWinEvent -= PlayersWin;
            CombatManager.Instance.GameState = GameState.GAME_WIN;
        }

        private void EnemiesWin()
        {
            CombatManager.EnemiesWinEvent -= EnemiesWin;
            CombatManager.PlayersWinEvent -= PlayersWin;
            CombatManager.Instance.GameState = GameState.GAME_LOSE;
        }

        private void OnPrincessFrogHurt(PrincessFrog.PrincessFrogHurtEvent e)
        {
            switch (e.RemainingHealth)
            {
                case > 35:
                    blizzardParticles.SetIntensity(0.25f);
                    break;
                case > 20:
                    blizzardParticles.SetIntensity(0.5f);
                    break;
                case > 10:
                    blizzardParticles.SetIntensity(0.75f);
                    break;
                default:
                    blizzardParticles.SetIntensity(1f);
                    break;
            }
        }

        private static class Epilogue10Dialogue
        {
            public static DialogueAsCode Opening => new DialogueAsCode()
                .Line(DialogueCharacter.Jay, "Feel that breeze picking up? There’s an opening up ahead!");

            public static DialogueAsCode Approach(MonoBehaviour owner, ControllableAudioChannel ivesBlips) => new DialogueAsCode()
                .Line(DialogueCharacter.Rocky, "How close are we to the Frog, Jackie?")
                .Line(DialogueCharacter.Jackie, "It's up ahead and out the opening. We’ve reached the same elevation.")
                .Line(DialogueCharacter.Rocky, "Then we should make preparations for battle.")
                .Line(DialogueCharacter.Rocky, "Jay, take the team up to the vanguard and see if you can get eyes on our men.")
                .Line(DialogueCharacter.Rocky, "The rest of us will set up camp and secure the rear.")
                .Line(DialogueCharacter.Jay, "On it Captain.")
                .Line(DialogueCharacter.Rocky, "Jackie, could you give Jay your tracker?")
                .Do(new CallbackEvent(() => owner.StartCoroutine(ivesBlips.FadeTo(0.8f, 0f))))
                .Line(DialogueCharacter.Jackie, "Just one sec. I’m getting another reading.")
                .Line(DialogueCharacter.Rocky, "Did the signal change again? A new direction?")
                .Line(DialogueCharacter.Jackie, "No, like. A separate reading on my tracker, near the base of the mountain I’d say.")
                .Line(DialogueCharacter.Rocky, "Don’t tell me there are more frogs out there...")
                .Line(DialogueCharacter.Jackie, "I don’t think so, the signature is quite different from the Frog we’ve been tracking.")
                .Line(DialogueCharacter.Jackie, "But we’re picking it up because it’s on the same tone we’re tracking.")
                .Line(DialogueCharacter.Jackie, "It’s as if something is trying to catch our attention...")
                .Line(DialogueCharacter.Jackie, "Or someone.")
                .Line(DialogueCharacter.Rocky, "What do you mean?")
                .Line(DialogueCharacter.Jackie, "If I’m not mistaken, this could be a NITES team.")
                .Line(DialogueCharacter.Rocky, "NITES... How do they know we’re here? Did you call them?")
                .Line(DialogueCharacter.Jackie, "What? No I haven't. These mountains are a signal dead zone.")
                .Line(DialogueCharacter.Jackie, "...They must have followed the last set of coordinates I sent them when I head out.")
                .Line(DialogueCharacter.Jackie, "I think they’re just looking for me. I don’t think they know you guys are out here. ")
                .Line(DialogueCharacter.Rocky, "...")
                .Line(DialogueCharacter.Jackie, "...Isn’t this good news? We could get help. ")
                .Line(DialogueCharacter.Rocky, "No. We are not getting help from them. We’ll handle it on our own.")
                .Line(DialogueCharacter.Jackie, "But why—")
                .Narrate("<i>Jackie glances to the side at Kade, she’s rummaging through her medical bag, counting and taking stock.</i>")
                .Line(DialogueCharacter.Jackie, "...") 
                .Line(DialogueCharacter.Jackie, "Okay, I gotcha.")
                .Narrate("<i>Jackie takes off the glove with the tracker. </i>")
                .Line(DialogueCharacter.Jackie, "...Hey Jay. You’ve seen me operate the tracker right?")
                .Line(DialogueCharacter.Jay, "It’s all up in here.")
                .Narrate("<i>Jackie looks at her glove for a breath, before handing it to Jay.</i>")
                .Line(DialogueCharacter.Jay, "Thanks, scout.")
                .Narrate("<i>Jay receives the gloves, and dons them methodically.</i>")
                .Narrate("<i>Eventually, the vanguard sets out, and the rest settle the outpost.</i>")
                .Narrate("<i>Before long, a large bonfire crackles in the center.</i>")
                .Exit(DialogueCharacter.Jay);

            public static DialogueAsCode Bonfire => new DialogueAsCode()
                .Narrate("<i>Preparing for battle, Jackie takes out a clean cloth to wipe down her staff as Rocky approaches.</i>")
                .Enter(DialogueCharacter.Rocky, CharacterActions.SetRight, DialogueSprite.RockySerious)
                .Line(DialogueCharacter.Rocky, "Jackie?")
                .Enter(DialogueCharacter.Jackie, CharacterActions.SetLeft, DialogueSprite.JackieDowncast)
                .Line(DialogueCharacter.Jackie, "Yes?")
                .Line(DialogueCharacter.Rocky, "I appreciate you giving Jay your tracker back there.")
                .Line(DialogueCharacter.Jackie, "Yeah?", DialogueSprite.JackieContemplative)    
                .Line(DialogueCharacter.Rocky, "You didn't push about the NITES back there either.")
                .Line(DialogueCharacter.Jackie, "...You said no.", DialogueSprite.JackieDowncast)
                .Line(DialogueCharacter.Rocky, "I say no all the time.")
                .Line(DialogueCharacter.Rocky, "It's nice not to be asked why.")
                .Line(DialogueCharacter.Jackie, "...")
                .Line(DialogueCharacter.Rocky, "We have some bad blood against HQ.")
                .Line(DialogueCharacter.Rocky, "After they found out what happened to the scouts, they wanted to leave them behind.")
                .Line(DialogueCharacter.Rocky, "So we didn’t go back either.")
                .Line(DialogueCharacter.Jackie, "...")
                .Line(DialogueCharacter.Rocky, "I think you should hear about what happened to your mother. You okay for that?")
                .Line(DialogueCharacter.Jackie, "...Yeah, I think so.", DialogueSprite.JackieContemplative)
                .Narrate("<i>Jackie stops shining her staff. Her eyes drift to the wood embers in the bonfire as Rocky starts.</i>");

            public static DialogueAsCode AilinStory(MonoBehaviour owner, AudioClip crystalHum, SmokeScreenOverlay smokeScreen)
            {
                var hum1 = AudioManager.Instance.CreateChannel(crystalHum, AudioCategory.Music, level: 0f);
                var hum2 = AudioManager.Instance.CreateChannel(crystalHum, AudioCategory.Music, level: 0f);
                var hum3 = AudioManager.Instance.CreateChannel(crystalHum, AudioCategory.Music, level: 0f);
                hum1.Play(); hum2.Play(); hum3.Play();
                hum1.SlowTempo(0.6f, 0f); hum2.SlowTempo(0.4f, 0f); hum3.SlowTempo(0.8f, 0f);
                return new DialogueAsCode()
                .Line(DialogueCharacter.Rocky, "It was dark, cold, and breezeless the night she disappeared.")
                .Line(DialogueCharacter.Rocky, "The first watch took their posts. The medical team settled in for a long night with the scouts rescued that day.")
                .Line(DialogueCharacter.Rocky, "From the reports, it happened minutes after the midnight dosage.")
                .Do(new CallbackEvent(() => owner.StartCoroutine(hum1.FadeTo(0.6f, 1.5f))))
                .Line(DialogueCharacter.Rocky, "After injecting the serums, the patients began to emit a faint humming sound.") // PLay humming sound
                .Do(new CallbackEvent(() => owner.StartCoroutine(hum2.FadeTo(0.6f, 1.5f))))
                .Line(DialogueCharacter.Rocky, "Apparently, the serums carried way more energy than they should have.")
                .Do(new CallbackEvent(() => owner.StartCoroutine(hum3.FadeTo(0.6f, 1.5f))))
                .Line(DialogueCharacter.Rocky, "Kade says it caused microscopic crystals in each infected scouts to shatter, which set off a chain reaction.")
                .Do(new CallbackEvent(() => {
                    hum3.RestoreTempo(1.5f); hum2.RestoreTempo(1.5f); hum1.RestoreTempo(1.5f);
                }))
                .Line(DialogueCharacter.Rocky, "By the time the medical team understood what they were hearing, the humming from each of them merged into one tone.")
                .Do(new CallbackEvent(() => {
                    IEnumerator ScheduleEvent()
                    {
                        owner.StartCoroutine(hum1.FadeTo(1f, 1.5f));
                        owner.StartCoroutine(hum3.FadeTo(1f, 1.5f));
                        owner.StartCoroutine(hum2.FadeTo(1f, 1.5f));
                        yield return new WaitForSeconds(4f);
                        hum1.Dispose(); hum2.Dispose(); hum3.Dispose();
                    }
                    owner.StartCoroutine(ScheduleEvent());
                }))
                .Line(DialogueCharacter.Rocky, "And then they woke up. The medical tent in the middle of camp came apart in seconds.")
                .Line(DialogueCharacter.Rocky, "We scrambled to contain the threat, but every protocol we had was meant for something coming from outside in.")
                .Line(DialogueCharacter.Rocky, "So Ailin did what Ailin did. Ordered everyone back. Handled it alone.")
                .Do(new CallbackEvent(() =>  smokeScreen.Deploy(new Vector2(0.5f, 0f))))
                .Line(DialogueCharacter.Rocky, "She deployed smoke screens around camp that we didn’t even know were there.", sfx: SoundID.VN_Smoke_Release)
                .Line(DialogueCharacter.Rocky, "The ash plume smothered the lights, the campfire, and both us and the enemy alike.")
                .Line(DialogueCharacter.Rocky, "No one but her had the training to fight in that. So we pulled out with as many non-combatants as we could gather.")
                .Line(DialogueCharacter.Rocky, "There we were, loading the unconscious onto the Jeeps, listening for every pitch drop in the humming.")
                .Line(DialogueCharacter.Rocky, "At some point, the sound of the rolling Jeeps took over, and we stopped listening for it.")
                .Do(new CallbackEvent(() => smokeScreen.FadeOut()))
                .Line(DialogueCharacter.Rocky, "We figured it was sorted, but when the smoke settled, Ailin was nowhere.")
                .Line(DialogueCharacter.Rocky, "We counted and double counted what we could. But not even Jay could get the headcount to add up.")
                .Line(DialogueCharacter.Rocky, "That’s how we lost her.");
            }

            public static DialogueAsCode AfterAilinStory => new DialogueAsCode()
                .Line(DialogueCharacter.Jackie, "...", DialogueSprite.JackieDowncast)
                .Line(DialogueCharacter.Jackie, "...You mentioned before that you were betrayed?")
                .Line(DialogueCharacter.Rocky, "...I don’t have a name. It was whoever tampered with the serums.")
                .Narrate("<i>Jackie looks down at the staff she stopped shining, catching her pinched reflection.</i>")
                .Line(DialogueCharacter.Jackie, "So you really don’t have a clue where Ma went?", DialogueSprite.JackieContemplative)
                .Line(DialogueCharacter.Rocky, "None that I can give you.")
                .Narrate("<i>Jackie lets out a long breath, one that she’d been holding for some time.</i>")
                .Line(DialogueCharacter.Jackie, "...I see.", DialogueSprite.JackieDowncast);

            public static DialogueAsCode JayReport => new DialogueAsCode()
                .Narrate("<i>As Jackie sits in silence, there is some commotion at the cave exit. Jay is back.</i>")
                .Enter(DialogueCharacter.Jay, CharacterActions.SetMiddle, DialogueSprite.JaySerious)
                .Line(DialogueCharacter.Jay, "Captain, I’ve got good news and bad.", DialogueSprite.JaySerious)
                .Line(DialogueCharacter.Rocky, "Let’s hear the good first.", DialogueSprite.RockySerious)
                .Line(DialogueCharacter.Jay, "Well, the team’s all down there, still alive and whole.", DialogueSprite.JaySerious)
                .Line(DialogueCharacter.Rocky, "And the bad?", DialogueSprite.RockySerious)
                .Line(DialogueCharacter.Jay, "It’s a Crystal field. It's freezing. And it’s crawling with Waste creatures.", DialogueSprite.JaySerious)
                .Line(DialogueCharacter.Jay, "We're outnumbered. We’re already one to one with our own scouts, without counting the creatures.")
                .Line(DialogueCharacter.Rocky, "Is there any way to isolate them? Get them out one by one?", DialogueSprite.RockySerious)
                .Line(DialogueCharacter.Jay, "Not likely. They're spread out, so getting to one might alert the others.", DialogueSprite.JaySerious)
                .Line(DialogueCharacter.Rocky, "Then it all comes down to manpower... There’s gotta be a solution here.", DialogueSprite.RockySerious)
                .Line(DialogueCharacter.Jay, "...")
                .Line(DialogueCharacter.Jay, "... If it’s a question of manpower. We do have an option. But you might not like it.")
                .Line(DialogueCharacter.Rocky, "If it gets our guys out of the cold, I’ll like it fine.")
                .Line(DialogueCharacter.Jay, "The NITES. I’m sure they’ll be willing to lend us a hand. If not for us, for Jackie.")
                .Line(DialogueCharacter.Rocky, "What, but what if...")
                .Line(DialogueCharacter.Rocky, "No, you know what. We’ll put it to a vote.")
                .Narrate("<i>Rocky stands up and addresses the rest of the camp. Explaining the situation.</i>")
                .Line(DialogueCharacter.Rocky, "All in favour of contacting the NITES, say aye.")
                .Exit(DialogueCharacter.Jackie, DialogueCharacter.Jay, DialogueCharacter.Rocky);

            public static DialogueAsCode NitesArrival => new DialogueAsCode()
                .Narrate("<i>Chatter fills the cave.</i>")
                .Narrate("<i>Soon, the atmosphere lightens, as flickers of recognition turn to smiles in the cave.</i>")
                .Enter(DialogueCharacter.Rocky, CharacterActions.SetLeft, DialogueSprite.RockySerious, fadeDuration: 0f)
                .Enter(DialogueCharacter.Ives, CharacterActions.SetRight, DialogueSprite.IvesSmile)
                .Line(DialogueCharacter.Ives, "So y’all need muscle eh? C’mere, I’ve got some for you.", DialogueSprite.IvesSmile)
                .Line(DialogueCharacter.Rocky, "Ives!? You’re here too.")
                .Line(DialogueCharacter.Ives, "Sure am, what’s up? The little bird didn’t say I’d be coming?")
                .Narrate("<i>Rocky, in a headlock, cranes his neck to give Jackie a curious look.</i>")
                .Exit(0.5f, DialogueCharacter.Rocky)
                .Move(DialogueCharacter.Ives, CharacterActions.SetLeft)
                .Enter(DialogueCharacter.Kade, CharacterActions.SetRight, DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Kade, "Ives. You look terrible.", DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Ives, "Hah, it’s nice to see you too, Kade.", DialogueSprite.IvesSmile)
                .Line(DialogueCharacter.Kade, "Skin is graying. Pupils dilated. Tremors. You’re on high-grade amplitude suppressants.", DialogueSprite.KadeSoft)
                .Line(DialogueCharacter.Ives, "Perceptive as always.", DialogueSprite.IvesSmile)
                .Line(DialogueCharacter.Ives, "Guilty as charged. Got tangled up in a little Waste incident.", DialogueSprite.IvesNeutral)
                .Line(DialogueCharacter.Kade, "You’re in great danger. Did any Crystals break near you on your way here?")
                .Line(DialogueCharacter.Ives, "A couple, but at a distance. I tried to avoid ‘em, but a scuffle is a scuffle.")
                .Line(DialogueCharacter.Kade, "How did you feel?")
                .Line(DialogueCharacter.Ives, "There was a bit of a flare up, but the Doc adjusted my dose and I’ve been fine since.")
                .Line(DialogueCharacter.Kade, "Get the adjustment wrong and the dose will either be worthless or kill you.", DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Ives, "Good thing he’s an expert.", DialogueSprite.IvesSmile)
                .Line(DialogueCharacter.Ives, "Hey, Doc! Front and center.", DialogueSprite.IvesNeutral)
                .Exit(0.5f, DialogueCharacter.Ives)
                .Enter(DialogueCharacter.Weise, CharacterActions.SetLeft, DialogueSprite.WeiseNeutral)
                .Line(DialogueCharacter.Weise, "...", DialogueSprite.WeiseNeutral)
                .Narrate("<i>Dr. Weise steps out from the shadows. His face is neutral and hands dug stiffly into the pockets of his lab coat.</i>")
                .Line(DialogueCharacter.Weise, "Now’s hardly the time for acclaims, Commander.", DialogueSprite.WeiseNeutral)
                .Narrate("<i>Kade freezes, her face growing pale.</i>")
                .Line(DialogueCharacter.Kade, "...You. Aleksander.", DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Weise, "Kade.", DialogueSprite.WeiseThinking)
                .Line(DialogueCharacter.Kade, "You’re alive. I... mourned you.", DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Weise, "I did the same.")
                .Line(DialogueCharacter.Kade, "... How far did you get? Your research to “go up”.")
                .Line(DialogueCharacter.Weise, "Nowhere. Over a decade, and nothing.")
                .Line(DialogueCharacter.Kade, "Then the slow way down was the only way there was.")
                .Line(DialogueCharacter.Kade, "The ones who didn't make it... there was no faster way after all.")
                .Line(DialogueCharacter.Weise, "...")
                .Narrate("<i>As Kade’s face softens, Jackie comes up beside her.</i>")
                .Move(DialogueCharacter.Kade, CharacterActions.SetMiddle)
                .Enter(DialogueCharacter.Jackie, CharacterActions.SetOffscreenRight, DialogueSprite.NoChange, 0f)
                .Move(DialogueCharacter.Jackie, CharacterActions.SetRight);

        }
    }
}
