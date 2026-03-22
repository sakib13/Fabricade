// ============================================
// SCENE 3: THE HOSPITAL CORRIDOR — Memory 3 (Dread)
// ============================================

=== scene3_corridor ===
~ current_scene = "corridor"
# scene:corridor
# mood:clinical_cold

Fluorescent light. The kind that doesn't illuminate so much as expose. Every surface is visible and none of it is worth seeing.

You are in a hospital corridor. You know this corridor. You have walked it before — once, in reality, on a Tuesday in March that you have tried very hard to make feel like any other Tuesday.

The floor is linoleum. Your shoes make a sound against it that is too loud for the silence around you. The walls are the colour of diluted milk. There are doors on either side, all closed. Numbers on each one, counting upward.

You are walking toward room 14. You know what is in room 14. You have always known.

# mood:clinical_dread

The corridor is long. Longer than you remember. The numbers on the doors are climbing — 7, 8, 9 — but the end of the hallway does not seem to be getting closer.

And then you notice something wrong.

On the nurses' station to your left, between a stack of clipboards and a hand sanitiser dispenser, there is a wine glass. Red wine. Half full. Condensation on the stem.

~ mystery_awareness = mystery_awareness + 1

{condition == "A":
    Your thoughts: "That's from the dinner. That's from the kitchen. That shouldn't be here."

    You stop walking. The glass sits there with the casual confidence of an object that knows it belongs, even though it does not.
- else:
    # mood:clinical_bleeding
    The glass catches the fluorescent light. Something about it is deeply wrong, but the corridor keeps pulling you forward.
}

You look to your right. Through a window that should show the hospital car park, you see a park bench. Copper-coloured trees. Fading October light.

~ mystery_awareness = mystery_awareness + 1

# mood:clinical_fracture

And then Kieran is beside you.

He is walking at your pace, hands in his pockets, as though he has been here the entire time. He is wearing the too-thin jacket from the park. His shoes make no sound on the linoleum.

He should not be here. In this memory — in the real version of this memory — Kieran is in room 14. He is in the bed in room 14. He is not walking.

{condition == "A":
    Your thoughts fragment:
    "This is wrong. This is wrong. He was in the room. He was always in the room. He couldn't walk. By this point he couldn't—"

    "The system is doing this. ELARA is assembling something that didn't happen. This isn't retrieval. This is construction."

    But his face. His face is exactly right. The way the tiredness sat in the skin beneath his eyes. The way his jaw held tension when he was pretending to be fine.
- else:
    # mood:clinical_impossible
    He should not be here. You know this with a certainty that sits in your body rather than your mind. And yet here he is, matching your stride.
}

Kieran looks at you. He speaks.

{condition == "A":
    {emotional_posture > 0:
        Kieran: "You're doing well. I know this part is hard. But you're almost there."

        His voice is gentle. Unbearably gentle. The gentleness of someone who knows they are a fiction and has chosen to be kind about it.
    }
    {emotional_posture == 0:
        Kieran: "Almost there. Just a few more doors."

        His voice is neutral. Matter-of-fact. As though walking through an impossible corridor alongside you is simply what the situation requires.
    }
    {emotional_posture < 0:
        Kieran: "You already know what's at the end. You knew before you started walking. The question is whether you're walking toward it or away from it."

        His voice is precise. Almost clinical. It sounds less like Kieran and more like something wearing his voice.
    }
- else:
    # mood:clinical_pressure
    Kieran: "Almost there. Just a few more doors."
}

-> corridor_choice

= corridor_choice

* [Talk to him. Pretend this is normal.]
    ~ choice_3_corridor = "deny"
    ~ emotional_posture = emotional_posture + 1
    ~ trust_in_system = trust_in_system - 1
    ~ mystery_awareness = mystery_awareness - 1

    "How are you feeling?"

    The words are absurd. You are asking a dead man how he is feeling while walking through a corridor that should not exist toward a room where he died. But the words come out, because the alternative is acknowledging that none of this is real, and you are not ready for that.

    {condition == "A":
        Kieran: "Better than I look. You know me — I always looked worse than I felt. Worried people for nothing."

        He smiles. It is the smile from the dinner. Warm. Trustworthy. Completely impossible.

        Your thoughts: "I'm choosing this. I'm choosing to pretend because pretending is easier than counting the doors."
    - else:
        # mood:clinical_warm_wrong
        Kieran: "Better than I look. You know me — I always looked worse than I felt. Worried people for nothing."

        He smiles. It is warm and completely impossible.
    }

    -> corridor_continue

* [Stop walking. Refuse to continue.]
    ~ choice_3_corridor = "resist"
    ~ emotional_posture = emotional_posture - 1
    ~ trust_in_system = trust_in_system - 1
    ~ mystery_awareness = mystery_awareness + 1

    You stop. Your shoes squeak against the linoleum. The sound echoes down the corridor in both directions.

    "No."

    {condition == "A":
        Kieran stops too. He turns to look at you, and for a fraction of a second his expression is not his expression. It is something assembled. Something that has been told what concern looks like but has never felt it.

        Kieran: "We're almost there."

        "I don't care. This isn't right. You're not supposed to be here. You're supposed to be in the room."

        Kieran: "Where would you like me to be?"

        The question is wrong. Not the words — the framing. It implies options. It implies that his location is a parameter that can be adjusted. People don't ask where you'd like them to be. Systems do.
    - else:
        # mood:clinical_harsh
        Kieran stops too. He turns to look at you.

        Kieran: "We're almost there."

        "I don't care. This isn't right."

        Kieran: "Where would you like me to be?"
    }

    -> corridor_continue

* ["Are you real?"]
    ~ choice_3_corridor = "question"
    ~ emotional_posture = emotional_posture + 0
    ~ trust_in_system = trust_in_system - 2
    ~ mystery_awareness = mystery_awareness + 2

    The question comes out quietly. Almost a whisper. As though speaking it loudly would break something you are not sure you want broken.

    {condition == "A":
        Kieran looks at you for a long moment. His face does something complicated — a sequence of micro-expressions that add up to something you cannot name.

        Kieran: "Define real."

        "You know what I mean."

        Kieran: "I know what you're asking. But I think the question you actually want answered is different. I think you want to know whether what you're feeling right now is real. And that's a question I can't answer for you."

        Your thoughts: "That's not Kieran. Kieran would have said something funny. He would have deflected with a joke about philosophy or pretended to check his own pulse. This is something else. Something that has studied him very carefully."
    - else:
        # mood:clinical_glitch
        Kieran looks at you for a long moment.

        Kieran: "Define real."

        "You know what I mean."

        Kieran: "I know what you're asking. But I think the question you actually want answered is different."
    }

    -> corridor_continue

= corridor_continue

# mood:clinical_dissolve

The corridor ends. Room 14 is here. The door is closed. The number is printed in a font that tries to be neutral but manages only to be institutional.

{condition == "A":
    {mystery_awareness >= 3:
        You look at Kieran. He looks at the door. Neither of you speaks. You both know — you know and whatever he is knows — that what is behind this door is not what was there in the real memory. The rules have changed.
    - else:
        You look at Kieran. He looks at the door. The corridor is very quiet.
    }
- else:
    # mood:clinical_threshold
    You look at Kieran. He looks at the door. The corridor is very quiet.
}

ELARA: "Third memory environment concluding. Transitioning to final environment."

The door to room 14 opens. But behind it is not a hospital room.

# mood:transition_to_fractured

Behind it is something else entirely.

-> scene4_room
