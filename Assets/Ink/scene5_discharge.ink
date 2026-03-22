// ============================================
// SCENE 5: DISCHARGE — Epilogue
// ============================================

=== scene5_discharge ===
~ current_scene = "discharge"
# scene:discharge
# mood:clinical_quiet

The preparation chamber. The reclining chair. The blanket that has not moved. You are back in the room where this began, and it looks exactly the same, which is either reassuring or the final proof that nothing in this facility responds to what has happened inside it.

You sit up. The cushion is still warm.

ELARA: "Session complete. Duration: forty-seven minutes. Four memory environments processed."

{condition == "A":
    {emotional_posture >= 2:
        ELARA: "Emotional processing markers indicate significant engagement. The system has logged elevated responsiveness across all four environments, with peak engagement during the second environment."

        The clinical summary of your grief, delivered with the warmth of a receipt.
    }
    {emotional_posture <= -2:
        ELARA: "Emotional processing markers indicate significant resistance. The system has noted elevated critical engagement across multiple environments. This pattern is within expected parameters for analytical participants."

        Resistance. They have a word for it. A category. You fit into a category.
    }
    {emotional_posture > -2 && emotional_posture < 2:
        ELARA: "Emotional processing markers indicate a mixed engagement profile. The system has logged variable responsiveness, consistent with participants undergoing initial recalibration."

        Mixed. As though ambivalence is a diagnostic category rather than the only reasonable response to what just happened.
    }
- else:
    # mood:clinical_summary
    ELARA: "Emotional processing markers have been logged. A post-session summary will be available for your review."
}

# mood:clinical_decision

ELARA: "Before you leave, the programme offers three post-session options."

ELARA: "Option one: schedule a follow-up session. The system can refine memory environments based on today's data, improving emotional fidelity in subsequent reconstructions."

ELARA: "Option two: delete this session's data. All recordings, logs, and reconstruction parameters from today's session will be permanently removed. This action cannot be undone."

ELARA: "Option three: take a copy of this session's recording. A compressed file will be provided on a portable drive. The data remains yours to review or discard at your discretion."

ELARA: "There is no clinical recommendation. The choice is yours."

-> discharge_choice

= discharge_choice

* [Schedule another session.]
    ~ choice_5_discharge = "return"

    "Thursday. Same time."

    {condition == "A":
        ELARA: "Confirmed. Session scheduled for Thursday, 09:00. The system will incorporate today's engagement data to improve environmental accuracy."

        Environmental accuracy. As though the goal is to make the lie more convincing next time.

        You stand. The door to the waiting room opens. The corridor is the same soft light as before. Your shoes make the same sound.

        But something has shifted. Not in the facility — in you. You are choosing to come back. Not because you believe the memories are real. Not because you trust the system. But because for forty-seven minutes, Kieran was alive in a room with you, and the grief was bearable, and the weight you carried in was distributed across the chair and the screen and the soft white walls.

        You are choosing manufactured comfort, and you are choosing it with your eyes open. There is a word for that, probably. You don't care what it is.
    - else:
        # mood:clinical_warm_return
        ELARA: "Confirmed. Session scheduled for Thursday, 09:00."

        You stand. The door to the waiting room opens.
    }

    # mood:ending_return

    {condition == "A":
        The waiting room. The chair designed for thirty minutes. The glass of water with its perfect condensation ring. You walk past them toward the exit.

        The thought arrives without effort, without drama:

        "I'll come back on Thursday. Same time. I don't think it matters whose memories they are, as long as someone remembers."

        The door closes behind you. The city is loud and bright and indifferent. You step into it carrying something that might be hope, or might be dependency, or might be the quiet recognition that all memory is reconstruction, and this programme is simply more honest about it than the rest of us.
    - else:
        # mood:ending_return_atmosphere
        The waiting room. The glass of water. The exit.

        You walk through it. The city receives you without comment.
    }

    -> END

* [Delete the session data.]
    ~ choice_5_discharge = "sever"

    "Delete it. All of it."

    {condition == "A":
        ELARA: "Confirming deletion request. This will remove all environmental reconstruction data, engagement logs, and derived parameters from today's session. Once deleted, this data cannot be recovered. Do you wish to proceed?"

        "Yes."

        A pause. Shorter than you expected.

        ELARA: "Deletion complete. Session data removed."

        The speed of it is what stays with you. The entire session — the kitchen, the bench, the corridor, the impossible room, Kieran's face in four different lights — reduced to a database operation that completed in less time than it takes to blink.

        You stand. The chair releases you without resistance.

        The thought arrives like a verdict:

        "The file deleted in 0.3 seconds. Kieran took longer than that to die. Some things should take the time they take."
    - else:
        # mood:clinical_cold_sever
        ELARA: "Deletion complete. Session data removed."

        You stand. The chair releases you without resistance.
    }

    # mood:ending_sever

    {condition == "A":
        The waiting room is unchanged. The water glass. The chair. The engineered neutrality. You walk through it and it means nothing.

        Outside, the air is cold. Real cold — the kind that exists independent of your emotional state, that does not adapt or calibrate or optimise. You pull your jacket tighter. It is the right thickness for the weather.

        You have deleted the only version of Kieran you could still sit across from. You have chosen the loneliness of unmediated grief over the comfort of a system that understood your sadness better than you do.

        You are not sure this was the right decision. You are sure it was yours.
    - else:
        # mood:ending_sever_atmosphere
        The waiting room. The exit. The cold air outside.

        You walk into it.
    }

    -> END

* [Take the recording. Say nothing.]
    ~ choice_5_discharge = "ambiguous"

    You say nothing for a moment. Then:

    "The recording. I'll take a copy."

    {condition == "A":
        ELARA: "Acknowledged. Preparing portable copy."

        A small compartment opens in the wall beside the chair. Inside is a drive — matte black, featureless, roughly the size of a thumbnail. Forty-seven minutes of reconstructed grief, compressed into something you could lose between sofa cushions.

        You take it. You put it in your pocket.

        ELARA: "The data is yours. The programme makes no recommendation regarding its use."

        You do not respond. ELARA does not ask you to.
    - else:
        # mood:clinical_neutral_take
        ELARA: "Acknowledged. Preparing portable copy."

        A small compartment opens in the wall. A matte black drive, the size of a thumbnail. You take it. You put it in your pocket.
    }

    # mood:ending_ambiguous

    {condition == "A":
        The waiting room. The glass of water. The door.

        You put the drive in your coat pocket, next to your keys and a receipt from a restaurant you don't remember visiting. You walked home. It was raining, you think. Or it might have been Tuesday.

        The drive stays in your pocket. You don't plug it in that evening, or the next. But you don't throw it away either. It sits in the drawer by your bed, next to a phone charger and a photograph of someone you are still deciding how to remember.

        Some questions don't have answers you can live inside. They have answers you carry around, like keys to doors you haven't found yet.
    - else:
        # mood:ending_ambiguous_atmosphere
        The waiting room. The door. The city.

        You put the drive in your coat pocket and walk home.
    }

    -> END
