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

    //VN SFX
    VN_page_flip = 500,

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