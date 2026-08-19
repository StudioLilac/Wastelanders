# Dialogue-as-Code: porting a script

How to take a writers' script (Jackie/Cam/Ives lines + stage directions) and turn it
into a code-built dialogue scene that plays through `DialogueBoxV2`.

## Read these first

| File | Why |
|---|---|
| `Assets/Scripts/Dialogue/DialogueAsCode.cs` | The builder API (`Line`, `Narrate`, `Enter`, `Exit`, `Do`) **and** the `DialogueCharacter` speaker enum. Each call appends a `DialogueEntry`; the builder implicitly converts to `DialogueEntry[]`. |
| `Assets/Scripts/Dialogue/Epilogue/Epilogue3Driving.cs` | The canonical worked example — copy its shape for a new scene. |
| `Assets/Scripts/Databases/SpriteDatabase.cs` | The `DialogueSprite` enum = every character expression. Short writer codes (Cam `nu`, Jackie `om`, Ives `qu`) map to the enum names (comments show the mapping). |
| `Assets/Scripts/Databases/SoundEffectsDatabase.cs` | The `SoundID` enum for `"Audio:"` cues. Add a new entry here if a sound is missing, then assign the clip in the Sound Effect Database asset. |
| `Assets/Scripts/DialogueScripts/DialogueEvents.cs` | Raw events (`CharacterActions` positions, `ExpressionChange`) for `Enter`/`Exit`/`Do`. |

## The whole API is enum-based — no asset wiring

Speakers are `DialogueCharacter`, expressions are `DialogueSprite`. You never touch an
`ActorProfile`, a `Sprite`, or a database in a scene file:

- **Actors** are summoned in exactly one place — `DialogueAsCode` calls
  `new GetActorDatabase().Query()` (answered by the live `DialogueBoxV2`) and maps the
  enum to a profile. It throws if no `DialogueBoxV2` is present.
- **Sprites** are resolved at the other edge — a `DialogueSprite` rides along on an
  `ExpressionChange` event and `StageDirector` looks it up in its `SpriteDatabase`
  (wired once on `Assets/Prefabs/LevelPrefabs/StageDirector.prefab`).

So a scene is pure data and needs zero `[SerializeField]`s.

## The pattern

0. Read the neighbouring file "DialogueToTranslate.md" to view the dialogue scene to translate 
if the user has not provided a scene.

1. **Make a scene class** next to the controller that owns it (e.g. under
   `Assets/Scripts/Dialogue/Epilogue/`). No constructor args, no fields, generally a static class.
    Expose the dialogue as one or more `DialogueAsCode` properties.
    Possibly consider using static on the enum namespaces. Although not standard, it should make it easier
    to type the various speaker names and actions. That way even writers can edit this code. 

2. **Translate each line** with `.Line(DialogueCharacter.X, "text", expr?, sfx?)`.
   - Speaker → `DialogueCharacter.X`. The expression is **optional**: only pass if a change 
   in expression is necessary. Occasionally the script will have a period of unkeyed sprites,
   if you feel a sprite would fit somewhere that is unkeyed, feel free to add it and add a comment
   that this was a additional choice to the script that you made. 
   - `Narration:` / `Audio:` / stage directions → `.Narrate("<i>...</i>", SoundID.X)`.
     Wrap narration in `<i>…</i>` to match house style.
  - If you feel that a line is too long (generally should keep things less than 2 sentences),
  then feel free to add a line break and add an annotation where it feels natural.  

3. **Stage the cast** at the top with
   `.Enter(DialogueCharacter.X, CharacterActions.SetLeft, DialogueSprite.Y)` (Left /
   Middle / Right), and `.Exit(DialogueCharacter.X, …)` at the end. These are silent
   event-only beats. Only enter when the character first speaks their line. For scenes
   where the character is expected to already have been in the scene, use a fade in.
   For scenes where the character is moving into frame, use a move in without fade.

4. **Split anything that must be `yield return`-ed.** A single `DialogueAsCode` builder
   is one uninterrupted dialogue batch. A screen fade-to-black, a timed wait, a
   background swap, or starting music must happen **in the calling coroutine between
   `Play()` calls** — you cannot embed them inside one builder. Break the script at each
   such moment into separate properties (`PartA`, `PartB`, …) and sequence them:

   ```csharp
   var driving = new Epilogue3Driving();
   yield return DialogueBoxV2.Instance.Play(driving.PartA);
   yield return black.FadeInDarkScreen(2f);            // the "[Fade to black, 2s]"
   yield return DialogueBoxV2.Instance.Play(driving.PartB);
   ```

   Rule of thumb: every `[Fade ...]`, `[Wait ...]`, background change, or music cue in
   the script = a split point between builder parts.

## Gotchas

- **Missing expression?** Add a value to `DialogueSprite`, then add the sprite to
  `SpriteDatabase.asset` (a `Get` miss logs a warning and returns null).
- **Missing speaker?** Add a value to `DialogueCharacter`, a field to `ActorDatabase`
  (+ assign the `ActorProfile` asset), and a `case` in `DialogueAsCode.Resolve` — this
  is how `Cam`/`Weise` were added.
- **Missing SFX?** Add a `SoundID` and assign the clip; an unassigned `SoundID` plays
  nothing.
- **`SpriteChange` vs `ExpressionChange`**: legacy `.asset` dialogue serializes a
  resolved `Sprite` via `SpriteChange`; code uses the id-based `ExpressionChange`.
  Both are handled by `StageDirector` — keep `SpriteChange` for back-compat.
- Silent beats (`Enter`/`Exit`/`Do`) are spoken by the Event actor so
  `DialogueBoxV2.SkipEntry` runs their events without showing a box.
