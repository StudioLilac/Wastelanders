using Cinemachine;
using DialogueScripts;
using Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Dialogue.Epilogue.Epilogue_10;

namespace Dialogue.Epilogue
{
    public class Epilogue_10_Cutscene : MonoBehaviour
    {
        public CinemachineVirtualCamera baseCamera;
        public CinemachineVirtualCamera dynamicCamera;
        public Transform CombatCameraAnchor;
        public Transform StartingCameraAnchor;
        public Transform SpyingCameraAnchor;
        public Transform SlimeDestination;
        public Transform FrogDestination;
        public Transform BeetleDestination;
        public Transform BeetleDestination2;
        public Transform BeetleDestination3;
        public Transform JackieBehindRock;
        public Transform JackieBehindCrystal;
        public Transform JackieBehindCrystal2;
        public Transform JackieBehindCrystal3;
        public Transform JackieConfrontation;
        public Transform JackieSpyingOnPrincessFrog;
        public Transform JackieAttackPrincessFrog;
        public Transform IvesInterceptPosition;
        public Jackie jackie;
        public EnemyIves ives;
        public SlimeStack slime;
        public SlimeStack slime2;
        public WasteFrog frog;
        public WasteFrog frog2;
        public Beetle beetle1;
        public Beetle beetle2;
        public Beetle beetle3;
        public List<EnemyClass> crystals;
        public PrincessFrog jackiePrincessFrog;
        public PrincessFrog truePrincessFrog;
        public AnalogueHorrorEffect analogueHorrorEffect;
        public ScalingLerpHandler overlayRocks;
        public UIFadeHandler overlayRocksUI;
        public GameObject fieldCrystal;

        public void JumpToCombat()
        {
            fieldCrystal.SetActive(false);
            overlayRocks.gameObject.SetActive(false);
            baseCamera.transform.position = CombatCameraAnchor.position;
            var enemies = new List<EnemyClass>() { slime, slime2, frog, frog2, beetle1, beetle2, beetle3, jackiePrincessFrog };
            CombatManager.Instance.SetEnemiesPassive(enemies);
            enemies.ForEach(e => e.gameObject.SetActive(false));
            jackie.transform.position = JackieSpyingOnPrincessFrog.transform.position;
            StartCoroutine(jackie.ResetPosition());
            StartCoroutine(truePrincessFrog.ResetPosition());
            StartCoroutine(ives.ResetPosition());
        }

        public IEnumerator Play(Epilogue_10 owner)
        {
            overlayRocks.gameObject.SetActive(true);
            baseCamera.transform.position = StartingCameraAnchor.position;
            CombatManager.Instance.SetEnemiesPassive(new List<EnemyClass>() { slime, slime2, frog, frog2, beetle1, beetle2, beetle3, jackiePrincessFrog}.Concat(crystals).ToList());
            beetle1.FaceLeft(); beetle2.FaceLeft(); beetle3.FaceLeft(); jackie.Emphasize();

            {
                StartCoroutine(slime.MoveToPosition(SlimeDestination.position, 0f, 12f));
                yield return jackie.MoveToPosition(JackieBehindRock.position, 0f, 1.5f);
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(frog.MoveToPosition(FrogDestination.position, 0f, 12f));
                yield return new DialogueAsCode()
                    .Do(new VerticalLayoutChange { Layout = Layout.Upper })
                    .InterruptedLine(DialogueCharacter.Jackie, "Crap, there's creatures up ahead. I need to hide.", duration: 2f).Play();
                dynamicCamera.Priority = 1; baseCamera.Priority = 0;
                StartCoroutine(overlayRocks.FadeInLightScreen(2.0f));
                yield return new WaitForSeconds(0.5f);
                yield return jackie.MoveToPosition(JackieBehindCrystal.position, 0f, 1f);
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(frog2.MoveToPosition(FrogDestination.position, 0f, 8f));
                StartCoroutine(slime2.MoveToPosition(SlimeDestination.position, 0f, 10f));
                yield return new DialogueAsCode().InterruptedLine(DialogueCharacter.Jackie, "Surroundings check. No shadows showing right?", duration: 2f).Play();
                yield return jackie.MoveToPosition(JackieBehindCrystal2.position, 0f, 0.5f);
                yield return new WaitForSeconds(3f);
            }
            {
                yield return new DialogueAsCode().InterruptedLine(DialogueCharacter.Jackie, "Got past them.", duration: 2f).Play();
                yield return new WaitForSeconds(0.5f);
                yield return jackie.MoveToPosition(JackieBehindCrystal3.position, 0f, 0.5f);

                yield return new DialogueAsCode()
                    .Line(DialogueCharacter.Jackie, "Why arn't these beetles moving?")
                    .Line(DialogueCharacter.Jackie, "Are they guarding the princess frog?")
                    .Line(DialogueCharacter.Jackie, "...")
                    .Line(DialogueCharacter.Jackie, "If so... let me try something.")
                    .Play();
                yield return new WaitForSeconds(0.5f);
                jackiePrincessFrog.gameObject.transform.position = jackie.gameObject.transform.position;
                jackiePrincessFrog.gameObject.SetActive(true);
                jackiePrincessFrog.Emphasize();
                jackie.gameObject.SetActive(false);
                SoundID.VN_finger_snap.Play();

                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(jackiePrincessFrog.MoveToPosition(jackiePrincessFrog.transform.position + new Vector3(2f, 0f, 0f), 0f, 0.8f));
                yield return StartCoroutine(jackiePrincessFrog.MoveToPosition(JackieConfrontation.position, 0f, 1.3f));
                yield return new WaitForSeconds(0.5f);
            }

            {
                SoundID.VN_purple_pulse.Play();
                StartCoroutine(owner.PurpleFlash());
                var relieved = StartCoroutine(new DialogueAsCode().Line(DialogueCharacter.Jackie, "You are relieved!").Play());
                yield return new WaitForSeconds(1.5f);
                StartCoroutine(beetle1.MoveToPosition(BeetleDestination.position, 0f, 6f));
                yield return new WaitForSeconds(0.3f);
                StartCoroutine(beetle3.MoveToPosition(BeetleDestination3.position, 0f, 5f));
                yield return new WaitForSeconds(0.2f);
                StartCoroutine(beetle2.MoveToPosition(BeetleDestination2.position, 0f, 6f));
                yield return new WaitForSeconds(1.5f);
                yield return relieved;

                yield return new DialogueAsCode().Line(DialogueCharacter.Jackie, "Glad I had practice.").Play();
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(jackiePrincessFrog.MoveToPosition(JackieSpyingOnPrincessFrog.position, 0f, 1.5f));
                baseCamera.transform.position = SpyingCameraAnchor.position;
                baseCamera.Priority = 1; dynamicCamera.Priority = 0;
                yield return new WaitForSeconds(2f);
                jackie.gameObject.transform.position = jackiePrincessFrog.gameObject.transform.position;
                jackiePrincessFrog.gameObject.SetActive(false); jackie.gameObject.SetActive(true);
                SoundID.VN_finger_snap.Play();
                yield return new WaitForSeconds(0.5f);
            }
            {
                yield return DialogueBoxV2.Instance.Play(Epilogue10Dialogue.PreBattleText());
                yield return new WaitForSeconds(0.5f);
                baseCamera.Priority = 0; dynamicCamera.Priority = 1;
                yield return StartCoroutine(jackie.MoveToPosition(jackie.transform.position + new Vector3(2f, 0f, 0f), 0f, 0.8f));
                StartCoroutine(jackie.MoveToPosition(JackieAttackPrincessFrog.position, 0f, 2f));
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(ives.MoveToPosition(IvesInterceptPosition.position, 0f, 1f));
                ives.AttackAnimation(FistCards.FIST_ANIMATION_NAME);
                SoundID.CB_fist_hit.Play();
                SoundID.VN_radio_static.Play();
                analogueHorrorEffect.Burst(0.5f, 0.5f);
                fieldCrystal.SetActive(false);
                truePrincessFrog.FaceLeft();
                baseCamera.Priority = 1; dynamicCamera.Priority = 0; baseCamera.transform.position = CombatCameraAnchor.position; baseCamera.m_Lens.OrthographicSize = 4;
                yield return StartCoroutine(jackie.StaggerEntities(ives, jackie, 0.4f));
                jackie.SetStaggered(true);
                yield return new WaitForSeconds(1f);
                jackiePrincessFrog.gameObject.transform.position = jackie.gameObject.transform.position;
                jackiePrincessFrog.gameObject.SetActive(true);
                jackiePrincessFrog.Emphasize();
                jackie.gameObject.SetActive(false);
                SoundID.VN_finger_snap.Play();
                yield return new WaitForSeconds(0.5f);
                yield return new DialogueAsCode()
                    .Do(new VerticalLayoutChange { Layout = Layout.Lower })
                    .Line(DialogueCharacter.Jackie, "Ives, can you hear me! Snap out of it!")
                    .Do(new CallbackEvent(() => analogueHorrorEffect.Burst(0.4f)))
                    .Line(DialogueCharacter.Ives, "XXXXX, X'XX XXXXXXX XXX.", sfx: SoundID.VN_radio_static)
                    .Line(DialogueCharacter.Jackie, "Crap. Looks like there's no other way out of this.")
                    .Play();
                jackie.gameObject.transform.position = jackiePrincessFrog.gameObject.transform.position;
                jackiePrincessFrog.gameObject.SetActive(false); jackie.gameObject.SetActive(true);
                SoundID.VN_finger_snap.Play(); jackie.SetStaggered(false);
                yield return new WaitForSeconds(1f);
            }

            StartCoroutine(overlayRocksUI.FadeInLightScreen(1.0f));
            CombatManager.Instance.SetEnemiesHostile(crystals.ToList());
        }
    }
}