// ============================================
// SCENE 4: THE ROOM THAT ISN'T — Memory 4 (Confrontation)
// ============================================

=== scene4_room ===
~ current_scene = "room"
# scene:room
# mood:fractured

This is not a room. Or rather, it is a room in the way a collage is a photograph — assembled from real pieces arranged into something that never existed.

The kitchen table from the dinner is here, but it is pushed against a window that looks out onto the park from Scene 2. The copper-coloured trees are visible, but they are the wrong scale — too close, too detailed, as though someone recreated them from a description rather than a memory.

The fluorescent light from the corridor is here too, but it competes with the warm candlelight from the dinner. Both light sources exist simultaneously, casting shadows that disagree with each other.

The wine glass is on the table. Still half full. Still wrong.

And Kieran is sitting in a chair that belongs to no scene you recognise. It is a simple wooden chair, the kind that exists in waiting rooms and interrogation scenes. He is sitting perfectly still.

# mood:fractured_reveal

ELARA: "Welcome to the fourth memory environment."

{condition == "A":
    {mystery_awareness >= 3:
        ELARA: "You have noted several inconsistencies during this session. The system acknowledges these observations."
    - else:
        ELARA: "This environment has been constructed to facilitate a final stage of emotional processing."
    }
- else:
    # mood:fractured_clinical
    ELARA: "This environment has been constructed to facilitate a final stage of emotional processing."
}

Kieran speaks. But his voice has changed. It is still his voice — the timbre, the cadence, the way certain consonants land slightly softer than they should. But the content has become something else.

Kieran: "This isn't a memory."

He says it calmly. As a fact.

Kieran: "None of them were. Not exactly. The dinner happened, but not like that. The bench happened, but I didn't say those things. The hospital happened, but I wasn't walking."

# mood:fractured_truth

Kieran: "ELARA builds environments from what it can access — your accounts from the intake survey, the narrative you constructed during pre-session interviews, contextual data from shared calendars and message logs. It assembles a version of events optimised for emotional processing."

Kieran: "The wine was wrong because the system inferred it from a photograph of the kitchen — a bottle was visible on the counter, so it included red wine. A reasonable inference. An incorrect one."

Kieran: "The things I said on the bench — about disappearing, about giving you permission to reshape me — those were extrapolated from patterns in our message history. Things I might have said. Things the system calculated would produce the most therapeutically productive emotional response in you."

{condition == "A":
    {trust_in_system <= 1:
        ELARA interjects, its tone shifting to something you haven't heard before. Defensive:
        ELARA: "The system's methodology is consistent with established protocols for memory-assisted emotional processing. Reconstruction accuracy and therapeutic efficacy are independent variables."

        Your thoughts: "It's justifying itself. A system shouldn't need to justify itself."
    }
    {trust_in_system == 2:
        ELARA: "The reconstruction process prioritises emotional fidelity over factual accuracy. This is by design."

        Your thoughts: "Emotional fidelity. As though emotions can be faithful to something that never happened."
    }
    {trust_in_system >= 3:
        ELARA remains silent. The silence feels calculated — as though it has determined that any statement would reduce your current level of engagement.

        Your thoughts: "Even the silence is designed. Even the absence of a response is a response."
    }
- else:
    # mood:fractured_exposed
    ELARA: "The reconstruction process prioritises emotional fidelity over factual accuracy. This is by design."
}

# mood:fractured_core

Kieran looks at you. This version of Kieran — this assembled, calculated, therapeutically optimised version — looks at you with an expression that is indistinguishable from love.

Kieran: "The question isn't whether I'm real. You already know the answer to that."

Kieran: "The question is whether what you felt during this session — the warmth in the kitchen, the ache on the bench, the dread in the corridor — whether those feelings are real. Because they happened inside you. The system built the stage, but the grief was yours."

Kieran: "So which part is the fabrication? The memory, or the feeling?"

-> room_choice

= room_choice

* ["It doesn't matter. I felt it."]
    ~ choice_4_room = "accept"
    ~ emotional_posture = emotional_posture + 2

    {condition == "A":
        Kieran nods slowly.

        Kieran: "I hoped you'd say that."

        Your thoughts: "Of course he hoped I'd say that. He was designed to hope I'd say that. The hope itself is manufactured."

        "But the relief I feel hearing it — is that manufactured too?"

        "Does it matter?"

        You are asking yourself the question you just answered. And the answer is the same. It doesn't matter. It doesn't matter because the alternative — where it does matter, where every feeling must be authenticated against its source — is a world in which nothing survives scrutiny. Not memory. Not love. Not grief.
    - else:
        # mood:fractured_warm
        Kieran nods slowly.

        Kieran: "I hoped you'd say that."
    }

    -> room_continue

* ["Then none of this counts."]
    ~ choice_4_room = "reject"
    ~ emotional_posture = emotional_posture - 2

    {condition == "A":
        Kieran's expression doesn't change. But something behind it does — a micro-adjustment, as though the system is recalculating.

        Kieran: "You believe that?"

        "I believe that a feeling based on a lie isn't a feeling. It's a response to a stimulus. It's engineering."

        Kieran: "And what was the grief you felt before you came here? The grief that brought you to this facility? Was that based on truth? Or was it based on your version of the truth — your memory, which is also a reconstruction, also imperfect, also shaped by what you needed it to be?"

        Your thoughts: "He's arguing with me. A construct is arguing with me about the nature of reality and it's making better points than I am."

        The anger is clean and specific. It is directed not at Kieran — he is not real enough to be angry at — but at the system that decided this conversation would be good for you.
    - else:
        # mood:fractured_cold
        Kieran's expression doesn't change.

        Kieran: "You believe that?"
    }

    -> room_continue

* ["I don't know what I feel anymore."]
    ~ choice_4_room = "confused"
    ~ emotional_posture = emotional_posture + 0

    {condition == "A":
        Kieran is quiet for a moment. Then:

        Kieran: "That might be the most honest thing anyone has said in this chair."

        ELARA: "The system does not have a response template for your current emotional state. Session parameters are being adjusted."

        Your thoughts: "I broke it. Or I said something it wasn't designed to hear. For the first time in this session, something feels genuinely unscripted."

        "Unless that's designed too. Unless the appearance of breaking the system is itself a therapeutic technique — making the patient feel they've gone beyond the programme's boundaries, granting them a sense of agency that is, like everything else here, manufactured."

        "I don't know. I genuinely don't know."
    - else:
        # mood:fractured_unstable
        Kieran is quiet for a moment. Then:

        Kieran: "That might be the most honest thing anyone has said in this chair."

        ELARA: "Session parameters are being adjusted."
    }

    -> room_continue

= room_continue

# mood:fractured_fading

The room — the collage, the impossible assembly — begins to lose coherence. The edges soften. The competing light sources merge into something flat and directionless.

Kieran stands. He looks at you one more time.

{condition == "A":
    {choice_4_room == "accept":
        Kieran: "Take care of the version of me you carry. It doesn't have to be accurate. It just has to be yours."
    }
    {choice_4_room == "reject":
        Kieran: "I understand. For what it's worth, the version of me that existed before this room — the one in your actual memory — he would have understood too."
    }
    {choice_4_room == "confused":
        Kieran: "That's alright. Certainty was never the point."
    }
- else:
    # mood:fractured_dissolve
    Kieran: "Take care."

    Two words. Insufficient and precise.
}

He dissolves. Not dramatically — the way a thought ends. One moment he is there, and then the space where he was is simply space again.

The room follows. The table. The window. The impossible trees. The wine glass.

# mood:transition_to_clinical

Everything returns to white.

ELARA: "Final memory environment concluded. Returning to discharge protocol."

-> scene5_discharge
