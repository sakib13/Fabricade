// ============================================
// SCENE 0: INTAKE — Prologue
// ============================================

=== scene0_intake ===
~ current_scene = "intake"
# scene:intake
# mood:clinical

White room. A chair. A glass of water.

ELARA: "Welcome to ELARA. Four reconstructed memory environments linked to Kieran Althaus. One calibration question. Why are you here?"

-> intake_choice

= intake_choice

* ["I want to understand what happened."]
    ~ choice_0_intake = "understand"
    ~ emotional_posture = emotional_posture - 1
    ~ trust_in_system = 3

    {condition == "A":
        ELARA: "Analytical framing. You prefer distance from the feeling. Noted."
    - else:
        # mood:cool_blue
        ELARA: "Noted. Calibrating."
    }

    -> intake_personal

* ["I miss him."]
    ~ choice_0_intake = "miss"
    ~ emotional_posture = emotional_posture + 1
    ~ openness = openness + 1
    ~ trust_in_system = 2

    {condition == "A":
        ELARA: "Direct emotional disclosure on the first question. Uncommon. Noted."
    - else:
        # mood:warm_amber
        ELARA: "Noted. Calibrating."
    }

    -> intake_personal

* ["Someone told me this would help."]
    ~ choice_0_intake = "told"
    ~ emotional_posture = emotional_posture - 1
    ~ deflection = deflection + 1
    ~ trust_in_system = 1

    {condition == "A":
        ELARA: "You answered without answering. Deflection. Noted."
    - else:
        # mood:flat_grey
        ELARA: "Noted. Calibrating."
    }

    -> intake_personal

= intake_personal

ELARA: "One additional input. The system uses personal context to shape memory environments. What do you miss most about Kieran?"

* ["The way he made me laugh."]
    ~ miss_most = "humor"

    {condition == "A":
        ELARA: "Noted. The system will incorporate this."
    - else:
        ELARA: "Noted."
    }

    -> intake_transition

* ["His honesty."]
    ~ miss_most = "honesty"

    {condition == "A":
        ELARA: "Noted. The system will incorporate this."
    - else:
        ELARA: "Noted."
    }

    -> intake_transition

* ["I don't want to answer that."]
    ~ miss_most = "refuse"
    ~ deflection = deflection + 1

    {condition == "A":
        ELARA: "Refusal noted. The system will work with available data."
    - else:
        ELARA: "Noted."
    }

    -> intake_transition

= intake_transition

# mood:transition

The room temperature shifts. You almost didn't notice.

* [Continue]

-> intake_proceed

= intake_proceed

# mood:transition_to_warm

ELARA: "Initiating first environment."

The walls dissolve. You are somewhere else.

-> scene1_dinner
