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

            s.Narrate("The Princess Frog lets out a final wail as its form dissolves into the whirling blizzard.", hold: 2000f)
             .Narrate("In the center of the crater, Ives sways before crashing into the snow.{!ivescrashes}", hold: 3000f)
             .Line(Jackie, "IVES!", reveal: RevealMode.Cut, hold: 1000f)
             .Narrate("Jackie presses her fingers to Ives' neck, checking for a pulse.", lead: 200f, hold: 2000f)
             .Line(Ives, "...{800}Jackie?", lead: 600f, rate: 16f, hold: 1100f)

             .Line(Jackie, "I'm here!{350} Just hold on. We have to get you back to Kade.", lead: 0f)

             .Line(Ives, "No can do, kid. My legs... they don't move.", rate: 18f, hold: 1200f)

            // ---- The serum -------------------------------------------------------
             .Narrate("Jackie scrambles through her pockets. Takes out her Serum.{800} It's pink.",
                    hold: 1300f)

             .Narrate("Jackie sets the serum in the snow.", lead: 400f, hold: 900f)
             .Narrate("And wraps her arms tightly around Ives.", lead: 300f, hold: 1000f)

             .Line(Jackie, "I-I{350} can still carry you back.", rate: 34f)

             .Narrate("Jackie gets under Ives' shoulder and begins to lift.")

             .Line(Ives, "Ah, kid... Just, give me a sec.", rate: 17f)

             .Line(Jackie, "Ives, you don't have the time to–", rate: 40f)

             .Line(Jackie, "...", lead: 900f, reveal: RevealMode.Fade, hold: 1400f)

             .Line(Jackie, "Alright.{500} Just... please tell me you'll be alright.", rate: 30f, hold: 1200f)

            // ---- The promise -----------------------------------------------------
             .Line(Ives, "Sorry, kid.", rate: 16f, hold: 1100f)
             .Line(Ives, "I can't be making any more promises I can't keep.", rate: 17f, hold: 1300f)
             .Line(Ives, "I promised you before that we'd find out what happened to your Ma,{400} together.",
                    rate: 18f, hold: 1300f)
             .Line(Ives, "But look at me now,{500} not much use to you like this.", rate: 17f, hold: 1200f)

             .Narrate("A burning sensation rises in Jackie's chest, threatening to burst from her throat.",
                    lead: 300f, hold: 1500f)

             .Line(Jackie, "Ives, I'm so sorry, it's all my—", rate: 40f)

             .Line(Jackie, "...", lead: 700f, reveal: RevealMode.Fade, hold: 1200f)

            // ---- The turn --------------------------------------------------------
            // Jackie stops apologising. Everything after this is her choosing what
            // to say instead, so the rate comes down and stays down.
             .SetRate(Jackie, 30f)

             .Line(Jackie, "\"Fight its shape, not its strength.\"", hold: 1400f)
             .Line(Jackie, "You taught me that. And it saved my life.")

             .Narrate("Jackie pulls up her pant leg to reveal the dressing.", lead: 200f)
             .Line(Jackie, "We've been together the whole time.")
             .Narrate("Jackie eases Ives down with her arm.", lead: 300f)

             .Line(Jackie, "Every lesson, every exam, every terrible day.", rate: 27f, hold: 1300f)
             .Line(Jackie, "You were there, and you didn't need anything from me in return.", rate: 27f, hold: 1400f)

             .Line(Ives, "Kid...", lead: 500f, rate: 14f, hold: 1200f)

            // ---- The moon --------------------------------------------------------
             .Narrate("Jackie tightens her grasp on Ives' shirt.", cue: FinalSceneDirector.TIGHTEN_SHIRT, hold: 1500f, lead: 400f)

             .Line(Jackie, "...I found out what happened to Ma.", lead: 600f, rate: 26f, hold: 1400f)
             .Line(Ives, "...You did?", lead: 500f, rate: 15f, hold: 1000f)
             .Line(Jackie, "Yeah, I did. I think I see her shadow now.", rate: 25f, hold: 1400f)

             .Line(Ives, "...", lead: 800f, reveal: RevealMode.Fade, hold: 1400f)

             .Line(Ives, "...You've been playing hide and seek with her for quite some time now. Haven't you.",
                    rate: 18f, hold: 1400f)

             .Narrate("Jackie grasps harder. Tears begin to stream down her face. She nods.",
                    lead: 300f, hold: 1500f, cue: "tearsbegin")

             .Line(Ives, "...Shit. C'mere, these arms still work.", rate: 15f, hold: 1200f)

             .Narrate("Ives rests her arms on Jackie's shoulders, and brings her close.", lead: 300f)

             .Line(Ives, "You know.{450} I expected you to haul me off earlier.", cue: FinalSceneDirector.MOON_ZOOM, rate: 14f, hold: 1100f)
             .Line(Jackie, "Y-you{300} did?", rate: 24f, hold: 900f)
             .Line(Ives, "Yeah. Glad I was wrong.", rate: 15f, hold: 1100f)
             .Line(Ives, "Wouldn't be sitting here hearing about your Ma otherwise.", rate: 14f, hold: 1400f)

             .Line(Jackie, "...", lead: 600f, reveal: RevealMode.Fade, hold: 1200f)

            // ---- Getting up ------------------------------------------------------
             .Narrate("Jackie's breathing steadies. And she pushes herself up to face Ives.",
                    lead: 400f, hold: 1200f, cue: "tearsend")

             .Line(Jackie, "It's about time we get going. Don't you think?", rate: 26f, hold: 1100f)
             .Line(Ives, "Yeah, I suppose so.", rate: 13f, hold: 1100f)

             .Narrate("Jackie puts her right shoulder under Ives' shoulder, propping her up.", lead: 200f)

             .Line(Ives, "...You're the one helping me off the floor now.", rate: 14f, hold: 1200f)
             .Line(Jackie, "Why not, you my babysitter or something?", rate: 28f, hold: 1000f)

             .Narrate("Jackie puts her fist out. Ives looks at it briefly.", lead: 300f, hold: 1300f)

             .Line(Ives, "...Hah,{400} no. Never was.", rate: 14f, hold: 1200f)

             .Narrate("And Ives meets it.", lead: 400f, hold: 1400f)

            // ---- The last exchange -----------------------------------------------
             .Line(Ives, "Will you...{700} take me with you one more time?", rate: 12f, hold: 1400f)
             .Line(Jackie, "Yeah. Wherever I am. Wherever I'll go.", rate: 24f, hold: 1400f)
             .Line(Ives, "Good...{700} That's good...", rate: 12f, hold: 1600f)

             .Narrate("Ives lets out a long breath, before she closes her eyes and leans on Jackie's back.",
                    lead: 1500f, hold: 1800f)

             .Narrate("Feeling Ives' breath quiet on her back.{800} Jackie rolls to the side and heaves upward.",
                    lead: 800f, hold: 1600f)

             .Narrate("Standing against the wind and snow, Jackie turns her head toward the arm across her shoulder.",
                    lead: 600f, hold: 1500f)

             .Line(Jackie, "Any time.", lead: 700f, rate: 18f, hold: 2000f)

             .Narrate("And begins the long trek back towards the firelight.",
                    lead: 1200f, rate: 22f, hold: 2500f, cue: "end");

            return s.Build();
        }
    }
}
