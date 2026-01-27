using DialogueScripts;
using UnityEngine;

[CreateAssetMenu(fileName = "ActorDatabase", menuName = "Dialogue/ActorDatabase")]
public class ActorDatabase : ScriptableObject
{
    public ActorProfile Jackie;
    public ActorProfile Ives;
    public ActorProfile Narration;
    public ActorProfile Broadcast;
    public ActorProfile Loudspeaker;
    public ActorProfile Tutorial;
    public ActorProfile Event;
}
