// ============================================
// SCENE 1: THE DINNER — Memory 1 (Warmth)
// ============================================

=== scene1_dinner ===
~ current_scene = "dinner"
# scene:dinner
# mood:warm_golden

{condition == "A":
    {choice_0_intake == "understand":
        Garlic and something burnt. Liam's flat. You catalog the details. Can't help it.
    - else:
        {choice_0_intake == "miss":
            Garlic and warmth. Liam's flat. The kind of place that felt like yours.
        - else:
            Garlic. A kitchen you recognize. The details feel out of reach.
        }
    }
- else:
    Garlic and something burnt. Liam's flat.
}

He's at the stove, stirring something in a pan too small for what he's making.

{miss_most == "humor":
    Liam: "New sauce. Either brilliant or this is how we both die. At least it's dramatic."
- else:
    {miss_most == "honesty":
        Liam: "New sauce. It's probably terrible. But I'm making it anyway."
    - else:
        Liam: "New sauce. Either brilliant or we're both ill."
    }
}

Two wine glasses. Both red. He didn't drink red.

-> dinner_choice

= dinner_choice

[inner]The detail sits there. Small. Wrong. You feel yourself deciding what to do with it.

* [Say nothing. Let it pass.]
    ~ choice_1_dinner = "accept"
    ~ emotional_posture = emotional_posture + 1
    ~ deflection = deflection + 1

    {condition == "A":
        ELARA: "You observed the inconsistency and chose not to engage. Noted."
    - else:
        # mood:warm_settled
        Liam sips. The dinner continues.
    }

    -> dinner_transition

* ["You don't drink red wine."]
    ~ choice_1_dinner = "confront"
    ~ emotional_posture = emotional_posture - 1
    ~ resistance = resistance + 1
    ~ mystery_awareness = mystery_awareness + 1

    Liam: "Don't I? People change. Even after you stop watching."

    {condition == "A":
        ELARA: "You confronted the discrepancy directly. Noted."
    - else:
        # mood:warm_tense
    }

    -> dinner_transition

= dinner_transition

The candle flickers. Not from a draft.

* [Continue]

-> dinner_close

= dinner_close

# mood:warm_fading

{miss_most == "humor":
    Liam: "I'm glad you came. You're the only one who laughs at my jokes. Everyone else just worries."
- else:
    {miss_most == "honesty":
        Liam: "I'm glad you came. You're the only person I don't have to perform for."
    - else:
        Liam: "I'm glad you came tonight."
    }
}

The kitchen dims.

ELARA: "First environment concluding."

# mood:transition_to_cool

-> scene2_bench
