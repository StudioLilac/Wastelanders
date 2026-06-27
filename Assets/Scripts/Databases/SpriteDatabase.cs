using System;
using System.Collections.Generic;
using UnityEngine;

/// Keyed repository of character expression sprites for dialogue-as-code.
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

/// Stable keys for every dialogue character expression.
public enum DialogueSprite
{
    NoChange, 
    None,

    // Cam
    CamNeutral,             // nu
    CamPout,                // po
    CamSmile,               // sm
    CamTalk,                // ta
    CamConfused,            // co
    CamProud,               // pr

    // Jackie
    JackieMouthOpen,        // om
    JackieSerious,          // se
    JackieSmile,            // sm
    JackieSurprisedClosed,  // su-cl
    JackieSurprisedOpen,    // su-op
    JackieTired,            // ti-cl

    // Ives
    IvesQuestioning,        // qu
    IvesLaugh,              // la
    IvesSmile,              // sm
    IvesNeutral,            // nu

    // Dr. Weise
    WeiseNeutral,           // nu
    WeiseSmile,             // sm
    WeiseThinking,          // th
}
