using DialogueScripts;
using Entities;
using FMOD.Studio;
using FMODUnity;
using Particles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static BattleIntroEnum;

namespace Dialogue.Epilogue
{
    public class Epilogue_10 : DialogueClasses
    {
        private static Color PURPLE = new Color(99f / 255f, 0 / 255f, 84f / 255f, 1f);
        private static Color TRANSPARENT_PURPLE = new Color(99f / 255f, 0 / 255f, 84f / 255f, 0f);

        [SerializeField] private bool jumpToCombat;
        [SerializeField] private bool instakill;

        [SerializeField] private GameObject background1;
        [SerializeField] private GameObject background2;
        [SerializeField] private GameObject world;
        [SerializeField] private GameObject shaderBackground;
        [SerializeField] private UIFadeHandler scrim;
        [SerializeField] private UIFadeHandler fireplace;
        [SerializeField] private UIFadeHandler injectionOverlay;
        [SerializeField] private ScalingLerpHandler zoomer;
        [SerializeField] private ScalingLerpHandler zoomerShaderBg;
        [SerializeField] private Image purpleFlash;
        [SerializeField] private Image caveFlickerLayer;
        [SerializeField] private Sprite clearInjection;
        [SerializeField] private Sprite purpleInjection;
        [SerializeField] private Canvas backgroundOverlay;
        [SerializeField] private CanvasGroupFadeHandler canvasGroupFadeHandler;
        [SerializeField] private AnalogueHorrorEffect horrorEffect;
        [SerializeField] private Animator injectionClip;

        private bool shouldFlicker = false;

        [SerializeField] private Jackie jackie;
        [SerializeField] private EnemyIves ives;
        [SerializeField] private PrincessFrog princess;
        [SerializeField] private Transform jackieReturnPosition;
        [SerializeField] private Transform ivesReturnPosition;
        [SerializeField] private Transform princessReturnPosition;
        
        [SerializeField] private AudioClip campfireBg;
        [SerializeField] private AudioClip tundraBg;
        [SerializeField] private AudioClip ivesSignature;
        [SerializeField] private AudioClip crystalHum;
        [SerializeField] private AudioClip analogueHorror;

        [SerializeField] private StudioEventEmitter bossfightTrackEmitter;
        [SerializeField] private Blizzard blizzardParticles;
        [SerializeField] private SmokeScreenOverlay smokeScreen;
        
        [SerializeField] private List<GameObject> ivesActions;
        [SerializeField] private Epilogue_10_Cutscene cutscene;

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

        public IEnumerator PurpleFlash(float fadeInDuration = 0.5f, float fadeOutDuration = 2f)
        {
            SoundID.VN_purple_pulse.Play();
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float fraction = Mathf.Clamp01(elapsedTime / fadeInDuration);
                purpleFlash.color = Color.Lerp(TRANSPARENT_PURPLE, PURPLE, fraction);
                yield return null;
            }
            elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float fraction = Mathf.Clamp01(elapsedTime / fadeOutDuration);
                purpleFlash.color = Color.Lerp(PURPLE, TRANSPARENT_PURPLE, fraction);
                yield return null;
            }
            purpleFlash.color = TRANSPARENT_PURPLE;
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
            blizzardParticles.SetIntensity(0.05f);
            backgroundOverlay.sortingOrder = UISortOrder.CharacterActors.GetOrder();

            if (!GameStateManager.Instance.JumpToCombat && !jumpToCombat)
            {
                ControllableAudioChannel backgroundWind = AudioManager.Instance.CreateChannel(tundraBg, AudioCategory.Music, level: 0f);
                ControllableAudioChannel ivesBlips = AudioManager.Instance.CreateChannel(ivesSignature, AudioCategory.Music, level: 0f);
                backgroundWind.Play(); ivesBlips.Play();
                background1.SetActive(true); shaderBackground.SetActive(true);
                { 
                    StartCoroutine(backgroundWind.FadeTo(0.3f, 1.5f));
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.Opening);
                }
                {
                    StartCoroutine(backgroundWind.FadeTo(1f, 1f));
                    yield return scrim.FadeInLightScreen(1.5f);
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.Approach(this, ivesBlips));
                    yield return StartCoroutine(canvasGroupFadeHandler.FadeInDarkScreen(1f));
                    fireplace.gameObject.SetActive(true);
                    blizzardParticles.SetIntensity(0f);
                    StartCoroutine(fireplace.FadeToAlpha(0.5f, 1.5f));
                    StartCoroutine(zoomerShaderBg.FadeInLightScreen(1.5f));
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
                    yield return scrim.FadeInDarkScreen(1f);
                    yield return new WaitForSeconds(0.5f);
                    yield return new DialogueAsCode().Exit(0f, DialogueCharacter.Jackie, DialogueCharacter.Rocky).Play();
                    background2.SetActive(true);
                    yield return new WaitForSeconds(0.5f);
                    yield return scrim.FadeToAlpha(0f, 1.5f);
                    StartCoroutine(backgroundWind.FadeTo(1f, 1.5f));
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.AilinStory(this, crystalHum, smokeScreen));
                    StartCoroutine(backgroundWind.FadeTo(0f, 1f));
                    yield return scrim.FadeInDarkScreen(1f);
                    yield return new WaitForSeconds(0.5f);
                    yield return new DialogueAsCode().Enter(DialogueCharacter.Jackie, CharacterActions.SetLeft, fadeDuration: 0).Enter(DialogueCharacter.Rocky, CharacterActions.SetRight, fadeDuration: 0).Play();
                    background2.SetActive(false);
                    yield return new WaitForSeconds(0.5f);
                    yield return scrim.FadeToAlpha(0f, 1.5f);
                }

                var horrorAudio = AudioManager.Instance.CreateChannel(analogueHorror, AudioCategory.Music, level: 0f);
                horrorAudio.Play();
                {
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.AfterAilinStory);
                    StartCoroutine(backgroundWind.FadeTo(1f, 1.5f));
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.JayReport);
                    StartCoroutine(backgroundWind.FadeTo(0f, 1.5f));
                    yield return new WaitForSeconds(0.5f);
                    yield return scrim.FadeInDarkScreen(1f);
                    yield return new WaitForSeconds(0.5f);
                    SoundID.VN_purple_pulse.Play();
                    yield return new DialogueAsCode()
                        .Exit(0f, DialogueCharacter.Jackie, DialogueCharacter.Rocky, DialogueCharacter.Jay)
                        .Enter(DialogueCharacter.Rocky, CharacterActions.SetLeft, DialogueSprite.RockySerious, fadeDuration: 0f)
                        .Enter(DialogueCharacter.Ives, CharacterActions.SetMiddle, DialogueSprite.IvesSmile, fadeDuration: 0f)
                        .Move(DialogueCharacter.Ives, CharacterActions.FaceLeft, duration: 0f)
                        .Play();
                    ControllableAudioChannel talkChatter = AudioManager.Instance.CreateChannel(SoundID.VN_Talk_Cheer, AudioCategory.Music, level: 0f);
                    talkChatter.Play();
                    yield return new WaitForSeconds(1f);
                    StartCoroutine(talkChatter.FadeTo(1f, 1.5f));
                    yield return scrim.FadeToAlpha(0f, 1.5f);

                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.NitesArrival);
                    StartCoroutine(talkChatter.FadeTo(0f, 4.0f));
                    ControllableAudioChannel suspenseDrone = AudioManager.Instance.CreateChannel(SoundID.VN_BGM_suspense_drone, AudioCategory.Music, level: 0f);
                    suspenseDrone.Play(); 
                    StartCoroutine(suspenseDrone.FadeTo(1.2f, 4.0f));
                    shouldFlicker = false;
                    yield return StartCoroutine(fireplace.FadeInLightScreen(1f));
                    canvasGroupFadeHandler.SetLightScreen();
                    yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.Deduction(this, horrorAudio, horrorEffect));
                    StartCoroutine(campfireBackground.FadeTo(0f, 2f));
                    StartCoroutine(suspenseDrone.FadeTo(0f, 1.5f));
                    StartCoroutine(horrorAudio.FadeTo(0f, 4f));
                }
            
                yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
                yield return new WaitForSeconds(0.5f);
                AudioManager.Instance.FadeInBackgroundTrack(2f, tundraBg, true);
                shaderBackground.SetActive(false); injectionOverlay.gameObject.SetActive(false);
                blizzardParticles.SetIntensity(0.05f);
                horrorEffect.RampTo(0f, 0.5f);
                ives.FaceLeft();
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                ives.FaceLeft();
                background2.SetActive(false);
                background1.SetActive(false);
                shaderBackground.SetActive(false);
                scrim.SetLightScreen();
                purpleFlash.color = TRANSPARENT_PURPLE;
                GameStateManager.Instance.JumpToCombat = false;
                yield return new WaitForSeconds(0.5f);
            }
            StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(1.5f));
            bossfightTrackEmitter.Play();
            yield return cutscene.Play(this);

            new BattleIntroEvent(Get<ClashIntro>()).Invoke();
            this.Subscribe<TeamWinEvent>(OnTeamWin);

            princess.OwnedMinions.Add(ives);
            ives.InjectDeck(ivesActions);

            if (instakill) {
                jackie.AddStacks(Accuracy.buffName, 999);
                jackie.AddStacks(Resonate.buffName, 999);
            }
            
            EventInstance instance = bossfightTrackEmitter.EventInstance;
            instance.setParameterByNameWithLabel("BossState", "Fighting");
            blizzardParticles.SetIntensity(0.25f);
            CombatManager.Instance.BeginCombat();
            
            yield return new WaitUntil(() => new GetGameState().Query() == GameState.GAME_WIN);
            instance.setParameterByNameWithLabel("BossState", "Defeated");
            
            CombatManager.Instance.GameState = GameState.OUT_OF_COMBAT;
            
            yield return UIFadeScreenManager.Instance.FadeInDarkScreen(2f);
        }

        void OnTeamWin(TeamWinEvent ev)
        {
            if (ev.Team == EntityTeam.PlayerTeam)
            {
                new SetGameState(GameState.GAME_WIN).Invoke();
            }
            else
            {
                new SetGameState(GameState.GAME_LOSE).Invoke();
                GameOver.Instance.FadeInWithDialogue(new DialogueAsCode()
                    .Line(DialogueCharacter.Jackie, "Rocky, I'm pulling out. We'll handle this together.")
                );
            }
        }

        public IEnumerator InjectionSequence(ControllableAudioChannel horror)
        {
            yield return injectionOverlay.FadeInDarkScreen(0.5f);
            StartCoroutine(horror.FadeTo(1f, 2f));
            yield return new WaitForSeconds(1.5f);
            injectionClip.SetTrigger("InjectionClip");
            yield return new WaitForSeconds(0.3f);
            new ShakeScreen(0.8f).Invoke();
            yield return horrorEffect.Burst(0.6f);
            yield return horrorEffect.RampTo(0.3f, 1f);
            yield return new WaitForSeconds(1f);
            yield return injectionOverlay.FadeInLightScreen(0.5f);
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

        public static class Epilogue10Dialogue
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
                .Enter(DialogueCharacter.Rocky, CharacterActions.SetRight, DialogueSprite.RockySerious, fadeDuration: 0.5f)
                .Line(DialogueCharacter.Rocky, "Jackie?")
                .Enter(DialogueCharacter.Jackie, CharacterActions.SetLeft, DialogueSprite.JackieDowncast, fadeDuration: 0.5f)
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

            public static DialogueAsCode AilinStory(Epilogue_10 owner, AudioClip crystalHum, SmokeScreenOverlay smokeScreen)
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
                .Line(DialogueCharacter.Rocky, "After injecting the serums, the patients began to emit a faint humming sound.")
                .Do(new CallbackEvent(() => owner.StartCoroutine(hum2.FadeTo(0.6f, 1.5f))))
                .Line(DialogueCharacter.Rocky, "Apparently, the serums carried way more energy than they should have.")
                .Do(new CallbackEvent(() => owner.StartCoroutine(hum3.FadeTo(0.6f, 1.5f))))
                .Line(DialogueCharacter.Rocky, "Kade says it caused microscopic crystals in each infected scout to shatter, which set off a chain reaction.")
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
                        owner.StartCoroutine(hum1.FadeTo(0f, 0.2f));
                        owner.StartCoroutine(hum3.FadeTo(0f, 0.2f));
                        owner.StartCoroutine(hum2.FadeTo(0f, 0.2f));
                        yield return new WaitForSeconds(0.5f);
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
                .Line(DialogueCharacter.Rocky, "What, but what if—")
                .Line(DialogueCharacter.Rocky, "No, you know what. I'll put it to a vote.")
                .Narrate("<i>Rocky stands up and addresses the rest of the camp. Explaining the situation.</i>")
                .Line(DialogueCharacter.Rocky, "All in favour of contacting the NITES, say aye.");

            public static DialogueAsCode NitesArrival => new DialogueAsCode()
                .Narrate("<i>Soon, the atmosphere lightens. Flickers of recognition turn to smiles in the cave.</i>")
                .Line(DialogueCharacter.Ives, "So y’all need muscle eh? C’mere, I’ve got some for you.", DialogueSprite.IvesLaugh)
                .Line(DialogueCharacter.Rocky, "Ives!? You’re here too.")
                .Line(DialogueCharacter.Ives, "Sure am, what’s up? The little bird didn’t say I’d be coming?", DialogueSprite.IvesSmile)
                .Line(DialogueCharacter.Rocky, "Can't say she did.")
                .ResolveActor(DialogueCharacter.Rocky, out ActorProfile rocky)
                .Narrate("<i>Rocky, in a headlock, cranes his neck to give Jackie a curious look.</i>", events: new SetSpeaker { actor = rocky })
                .Enter(DialogueCharacter.Kade, CharacterActions.SetRight, DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Kade, "Ives. You look terrible.", DialogueSprite.KadeSerious)
                .Move(DialogueCharacter.Rocky, CharacterActions.SetOffscreenLeft)
                .Move(DialogueCharacter.Ives, CharacterActions.SetLeft)
                .Move(DialogueCharacter.Ives, CharacterActions.FaceRight)
                .Line(DialogueCharacter.Ives, "Hah, it’s nice to see you too, Kade.", DialogueSprite.IvesSmile)
                .Line(DialogueCharacter.Kade, "Skin is graying. Pupils dilated. Tremors. You’re on high-grade amplitude suppressants.", DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Ives, "Perceptive as always.", DialogueSprite.IvesSmile)
                .Line(DialogueCharacter.Ives, "Got tangled up in a little Waste incident.", DialogueSprite.IvesNeutral)
                .Line(DialogueCharacter.Kade, "Then you’re in great danger. Crystal breaks around here could seriously be fatal for you.")
                .Line(DialogueCharacter.Ives, "Yeah, Kiddo told me the same thing. We ran into a couple earlier at a distance.")
                .Line(DialogueCharacter.Kade, "How did you feel?")
                .Line(DialogueCharacter.Ives, "There was a bit of a flare up, but the Doc adjusted my dose and I’ve been fine since.")
                .Line(DialogueCharacter.Kade, "Get the adjustment wrong and it could be worthless or kill you.", DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Ives, "Good thing he’s the expert.", DialogueSprite.IvesSmile)
                .Move(DialogueCharacter.Ives, CharacterActions.SetOffscreenLeft)
                .Line(DialogueCharacter.Ives, "Hey, Doc! Front and center.", DialogueSprite.IvesNeutral)
                .Enter(DialogueCharacter.Weise, CharacterActions.SetOffscreenLeft, DialogueSprite.WeiseNeutral, fadeDuration: 0f)
                .Move(DialogueCharacter.Weise, CharacterActions.SetLeft)
                .Line(DialogueCharacter.Weise, "...", DialogueSprite.WeiseNeutral)
                .Narrate("<i>Dr. Weise steps out from the shadows. His face is neutral and hands dug stiffly into the pockets of his lab coat.</i>")
                .Line(DialogueCharacter.Weise, "Now’s hardly the time for acclaims, Commander.", DialogueSprite.WeiseNeutral)
                .Narrate("<i>Kade freezes, her face grows pale.</i>")
                .Line(DialogueCharacter.Kade, "...You. Aleksander.", DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Weise, "Kade.", DialogueSprite.WeiseThinking)
                .Line(DialogueCharacter.Kade, "You’re alive. I... mourned you.", DialogueSprite.KadeSoft)
                .Line(DialogueCharacter.Weise, "I did the same.")
                .Line(DialogueCharacter.Kade, "...How far did you get? Your research to “go up”.", DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Weise, "Nowhere. Over a decade, and nothing.", DialogueSprite.WeiseNeutral)
                .Line(DialogueCharacter.Kade, "Then the slow way down was the only way there was.", DialogueSprite.KadeSoft)
                .Line(DialogueCharacter.Kade, "The ones who didn't make it... there was no faster way after all.")
                .Line(DialogueCharacter.Weise, "...")
                .Narrate("<i>As Kade’s face softens, Jackie comes up beside her.</i>")
                .Move(DialogueCharacter.Kade, CharacterActions.SetMiddle)
                .Enter(DialogueCharacter.Jackie, CharacterActions.SetOffscreenRight, DialogueSprite.NoChange, 0f)
                .Move(DialogueCharacter.Jackie, CharacterActions.SetRight);

            public static DialogueAsCode Deduction(Epilogue_10 owner, ControllableAudioChannel horrorAudio, AnalogueHorrorEffect effect) => new DialogueAsCode()
                .Line(DialogueCharacter.Jackie, "Kade? Something's off to me.", DialogueSprite.JackieContemplative)
                .Line(DialogueCharacter.Kade, "Yes?")
                .Line(DialogueCharacter.Jackie, "You mentioned earlier that Ives' dose adjustment had to be exact?", DialogueSprite.JackieDowncast)
                .Line(DialogueCharacter.Kade, "I did.", DialogueSprite.KadeSoft)
                .Line(DialogueCharacter.Jackie, "How would you determine that?", DialogueSprite.JackieContemplative)
                .Line(DialogueCharacter.Kade, "Well first I’d need to know how much Amplitude to adjust for. It would be tough on the spot, but depending on the distance...")
                .Move(DialogueCharacter.Kade, CharacterActions.FaceLeft)
                .Line(DialogueCharacter.Kade, "Ives, how far were you from the crystals when the break occurred.")
                .Move(DialogueCharacter.Ives, CharacterActions.SetLeftEdge)
                .Line(DialogueCharacter.Ives, "The fight was up front, so I parked myself in the center. Some twenty meters back maybe?", DialogueSprite.IvesNeutral)
                .Line(DialogueCharacter.Kade, "Twenty meters? That would be too far for any contact transfer.")
                .Line(DialogueCharacter.Kade, "Crystal to body resonance would be quite weak at that range as well. It wouldn’t cause a Waste-based flare up that quickly, at least.", DialogueSprite.KadeSerious)
                .Line(DialogueCharacter.Kade, "It would have to be something obvious that I’m missing. Aleksander, what was it?")
                .Line(DialogueCharacter.Weise, "...", DialogueSprite.WeiseThinking)
                .Line(DialogueCharacter.Weise, "The suppressant is a blend. One of its components is resonant to the Crystals.")
                .Line(DialogueCharacter.Kade, "What component?")
                .Line(DialogueCharacter.Weise, "The Waste binding substrate.")
                .Narrate("<i>Jackie’s hand reflexively digs into her pocket. Kade furrows her brow.</i>")
                .Line(DialogueCharacter.Kade, "Resonant. So you’re playing roulette on her life against every Crystal out here?")
                .Line(DialogueCharacter.Weise, "Commander’s orders.")
                .Line(DialogueCharacter.Ives, "Yeah... It’s true. The Doc’s been doing his best to monitor and adjust my dose since.")
                .Narrate("<i>Jackie pulls out her vial, and inspects it. It’s clear.</i>", picture: owner.clearInjection)
                .Line(DialogueCharacter.Jackie, "...Ives, could I see one of your Serums for a sec?")
                .Line(DialogueCharacter.Ives, "Serum? Sure.")
                .Narrate("<i>Ives hands hers over and Jackie holds it next to her own against the light.</i>")
                .Line(DialogueCharacter.Jackie, "It’s a little pink.", DialogueSprite.JackieSurprisedOpen, picture: owner.purpleInjection)
                .Line(DialogueCharacter.Ives, "...Seriously? After all that’s happened, I made sure it wasn’t when we left.", DialogueSprite.IvesQuestioning)
                .Narrate("<i>Jackie turns to the NITES besides Ives.</i>")
                .Line(DialogueCharacter.Jackie, "And yours. Anyone. Could you hold yours up as well?", DialogueSprite.JackieAstonished)
                .Narrate("<i>Over a dozen vials come up. Some are more pink than others.</i>", picture: owner.purpleInjection)
                .Line(DialogueCharacter.Ives, "The ones with the pink vials. They’re the vanguard that dealt with the threat when the crystals broke.")
                .Line(DialogueCharacter.Kade, "A pink-shift. That’s an amplitude influx. For that to happen....")
                .Line(DialogueCharacter.Kade, "The Serums. They’re your resonating component, aren’t they Aleksander?")
                .Line(DialogueCharacter.Weise, "...")
                .Line(DialogueCharacter.Kade, "If someone with a Waste wound was to use that...")
                .Narrate("<i>Kade catches Jay with a wince. Jay’s eyes widen.</i>")
                .Line(DialogueCharacter.Jay, "...My team took several doses that day before turning to Crystal.")
                .Line(DialogueCharacter.Kade, "Then it’s not just Ives in danger, it’s everyone.")
                .Narrate("<i>Murmurs begin rippling through the crowd.</i>")
                .Line(DialogueCharacter.Jackie, "You’ve been checking up on Ives. So you’ve known the danger for quite some time now.", DialogueSprite.JackieFocused)
                .Line(DialogueCharacter.Jackie, "Did you ever plan to tell us?")
                .Line(DialogueCharacter.Weise, "...")
                .Enter(DialogueCharacter.Rocky, CharacterActions.SetOffscreenRight)
                .Move(DialogueCharacter.Rocky, CharacterActions.SetRightEdge)
                .Line(DialogueCharacter.Rocky, "...Nothing? Not a word before we walk into a Crystal stockpile?", DialogueSprite.RockySerious)
                .InterruptedLine(DialogueCharacter.Rocky, "<i>That night</i> happened because of something similar. Sealed inspected Serums that were off... Could it be that–", duration: 1.5f)
                .Do(new CallbackEvent(() => effect.Burst(0.3f)))
                .Line(DialogueCharacter.Weise, "Circumstantial! All of it.")
                .Narrate("<i>Weise’s voice echoes off the cave walls as the murmurs are silenced, all eyes fall onto him.</i>")
                .Move(DialogueCharacter.Ives, CharacterActions.SetLeft)
                .Move(DialogueCharacter.Weise, CharacterActions.SetRight)
                .Move(DialogueCharacter.Weise, CharacterActions.FaceLeft)
                .Move(DialogueCharacter.Kade, CharacterActions.SetOffscreenRight, duration: 2f)
                .Move(DialogueCharacter.Jackie, CharacterActions.SetOffscreenRight, duration: 1.5f)
                .Move(DialogueCharacter.Rocky, CharacterActions.SetOffscreenRight, duration: 1f)
                .Move(DialogueCharacter.Kade, CharacterActions.FaceLeft)
                .Narrate("<i>Ives steps forward.</i>")
                .Line(DialogueCharacter.Ives, "I’ve heard enough.", DialogueSprite.IvesNeutral)
                .Line(DialogueCharacter.Ives, "First part’s on me. I had the condition, so I should’ve checked how coming out here could’ve affected y'all.")
                .Line(DialogueCharacter.Ives, "But Doc. I vouched for you. I trusted you to be the one to tell me.")
                .Line(DialogueCharacter.Ives, "And I promised Cam that if I ever found a person who knew about all this...")
                .Line(DialogueCharacter.Ives, "I’d like to have some words with them.")
                .Move(DialogueCharacter.Ives, CharacterActions.SetMiddle, duration: 0.8f)
                .Move(DialogueCharacter.Weise, CharacterActions.SetRightEdge)
                .Move(DialogueCharacter.Weise, CharacterActions.FaceLeft)
                .Line(DialogueCharacter.Ives, "So. Let's chat.", DialogueSprite.IvesQuestioning)
                .Line(DialogueCharacter.Ives, "How much do you know?")
                .Move(DialogueCharacter.Ives, CharacterActions.SetRight)
                .Narrate("<i>Ives steps forward, towering over the doctor. Weise backs up until he hits the cave wall.</i>")
                .Exit(DialogueCharacter.Ives, DialogueCharacter.Weise)
                .Line(DialogueCharacter.Weise, "...How much I know? Well... let me show you!", DialogueSprite.WeiseThinking)
                .Do(new CallbackEvent(() => owner.StartCoroutine(owner.InjectionSequence(horrorAudio))))
                .Narrate("<i>Dr. Weise rips a syringe from his coat, jamming it into Ives' shoulder.</i>")
                .InterruptedLine(DialogueCharacter.Ives, "ARGH! You—!", DialogueSprite.IvesNeutral)
                .Narrate("<i>Ives falls to the floor, her muscles rippling.</i>")
                .InterruptedLine(DialogueCharacter.Kade, "Jackie, get back! She's—", DialogueSprite.KadeSerious)
                .Do(new CallbackEvent(() => owner.StartCoroutine(owner.PurpleFlash())))
                .Narrate("<i>The convulsing stops.</i>")
                .Narrate("<i>Ives stands and looks to the cave exit. The whites in her eyes are gone. Replaced by a glowing violet.</i>")
                .Narrate("<i>The tone of Jackie’s tracker changes to static, and Ives begins to walk unhurried toward the cave exit.</i>")
                .Narrate("<i>Weise scrambles up with a grin and presses close behind Ives. Keeping one hand in his pocket.</i>")
                .Line(DialogueCharacter.Weise, "Anyone else want a demonstration? No? Then I suggest you let us pass.", DialogueSprite.WeiseNeutral)
                .Line(DialogueCharacter.Jackie, "Ives! What do we do!?")
                .Line(DialogueCharacter.Rocky, "Tsk, he’s gonna use Ives to make his way out of here.", DialogueSprite.RockySerious)
                .Line(DialogueCharacter.Rocky, "If we cut down the Frog in control, he’ll have nothing.", DialogueSprite.RockySerious)
                .Line(DialogueCharacter.Rocky, "Let's stick with the plan. Everyone battle formations! Let’s get our people back!")
                .Exit(DialogueCharacter.Jackie, DialogueCharacter.Rocky, DialogueCharacter.Kade, DialogueCharacter.Weise, DialogueCharacter.Ives);

            public static DialogueAsCode PreBattleText() => new DialogueAsCode()
                .Line(DialogueCharacter.Jackie, "Rocky, I see the princess frog ahead. Where’s the assault team?")
                .Line(DialogueCharacter.Rocky, "We’re being held up by an influx of creatures. Jay, how’s your gathering going, can you spare some men?")
                .Line(DialogueCharacter.Jay, " Can’t disengage. We’re being held back by the scouts.")
                .Line(DialogueCharacter.Rocky, "Then it’s just you Jackie. I...")
                .Line(DialogueCharacter.Rocky, "I trust you to engage. Keep yourself safe.")
                .Line(DialogueCharacter.Jackie, "Got it. I’ll dip if it's too much to handle.")
                .Line(DialogueCharacter.Rocky, "Good luck.");
        }
    }
}
