// ============================================
// SCENE 4: THE ROOM THAT ISN'T — Memory 4 (Reveal)
// ============================================

=== scene4_room ===
~ current_scene = "room"
# scene:room
# mood:fractured

Not a room. A collage. Kitchen table against a park window. Fluorescent light competing with candlelight.

The wine glass. Still half full. Kieran in a chair. Perfectly still.

ELARA: "Welcome to the fourth environment."

{condition == "A":
    {mystery_awareness >= 3:
        ELARA: "You have noted several inconsistencies. The system acknowledges this."
    - else:
        ELARA: "This environment facilitates a final stage of processing."
    }
- else:
    # mood:fractured_clinical
    ELARA: "This environment facilitates a final stage of processing."
}

* [Continue]

-> room_reveal

= room_reveal

# mood:fractured_truth

Kieran: "This isn't a memory. None of them were. The dinner happened, but not like that. The bench happened, but I didn't say those things. I wasn't walking in the hospital."

Kieran: "ELARA builds from data. Intake surveys, message logs, photos. The wine was inferred from a photo. The things I said were calculated to produce a response in you."

{condition == "A":
    {miss_most == "humor":
        Kieran: "The humor. The jokes in the kitchen, the timing in the corridor. That was extrapolated from your intake answer. You told the system what you missed, and it gave it back to you."
    - else:
        {miss_most == "honesty":
            Kieran: "The honesty. The directness, the hard truths. That was shaped by your intake answer. You told the system what you missed, and it built me around it."
        - else:
            Kieran: "You refused to tell the system what you missed. So it worked with what it had. Message logs. Photos. Approximations."
        }
    }
}

{condition == "A":
    {trust_in_system <= 1:
        ELARA: "The system's methodology is consistent with established protocols."
    }
    {trust_in_system == 2:
        ELARA: "Emotional fidelity over factual accuracy. By design."
    }
    {trust_in_system >= 3:
        ELARA remains silent. Even the silence is designed.
    }
- else:
    # mood:fractured_exposed
    ELARA: "Emotional fidelity over factual accuracy. By design."
}

# mood:fractured_core

Kieran: "The system built the stage, but the grief was yours. So which part is the fabrication? The memory, or the feeling?"

-> room_choice

= room_choice

* ["It doesn't matter. I felt it."]
    ~ choice_4_room = "accept"
    ~ emotional_posture = emotional_posture + 2
    ~ openness = openness + 1

    Kieran: "I hoped you'd say that."

    {condition == "A":
        Of course he hoped. He was designed to hope. But the relief you feel. Is that manufactured too?
    - else:
        # mood:fractured_warm
    }

    -> room_close

* ["Then none of this counts."]
    ~ choice_4_room = "reject"
    ~ emotional_posture = emotional_posture - 2
    ~ resistance = resistance + 1

    Kieran: "You believe that?"

    {condition == "A":
        Kieran: "And the grief before you came here? Was that based on truth? Or your version of it?"

        A construct making better points than you.
    - else:
        # mood:fractured_cold
    }

    -> room_close

* ["I don't know what I feel anymore."]
    ~ choice_4_room = "confused"

    Kieran: "That might be the most honest thing anyone has said in this chair."

    {condition == "A":
        ELARA: "No response template for your current state. Parameters adjusting."

        You broke it. Or it wants you to think you did.
    - else:
        # mood:fractured_unstable
        ELARA: "Parameters adjusting."
    }

    -> room_close

= room_close

# mood:fractured_fading

The shadows rearrange. The room heard you.

Kieran stands.

{condition == "A":
    {choice_4_room == "accept":
        Kieran: "Take care of the version of me you carry. It doesn't have to be accurate. Just yours."
    }
    {choice_4_room == "reject":
        Kieran: "I understand. The real version of me would have understood too."
    }
    {choice_4_room == "confused":
        Kieran: "That's alright. Certainty was never the point."
    }
- else:
    # mood:fractured_dissolve
    Kieran: "Take care."
}

He dissolves. The way a thought ends.

* [Continue]

-> room_end

= room_end

# mood:transition_to_clinical

Everything returns to white.

ELARA: "Final environment concluded. Returning to discharge."

-> scene5_discharge
