using FMOD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


/*
 * Class that holds all the sound effects for a scene 
 *  */
[CreateAssetMenu(fileName = "New Sound Effect Database", menuName = "Sound Effect Database")]
public class SoundEffectsDatabase : ScriptableObject
{
    [SerializeField] private List<SoundConfig> soundConfigs;
    private Dictionary<SoundID, AudioClip> soundDictionary;

    public AudioClip GetClipByID(SoundID soundID)
    {
        if (soundDictionary == null)
        {
            soundDictionary = new();
            foreach (SoundConfig config in soundConfigs)
            {
                soundDictionary[config.id] = config.clip;
            }
        }
        return soundDictionary[soundID];
    }
}
public enum SoundID
{
    None = 0,
    
    //Combat SFX 
    CB_axe_cut = 200,
    CB_block = 201,
    CB_clash_tie = 202,
    CB_excavate = 203,
    CB_fist_hit = 204,
    CB_frog_hit = 205,
    CB_gun_hit = 206,
    CB_queen_hit = 207,
    CB_slime_hit = 208,
    CB_staff_hit = 209,
    CB_pincer_hit = 210,
    CB_hatchery_summon = 211,
    CB_roll_dice = 212,
    CB_queen_beetle_spawn = 213,

    //VN SFX
    VN_page_flip = 500,
    VN_footsteps = 501,
    VN_jacket_rustle_1 = 502,
    VN_jacket_rustle_2 = 503,
    VN_finger_snap = 504,
    VN_purple_pulse = 505,
    VN_pulse_2 = 506,
    VN_ives_cough_1 = 507,
    VN_ives_cough_2 = 508,
    VN_door_knock = 509,
    VN_dragon_roar_1 = 510,
    VN_dragon_roar_2 = 511,
    VN_bone_crunch_1 = 512,
    VN_bone_crunch_2 = 513,
    VN_skull_crash = 514,
    VN_dragon_defeat_1 = 515,
    VN_dragon_defeat_2 = 516,
    VN_bone_pop = 517,
    VN_video_call_pickup = 518,
    VN_video_call_hangup = 519,
    VN_radio_static = 520,

    // Epilogue 3 - convoy / tire burst
    VN_tire_burst = 521,
    VN_drill = 523,
    VN_door_shut = 524,
    VN_engine_rev = 525,
    VN_jack_raising = 528,

    // Epilogue 7 - cave creature fight
    VN_ep7_jackie_hit_wall = 530,
    VN_ep7_dragon_hit_wall = 531,
    VN_ep7_dragon_hiss = 532,
    VN_ep7_dragon_death = 533,
    VN_ep7_tracker_slow = 534,
    VN_ep7_tracker_loop = 535,   // ambient loop
    VN_ep7_water_drips = 536,    // ambient loop
    VN_ep7_gravel_drag = 537,
    VN_ep7_pant_rip = 538,
    VN_ep7_creature_growl = 539,
}

public static class SoundIDExtensions
{
    public static void Play(this SoundID soundID)
    {
        AudioManager.Instance.PlaySFX(soundID);
    }
}

[Serializable]
public struct SoundConfig
{
    public SoundID id;
    public AudioClip clip;
}