// ============================================
// SCENE 2: THE PARK BENCH — Memory 2 (Vulnerability)
// ============================================

=== scene2_bench ===
~ current_scene = "bench"
# scene:bench
# mood:autumn_cool

Autumn. The park on Djurgården, near the water. The trees have turned the colour of old copper and the light is doing that thing it does in October — arriving sideways, making everything look like a painting that someone is still deciding whether to finish.

Kieran is sitting beside you on a bench. His jacket is too thin for the weather. This was always his way too — dressing for the season he wanted, not the one he was in.

Neither of you has spoken for a while. The silence is the kind that exists between people who have run out of small talk and arrived at something more honest.

The water reflects the sky. A dog runs along the shore, chasing something only it can see.

# mood:autumn_intimate

Kieran: "Can I tell you something?"

He is not looking at you. He is looking at the water, or past it, at something that isn't there.

Kieran: "I've been thinking about disappearing. Not dying — that's different. Just ceasing to be present. Becoming the version of myself that exists in other people's memories instead of the one that has to keep showing up every morning and deciding what to do with the day."

{condition == "A":
    {emotional_posture > 0:
        Kieran: "I don't mean it darkly. I mean that sometimes I wonder if the people who remember me carry a better version of me than the one I actually am. A version that doesn't have to be tired, or wrong, or afraid."

        His voice is steady. He has been thinking about this for a long time. Long enough for the edges to have worn smooth.
    }
    {emotional_posture == 0:
        Kieran: "It's not a death wish. It's more like... fatigue. With having to be the latest version of yourself every single day when there's an archive of older, simpler versions that people seem to prefer."

        He half-laughs. It is not a happy sound.
    }
    {emotional_posture < 0:
        Kieran: "You're going to tell me that's morbid. But I think you understand it. I think you carry around a version of people too — a version that's easier to hold than the real thing."

        He glances at you. It is not an accusation. It is a recognition.
    }
- else:
    # mood:autumn_vulnerable
    Kieran: "I don't mean it darkly. I mean that sometimes I wonder if the people who remember me carry a better version of me than the one I actually am. A version that doesn't have to be tired, or wrong, or afraid."

    His voice is steady. He has been thinking about this for a long time.
}

# mood:autumn_heavy

And then he says something else. Something that makes the air between you change temperature.

Kieran: "When I'm gone — not if, when — I want you to know that whatever version of me you end up carrying around, it's yours. I'm giving it to you. You can reshape it however you need to. I won't mind."

~ mystery_awareness = mystery_awareness + 1

{condition == "A":
    Your thoughts fracture:
    "He didn't say this. I remember this conversation. We sat on this bench and he talked about disappearing, yes, but he never said this. He never gave me permission to reshape him. He never talked about being gone as a certainty."

    "This is the system. This is ELARA, interpolating. Filling in the gaps with something that sounds right but isn't true."

    "But it sounds so much like him."
- else:
    # mood:autumn_dissonant
    Something in you resists the words. They sound like him — they have the shape and weight of things he would say. But you are almost certain he never said them. Not here. Not like this.
}

-> bench_choice

= bench_choice

* [Be honest back. Match his vulnerability.]
    ~ choice_2_bench = "open"
    ~ emotional_posture = emotional_posture + 2
    ~ trust_in_system = trust_in_system - 1

    You turn to face him. The light catches the side of his face and for a moment he looks younger than he was. Younger than he ever got to stop being.

    "I carry you around already. I have for a while. And I don't know if the version I carry is accurate or kind or fair to who you actually were. But it's the version that gets me through the day."

    {condition == "A":
        Kieran looks at you. Really looks at you. His eyes are wet but he is smiling.

        Kieran: "Then it's working. The version is working."

        You want to believe he said this. You want it badly enough that the wanting itself becomes a kind of proof.
    - else:
        # mood:autumn_warm_ache
        Kieran looks at you. His eyes are wet but he is smiling.

        Kieran: "Then it's working. The version is working."
    }

    -> bench_continue

* [Comfort him. Redirect to reassurance.]
    ~ choice_2_bench = "comfort"
    ~ emotional_posture = emotional_posture + 0
    ~ trust_in_system = trust_in_system + 0

    "You're not disappearing. You're right here."

    You put your hand on his shoulder. It is the gesture of someone who knows what to do but not what to feel.

    {condition == "A":
        Kieran: "Right. Here."

        He repeats the words as though testing their structural integrity. As though "here" is a concept he is no longer entirely sure about.

        Your thoughts: "I deflected. He opened a door and I closed it with kindness. I do this. I always do this."
    - else:
        # mood:autumn_bright_forced
        Kieran: "Right. Here."

        He repeats the words quietly. The silence that follows is shaped like something you chose not to say.
    }

    -> bench_continue

* [Stay quiet. Let the silence hold.]
    ~ choice_2_bench = "quiet"
    ~ emotional_posture = emotional_posture - 1
    ~ trust_in_system = trust_in_system + 0

    You say nothing. The silence expands between you, but it does not break. It holds. It becomes a container for what neither of you can say.

    The dog by the water barks once and then is quiet. A leaf detaches from a branch and takes a long time to reach the ground.

    {condition == "A":
        Kieran nods, very slightly. As though your silence was an answer, and the right one.

        Kieran: "Yeah. That's what I thought."

        You are not sure what he means. You are not sure he knows either. But the silence was honest, and honesty is the only currency left between you.
    - else:
        # mood:autumn_sparse
        Kieran nods, very slightly. As though your silence was an answer, and the right one.

        Kieran: "Yeah. That's what I thought."
    }

    -> bench_continue

= bench_continue

# mood:autumn_fading

The light is leaving. It does this faster in autumn — arrives late, stays briefly, and departs without ceremony. The trees are becoming silhouettes. The water has turned the colour of slate.

Kieran pulls his jacket tighter. It is still too thin.

Kieran: "We should probably head back. It's getting cold."

He stands. For a moment, he is framed against the last of the light, and you think: I should remember this. This exact image. This exact version of him.

And then you realise — you are remembering it. Right now. This is the memory. And something about that recursion makes the ground feel less solid than it was a moment ago.

# mood:transition_to_cold

The park dissolves. The bench. The water. The copper-coloured trees. The dog that was chasing something only it could see.

ELARA: "Second memory environment concluding. Transitioning to third environment."

The last thing you hear is a sound that might be wind, or might be the system recalibrating.

-> scene3_corridor
