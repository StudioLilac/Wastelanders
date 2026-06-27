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

1. **Make a scene class** next to the controller that owns it (e.g. under
   `Assets/Scripts/Dialogue/Epilogue/`). No constructor args, no fields. Expose the
   dialogue as one or more `DialogueAsCode` properties.

2. **Translate each line** with `.Line(DialogueCharacter.X, "text", expr?, sfx?)`.
   - Speaker → `DialogueCharacter.X`. The expression is **optional**: only pass a
     `DialogueSprite` when the script gives a parenthetical code (e.g. `Jackie (om)` →
     `DialogueSprite.JackieMouthOpen`). Omit it (defaults to `NoChange`) for un-coded
     lines — the actor keeps whatever sprite they entered with, so don't invent
     expressions just to fill the slot.
   - `Narration:` / `Audio:` / stage directions → `.Narrate("<i>...</i>", SoundID.X)`.
     Wrap narration in `<i>…</i>` to match house style.

3. **Stage the cast** at the top with
   `.Enter(DialogueCharacter.X, CharacterActions.SetLeft, DialogueSprite.Y)` (Left /
   Middle / Right), and `.Exit(DialogueCharacter.X, …)` at the end. These are silent
   event-only beats.

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
