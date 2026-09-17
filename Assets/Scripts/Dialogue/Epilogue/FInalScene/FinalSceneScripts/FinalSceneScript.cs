namespace Cinematics
{
    public static class FinalSceneScript
    {
        private const DialogueCharacter Narration = DialogueCharacter.Narration;
        private const DialogueCharacter Jackie = DialogueCharacter.Jackie;
        private const DialogueCharacter Ives = DialogueCharacter.Ives;

        public static CinematicBeat[] Build()
        {
            var s = new CinematicScript();

            s.SetRate(Narration, 35f)
             .SetRate(Jackie, 42f)
             .SetRate(Ives, 20f);

            s.Line(DialogueCharacter.Event, string.Empty, cue: FinalSceneDirector.INTRO_START, hold: 5950f)
             .Narrate("The Princess Frog lets out a final wail as its form dissolves into the whirling blizzard.", hold: 2000f)
             .Narrate("In the center of the crater, Ives sways before crashing into the snow.{!ivescrashes}", hold: 2000f)
             .Line(Jackie, "IVES!", lead: 400f, hold: 1000f)
             .Narrate("Jackie presses her fingers to Ives' neck, checking for a pulse.", lead: 200f, hold: 2000f)
             .Line(Ives, "...{800}Jackie?", lead: 600f, rate: 16f, hold: 1100f)

             .Line(Jackie, "I'm here!{350} Just hold on. We have to get you back to Kade.", lead: 0f)

             .Line(Ives, "No can do, kid. My legs... they don't move.", rate: 18f, hold: 1200f)

            // ---- The serum -------------------------------------------------------
             .Narrate("Jackie scrambles through her pockets. Takes out her Serum.{800} It's pink.",
                    hold: 1300f)

             .Narrate("Jackie sets the serum in the snow.", lead: 400f)
             .Narrate("And wraps her arms tightly around Ives.", lead: 200f)

             .Line(Jackie, "I-I{350} can still carry you back.", rate: 34f)

             .Narrate("Jackie gets under Ives' shoulder and begins to lift.")

             .Line(Ives, "Ah, kid... Just, give me a sec.", rate: 17f)

             .Line(Jackie, "Ives, you don't have the time to–", rate: 40f)

             .Line(Jackie, "...", lead: 700f, reveal: RevealMode.Fade, hold: 1400f) //52/55

             .Line(Jackie, "Alright.{500} Just... please tell me you'll be alright.", rate: 30f)

            // ---- The promise -----------------------------------------------------
             .Line(Ives, "Sorry, kid.", rate: 16f, hold: 1100f)
             .Line(Ives, "I can't be making any more promises I can't keep.", hold: 1300f)
             .Line(Ives, "I promised before we'd find out what happened to your Ma,{400} together.", rate: 20f, hold: 1200f)
             .Line(Ives, "Look at me now,{400} not much use to you like this.", rate: 20f, hold: 1100f)

             .Narrate("A burning sensation rises in Jackie's chest, threatening to burst from her throat.", hold: 1300f)

             .Line(Jackie, "Ives, I'm so sorry, it's all my—", rate: 40f)

             .Line(Jackie, "...", lead: 600f, reveal: RevealMode.Cut, exit: new ExitMode.Cut(0f), hold: 1200f)

            // ---- The turn --------------------------------------------------------
            // Jackie stops apologising. Everything after this is her choosing what
            // to say instead, so the rate comes down and stays down.
             .SetRate(Jackie, 32f)

             .Line(Jackie, "\"Fight its shape, not its strength.{0}\"", lead: 200f, hold: 1300f)
             .Line(Jackie, "You taught me that. It saved my life.")

             .Narrate("Jackie pulls up her pant leg to reveal the dressing.")
             .Line(Jackie, "We've been together the whole time.")
             .Narrate("Jackie eases Ives down with her arm.", lead: 200f)

             .Line(Jackie, "Every lesson, every exam, every terrible day.", rate: 28f, hold: 1300f)
             .Line(Jackie, "You were there, and you didn't need anything from me in return.", rate: 28f, hold: 1400f)
             .Line(Ives, "Kid...", lead: 200f, rate: 14f, hold: 1200f)

            // ---- The moon --------------------------------------------------------
             .Narrate("Jackie tightens her grasp on Ives' shirt.", cue: FinalSceneDirector.TIGHTEN_SHIRT, hold: 1500f, lead: 200f)

             .Line(Jackie, "I... found out what happened to Ma.", lead: 600f, rate: 25f, hold: 1400f)
             .Line(Ives, "...You did?", lead: 500f, rate: 15f, hold: 1000f)
             .Line(Jackie, "Yeah, she’s still hiding. But I think I see her shadow now.", rate: 25f, hold: 1200f)
             .Line(Ives, $"...You've been playing hide and seek with her for{{!{FinalSceneDirector.TEARS_BEGIN}}} quite some time now. Haven't you.", lead: 400f, rate: 18f, hold: 1400f)
             .Narrate("Jackie grasps harder. Tears begin to stream down her face. She nods.", rate: 30f, lead: 300f, hold: 1500f)
             .Line(Ives, "...Shit. C'mere, these arms still work.", rate: 16f, hold: 1200f)

             .Narrate("Ives rests her arms on Jackie's shoulders,{400} and brings her close.", rate: 30f, lead: 200f)

             .Line(Ives, "You know. I{300} expected you to haul me off earlier.", lead: 200f, cue: FinalSceneDirector.MOON_ZOOM, rate: 14f, hold: 1200f)
             .Line(Jackie, "Y-you{300} did?", rate: 24f, hold: 1000f)
             .Line(Ives, "Yeah. Glad I was wrong.", rate: 15f, hold: 1300f)
             .Line(Ives, "Wouldn't be sitting here hearing about your Ma otherwise.", rate: 15f, hold: 1600f)
             .Line(Jackie, "...", lead: 700f, reveal: RevealMode.Fade, hold: 1500f)

            // ---- Getting up ------------------------------------------------------ 160/163
             .Narrate("Jackie's breathing steadies. And she pushes herself up to face Ives.", rate: 25f, lead: 400f, hold: 1500f, cue: FinalSceneDirector.TEARS_END)

             .Line(Jackie, "It's about time we get going. Don't you think?", rate: 26f, hold: 1300f)
             .Line(Ives, "Yeah,{400} I suppose so.", rate: 14f, hold: 1000f)

             .Narrate("Jackie puts her right shoulder under Ives' shoulder, propping her up.", lead: 200f, hold: 1300f)

             .Line(Ives, "...You're the one helping me off the floor now.", rate: 15f, hold: 1300f)
             .Line(Jackie, "...Why not,{800} you my babysitter or something?", lead: 400f, rate: 26f, hold: 1500f)

             .Narrate("Jackie puts her fist out.{400} Ives looks at it briefly.", lead: 400f, hold: 1500f)
             .Line(Ives, "...Hah,{400} no.{800} Never was.", lead: 700f, rate: 14f, hold: 1100f)
             .Narrate("And Ives meets it.", lead: 400f, hold: 1300f)

            // ---- The last exchange -----------------------------------------------
             .Line(Ives, "Will you... take me with you one more time?", rate: 14f)
             .Line(Jackie, "Yeah.{500} Wherever I am. Wherever I'll go.", lead: 400f, rate: 24f)
             .Line(Ives, "Good...{1000} That's good...", lead: 200f, rate: 12f, hold: 1000f)

             .Narrate("Ives lets out a long breath, before she closes her eyes and leans on Jackie's back.",
                    lead: 1300f, hold: 1800f)

             .Narrate("Feeling Ives' breath quiet on her back.{500} Jackie rolls to the side and heaves upward.",
                    lead: 800f, hold: 1600f)

             .Narrate("Standing against the wind and snow,{700} Jackie turns towards the arm across her shoulder.",
                    lead: 600f, hold: 1500f)

             .Line(Jackie, "Any time.", lead: 700f, rate: 18f, hold: 2000f)

             .Narrate("And begins the long trek back towards the firelight.",
                    lead: 1200f, rate: 22f, hold: 2500f, cue: "end");

            return s.Build();
        }
    }
}
