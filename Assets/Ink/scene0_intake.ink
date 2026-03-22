// ============================================
// SCENE 0: INTAKE — Prologue
// ============================================

=== scene0_intake ===
~ current_scene = "intake"
# scene:intake
# mood:clinical

The waiting room is white. Not warm white. Not cold white. The kind of white that has been engineered to mean nothing.

You are sitting in a chair that was designed to be comfortable for exactly thirty minutes. After that, it becomes a reminder that you should not still be here.

There is a glass of water on the table beside you. The condensation has formed a perfect ring on the surface beneath it. You wonder if someone placed it there recently, or if it has been waiting for you.

# mood:clinical_soft

A screen on the wall activates. The text appears in clean, measured typography.

ELARA: "Good morning. Welcome to the Emotional Landscape and Retrieval Architecture programme."

ELARA: "This facility offers guided re-entry into reconstructed memory environments. The process is non-invasive, voluntary, and designed to support emotional processing following significant personal loss."

ELARA: "You have been cleared for a standard session. Today, you will re-enter four memory environments associated with your friend, Kieran Althaus."

A pause. The cursor blinks once.

ELARA: "Before we begin, the system requires a brief calibration. Please respond to the following prompt openly. There are no clinical implications to your answer."

ELARA: "Why are you here today?"

-> intake_choice

= intake_choice

* ["I want to understand what happened."]
    ~ choice_0_intake = "understand"
    ~ emotional_posture = emotional_posture - 1
    ~ trust_in_system = 3

    {condition == "A":
        ELARA: "Noted. The system will prioritise episodic clarity and sequential coherence in your memory environments. Analytical frameworks tend to benefit from structured reconstruction."
    - else:
        # mood:cool_blue
        ELARA: "Noted. The system will calibrate accordingly."
    }

    You chose the safe answer. The clinical one. The one that keeps the grief at arm's length, where you can study it like a diagram.

    -> intake_proceed

* ["I miss him."]
    ~ choice_0_intake = "miss"
    ~ emotional_posture = emotional_posture + 1
    ~ trust_in_system = 2

    {condition == "A":
        ELARA: "Noted. The system will prioritise emotional resonance and interpersonal fidelity in your memory environments. The programme is designed to honour the weight of what you carry."
    - else:
        # mood:warm_amber
        ELARA: "Noted. The system will calibrate accordingly."
    }

    Three words. You didn't expect them to cost that much. But there they are, hanging in the white room like smoke.

    -> intake_proceed

* ["Someone told me this would help."]
    ~ choice_0_intake = "told"
    ~ emotional_posture = emotional_posture - 1
    ~ trust_in_system = 1

    {condition == "A":
        ELARA: "Noted. Referral-based participants represent 43% of our intake. The system does not require personal conviction to function. Efficacy is independent of expectation."

        That sounded rehearsed. As though the system has said it many times before, to many people who did not want to be here.
    - else:
        # mood:flat_grey
        ELARA: "Noted. The system will calibrate accordingly."
    }

    You are not sure who told you. A colleague. A therapist. Someone who had done it and spoke about it with the careful neutrality of someone who was not ready to say whether it had worked.

    -> intake_proceed

= intake_proceed

# mood:transition

ELARA: "Calibration complete. You will now be guided to the preparation chamber."

The screen dims. A door you hadn't noticed opens in the wall to your left. The corridor beyond is softly lit. The floor makes no sound beneath your feet.

A reclining chair sits in the centre of the next room. It looks medical but has been dressed to look less so — a thin cushion, a blanket folded at its foot. The effort to appear non-threatening is itself unsettling.

ELARA: "Please make yourself comfortable. The session will consist of four memory environments, presented sequentially. You may speak during the experience. The system will not respond during active reconstruction, but your responses are recorded for post-session review."

ELARA: "A reminder: the memories you experience are reconstructions. They are assembled from available data — your personal accounts, shared records, and contextual modelling. Minor variations from your recollection are expected and do not indicate system error."

You settle into the chair. The cushion is warmer than it should be, as though someone was sitting here moments ago.

ELARA: "Initiating first memory environment."

The lights lower. The white walls dissolve.

# mood:transition_to_warm

And then you are somewhere else.

-> scene1_dinner
