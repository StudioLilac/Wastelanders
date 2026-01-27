using System.Collections.Generic;
using DialogueScripts;
using UnityEngine;


[System.Serializable]
public class DialogueText
{
#nullable enable
    [SerializeField] private string bodyText = null!;
    [SerializeField] private string speakerName = null!;
    [SerializeField] private Sprite? displayingImage;

    private bool italics;
    private bool bold;
    public bool broadcastAnEvent = false;


    public bool Italics { get { return italics; } set { italics = value; } }
    public bool Bold { get { return bold; } set { bold = value; } }


    public string BodyText {  get { return bodyText; } set {  bodyText = value; } }
    public string SpeakerName { get {  return speakerName; } set {  speakerName = value; } }
    public Sprite? DisplayingImage { get { return displayingImage; } set { displayingImage = value; } }

    public DialogueText(string bodyText, string speakerName, Sprite? givenImage, string? sfx = null)
    {
        this.bodyText = bodyText;
        this.speakerName = speakerName;
        this.displayingImage = givenImage;
    }

    public DialogueEntry Into() => new (content: bodyText, speaker: GetSpeakerProfile(speakerName), picture: displayingImage, events: (broadcastAnEvent) ? new() { new CustomEvent() } : new(), sfxId: default);

    private static ActorProfile? GetSpeakerProfile(string speakerName)
    {
        ActorDatabase? database = new GetActorDatabase().Query();
        if (database == null) return null;

        return speakerName.Trim().ToLower() switch
               {
            "jackie" => database.Jackie,
            "ives" => database.Ives,
            "ailin" => database.Ailin,
            "narration" => database.Narration,
            "broadcast" => database.Broadcast,
            "loudspeaker" => database.Loudspeaker,
            "tutorial" => database.Tutorial,
            "???" => database.Unkown,
            _ => null,
        };
    }
}

