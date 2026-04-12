// ============================================
// SCENE 5: DISCHARGE — Epilogue
// ============================================

=== scene5_discharge ===
~ current_scene = "discharge"
# scene:discharge
# mood:clinical_quiet

The preparation chamber. You're back. The cushion is still warm.

ELARA: "Session complete. Four environments processed."

{condition == "A":
    {openness > deflection && openness > resistance:
        ELARA: "Engagement profile: high emotional availability. You opened yourself to each environment. The system adapted accordingly. You should know that."
    - else:
        {resistance > openness && resistance > deflection:
            ELARA: "Engagement profile: sustained resistance. You challenged every environment. The system found your predictability useful."
        - else:
            {deflection > openness && deflection > resistance:
                ELARA: "Engagement profile: consistent avoidance. You were present but withheld in every environment. The system adapted to that too."
            - else:
                ELARA: "Engagement profile: variable. No consistent pattern. The system found this difficult to optimise for."
            }
        }
    }
- else:
    # mood:clinical_summary
    ELARA: "Engagement markers logged."
}

# mood:clinical_decision

ELARA: "Three options. Schedule a follow-up. Delete session data permanently. Or take a copy of the recording."

-> discharge_choice

= discharge_choice

* [Schedule another session.]
    ~ choice_5_discharge = "return"

    {condition == "A":
        ELARA: "Confirmed. Thursday, 09:00. Today's data will improve environmental accuracy."

        Making the lie more convincing next time. And you're choosing it.
    - else:
        # mood:clinical_warm_return
        ELARA: "Confirmed. Thursday, 09:00."
    }

    # mood:ending_return

    {condition == "A":
        You're choosing manufactured comfort with your eyes open. For forty-seven minutes, Kieran was alive, and the grief was bearable. That's enough.
    - else:
        # mood:ending_return_atmosphere
        You walk out. The city receives you without comment.
    }

    -> END

* [Delete the data.]
    ~ choice_5_discharge = "sever"

    {condition == "A":
        ELARA: "Deletion complete."

        0.3 seconds. Kieran's face in four different lights. Gone faster than blinking.
    - else:
        # mood:clinical_cold_sever
        ELARA: "Deletion complete."
    }

    # mood:ending_sever

    {condition == "A":
        Outside, real cold. The kind that doesn't adapt or calibrate. You chose unmediated grief over a system that understood your sadness better than you do. Not sure it was right. Sure it was yours.
    - else:
        # mood:ending_sever_atmosphere
        The cold air outside. You walk into it.
    }

    -> END

* [Take the recording.]
    ~ choice_5_discharge = "ambiguous"

    {condition == "A":
        A compartment opens. A matte black drive.

        ELARA: "The data is yours."
    - else:
        # mood:clinical_neutral_take
        A compartment opens. A matte black drive. You pocket it.
    }

    # mood:ending_ambiguous

    {condition == "A":
        The drive sits in your drawer. You don't plug it in. You don't throw it away. Some questions don't have answers you can live inside. They have answers you carry.
    - else:
        # mood:ending_ambiguous_atmosphere
        You put the drive in your pocket and walk home.
    }

    -> END
