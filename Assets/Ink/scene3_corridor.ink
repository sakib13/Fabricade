// ============================================
// SCENE 3: THE HOSPITAL CORRIDOR — Memory 3 (Dread)
// ============================================

=== scene3_corridor ===
~ current_scene = "corridor"
# scene:corridor
# mood:clinical_cold

{condition == "A":
    {openness > deflection && openness > resistance:
        Fluorescent light. The corridor stretches. You keep walking.
    - else:
        {resistance > openness && resistance > deflection:
            Fluorescent light. Sterile. The corridor doesn't care if you walk it.
        - else:
            {deflection > openness && deflection > resistance:
                Fluorescent light. The corridor stretches. You've been walking toward this and away from it.
            - else:
                Fluorescent light. The kind that exposes rather than illuminates.
            }
        }
    }
- else:
    Fluorescent light. The kind that exposes rather than illuminates.
}

Hospital corridor. Room 14 ahead. You know what's there.

~ mystery_awareness = mystery_awareness + 1

On the nurses' station: a wine glass. Red. From the dinner. It shouldn't be here.

# mood:clinical_fracture

Then Liam is beside you. Walking. He should not be here. In the real memory, he's in the bed.

~ mystery_awareness = mystery_awareness + 1

{miss_most == "humor":
    Liam: "You look like you've seen a ghost. Which, fair point."
    Too precise. Too perfectly timed. He always knew when to break the tension. But this feels rehearsed.
- else:
    {miss_most == "honesty":
        Liam: "You know I shouldn't be here. I know you know. Let's keep walking anyway."
        Too direct. Exactly what he would have said. Exactly.
    - else:
        Liam looks at you but says nothing. The silence feels calculated.
    }
}

-> corridor_choice

= corridor_choice

[inner]Every instinct pulls in a different direction.

* [Stop walking.]
    ~ choice_3_corridor = "resist"
    ~ emotional_posture = emotional_posture - 1
    ~ resistance = resistance + 1
    ~ trust_in_system = trust_in_system - 1
    ~ mystery_awareness = mystery_awareness + 1

    "No. You're supposed to be in the room."

    Liam: "Where would you like me to be?"

    {condition == "A":
        People don't ask where you'd like them to be. Systems do.

        {resistance >= 2:
            ELARA: "You resisted again. The system is recalculating your profile."
        - else:
            ELARA: "You refused to continue. Registered."
        }
    - else:
        # mood:clinical_harsh
    }

    -> corridor_transition

* ["Are you real?"]
    ~ choice_3_corridor = "question"
    ~ trust_in_system = trust_in_system - 2
    ~ mystery_awareness = mystery_awareness + 2

    Liam: "Define real."

    {condition == "A":
        That's not Liam. Liam would have made a joke. This has studied him carefully.

        {mystery_awareness >= 3:
            ELARA: "You asked whether this was real. The system cannot answer that. But it recorded the question."
        - else:
            ELARA: "You asked whether this was real. Noted."
        }
    - else:
        # mood:clinical_glitch
    }

    -> corridor_transition

= corridor_transition

The fluorescent hum changes pitch.

* [Continue]

-> corridor_close

= corridor_close

# mood:clinical_dissolve

Room 14. Door closed.

ELARA: "Third environment concluding."

# mood:transition_to_fractured

The door opens. Behind it is not a hospital room.

-> scene4_room
