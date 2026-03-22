// ============================================
// SCENE 1: THE DINNER — Memory 1 (Warmth)
// ============================================

=== scene1_dinner ===
~ current_scene = "dinner"
# scene:dinner
# mood:warm_golden

The kitchen smells like garlic and something burnt at the edges. It takes you a moment to place it — Kieran's flat. The one on Södermalm with the window that never closed properly and the radiator that ticked like a metronome.

He is standing at the stove with his back to you, stirring something in a pan that is too small for what he is making. This was always his way. Everything slightly too ambitious for the tools at hand.

Kieran: "I think I've invented a new kind of pasta sauce. It's either brilliant or it's going to make us both ill."

He turns and grins. That grin. The one that made you trust him with things you shouldn't have, because anyone who smiled like that couldn't possibly let you down.

The table is set for two. Mismatched plates. A candle that has already burned halfway down. And two glasses of wine — both red.

# mood:warm_unsettled

You look at the wine. Something shifts, very faintly, at the back of your mind.

Kieran didn't drink red wine. He had a thing about it — said it tasted like iron and regret. He drank white, or beer, or nothing at all. You remember this clearly. You remember making fun of him for it.

But here he is, reaching for the glass, lifting it to his lips as though this is the most natural thing in the world.

{condition == "A":
    Your thoughts surface like text on a screen:
    "That's not right. He never drank red wine. I'm sure of it. Aren't I?"
- else:
    # mood:warm_with_dissonance
    You notice the wine. Something about it feels wrong, but you cannot quite name it.
}

-> dinner_choice

= dinner_choice

* [Say nothing. Let the memory play out.]
    ~ choice_1_dinner = "accept"
    ~ emotional_posture = emotional_posture + 1
    ~ mystery_awareness = mystery_awareness + 0

    You let it pass. It is a small thing. A detail. Maybe you are misremembering. Maybe he tried red wine once and you forgot. People are allowed to change in the spaces between your memories of them.

    {condition == "A":
        Kieran takes a sip and sets the glass down. He looks relaxed. At ease. The dinner continues as though nothing has happened, because nothing has.

        And the thought dissolves, like sugar in warm water. You let it.
    - else:
        # mood:warm_settled
        Kieran takes a sip and sets the glass down. The dinner continues.
    }

    -> dinner_continue

* ["You don't drink red wine."]
    ~ choice_1_dinner = "confront"
    ~ emotional_posture = emotional_posture - 1
    ~ mystery_awareness = mystery_awareness + 1

    The words come out before you can soften them.

    {condition == "A":
        Kieran pauses. The grin doesn't disappear, but it changes — becomes something practiced rather than spontaneous.

        Kieran: "Don't I? Maybe I started. People change, you know. Even after you stop watching."

        The sentence lands strangely. Even after you stop watching. You are not sure if he means it the way it sounds.

        Your thoughts: "That was defensive. He was never defensive about small things. Something is off."
    - else:
        # mood:warm_tense
        Kieran pauses, then shrugs.

        Kieran: "Don't I? Maybe I started. People change, you know. Even after you stop watching."
    }

    -> dinner_continue

* [Reach for the glass yourself. Change the subject internally.]
    ~ choice_1_dinner = "deflect"
    ~ emotional_posture = emotional_posture + 0
    ~ mystery_awareness = mystery_awareness + 0

    You pick up your own glass and take a long sip. The wine is good. Too good for Kieran's usual budget. Another small detail that doesn't quite fit, but you tuck it away.

    {condition == "A":
        Your thoughts: "Don't look at it too closely. You're here to remember, not to investigate. Just be here."

        You focus on the taste, the weight of the glass, the warmth of the room. These things are real enough.
    - else:
        # mood:warm_muted
        You focus on the taste, the weight of the glass, the warmth of the room.
    }

    -> dinner_continue

= dinner_continue

# mood:warm_golden

Kieran serves the pasta. It is, predictably, too much for two people. He has overcooked the garlic and undercooked the onions. It is exactly as you remember.

Kieran: "So I watched that documentary. The one about the architect who built a house and then refused to live in it."

Kieran: "He said the design was perfect but the experience of living in it would contaminate the idea. That the house was better as a concept than as a home."

{condition == "A":
    {emotional_posture > 0:
        Kieran leans forward, animated.
        Kieran: "I thought it was beautiful, honestly. Sad, but beautiful. Like he loved the idea of the house more than he could love the actual walls and floors."

        You listen. He always did this — found the tender nerve in something and pressed on it gently, not to hurt, but to show you it was there.
    }
    {emotional_posture == 0:
        Kieran shrugs, half-smiling.
        Kieran: "Strange man. But I sort of understood him. Sometimes the idea of something is easier to hold onto than the thing itself."

        He trails off. The sentence sits between you like something he didn't mean to say out loud.
    }
    {emotional_posture < 0:
        Kieran watches you carefully as he speaks.
        Kieran: "You probably think that's ridiculous. That a house should be lived in. That things should be used for what they're built for."

        He is testing you. You are not sure when he started doing that.
    }
- else:
    # mood:warm_steady
    Kieran: "I thought about it for days afterward. Whether it's possible to love something more as an idea than as a reality. Whether the gap between the two is where grief lives."

    He says this casually, as though he hasn't just said something that will sit inside you for years.
}

You eat. The food is imperfect and comforting. The candle flickers. Outside the window that never closes, the city hums its low evening frequency.

{condition == "A":
    {choice_1_dinner == "confront":
        The wine sits in its glass, untouched by Kieran now. You notice he hasn't taken another sip since you mentioned it. The detail files itself away.
    }
}

# mood:warm_fading

Kieran: "I'm glad you came over tonight."

He says it simply. Without emphasis. The way you say things that are true enough to not need decoration.

And then the kitchen begins to dim. Not dramatically — the way an evening ends. The candle burns lower. The sounds outside soften. Kieran's face is still there, but the edges of the room are losing their definition.

ELARA: "First memory environment concluding. Transitioning to second environment."

# mood:transition_to_cool

The warmth drains from the room like water from a basin. The last thing you see is the wine glass on the table, catching light that is no longer there.

-> scene2_bench
