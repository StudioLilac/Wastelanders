using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteDatabase", menuName = "Dialogue/SpriteDatabase")]
public class SpriteDatabase : ScriptableObject
{
    [SerializeField] private List<SpriteConfig> sprites;
    private Dictionary<DialogueSprite, Sprite> lookup;

    public Sprite Get(DialogueSprite id)
    {
        if (lookup == null)
        {
            lookup = new();
            foreach (SpriteConfig config in sprites)
            {
                lookup[config.id] = config.sprite;
            }
        }

        if (!lookup.TryGetValue(id, out Sprite sprite) || sprite == null)
        {
            Debug.LogWarning($"SpriteDatabase is missing a sprite for {id}. Assign it in the SpriteDatabase asset.");
        }
        return sprite;
    }

    [Serializable]
    public struct SpriteConfig
    {
        public DialogueSprite id;
        public Sprite sprite;
    }
}

// NOTE: These values are serialized by integer into SpriteDatabase.asset.
// Never reorder or renumber existing entries — it silently remaps the asset's
// sprites. Add new entries with a fresh, unused explicit value (see below).
public enum DialogueSprite
{
    NoChange = 0,
    None = 1,

    CamNeutral = 2,             // nu
    CamPout = 3,                // po
    CamSmile = 4,               // sm
    CamTalk = 5,                // ta
    CamConfused = 6,            // co
    CamProud = 7,               // pr
    CamSerious = 71,            // se
    CamAngry = 72,
    CamCriticize = 73,
    CamRelieved = 74,

    JackieRetort = 8,             // om
    JackieSerious = 9,          // se
    JackieSmile = 10,           // sm
    JackieSurprisedClosed = 11, // su-cl
    JackieSurprisedOpen = 12,   // su-op
    JackieTired = 13,           // ti-cl
    JackieAstonished = 131,
    JackieNeutralSoft = 132,
    JackieFocused = 133,
    JackieWry = 134,
    JackieStern = 135,

    IvesQuestioning = 14,       // qu
    IvesLaugh = 15,             // la
    IvesSmile = 16,             // sm
    IvesNeutral = 17,           // nu

    WeiseNeutral = 18,          // nu
    WeiseSmile = 19,            // sm
    WeiseThinking = 20,         // th

    KadeSerious = 30,
    KadeSoft = 31,
    JaySerious = 32,
    JaySorry = 33,
    RockySerious = 34,
    RockySmile = 35,
}
