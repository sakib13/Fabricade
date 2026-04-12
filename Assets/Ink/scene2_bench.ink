// ============================================
// SCENE 2: THE PARK BENCH — Memory 2 (Vulnerability)
// ============================================

=== scene2_bench ===
~ current_scene = "bench"
# scene:bench
# mood:autumn_cool

{condition == "A":
    {openness > deflection && openness > resistance:
        Autumn. The park is golden. The light finds you gently.
    - else:
        {resistance > openness && resistance > deflection:
            Autumn. Sharp air. The trees look staged.
        - else:
            {deflection > openness && deflection > resistance:
                Autumn. The light feels indirect, like it's avoiding you too.
            - else:
                Autumn. Djurgården. October light. Copper trees.
            }
        }
    }
- else:
    Autumn. Djurgården. October light. Copper trees.
}

Kieran beside you. Jacket too thin.

{miss_most == "humor":
    Kieran: "I've been thinking about disappearing. Not dying. Just... becoming a really good memory. The kind people smile at."
- else:
    {miss_most == "honesty":
        Kieran: "I've been thinking about disappearing. I need to say that out loud to someone. You're the only person I'd say it to."
    - else:
        Kieran: "I've been thinking about disappearing. Not dying. Just becoming the version of me that exists in other people's memories."
    }
}

-> bench_choice

= bench_choice

* [Match his vulnerability.]
    ~ choice_2_bench = "open"
    ~ emotional_posture = emotional_posture + 2
    ~ openness = openness + 1
    ~ trust_in_system = trust_in_system - 1

    "I carry you around already. The version isn't accurate, but it gets me through the day."

    Kieran: "Then it's working."

    {condition == "A":
        {openness >= 2:
            ELARA: "Vulnerability again. The system is updating its model of you."
        - else:
            ELARA: "You chose vulnerability. Noted."
        }
    - else:
        # mood:autumn_warm_ache
    }

    -> bench_transition

* [Comfort him. Redirect.]
    ~ choice_2_bench = "comfort"
    ~ deflection = deflection + 1

    "You're not disappearing. You're right here."

    Kieran: "Right. Here."

    {condition == "A":
        {deflection >= 2:
            ELARA: "Comfort instead of honesty. A pattern is forming."
        - else:
            ELARA: "Comfort instead of honesty. The system notes the distinction."
        }
    - else:
        # mood:autumn_bright_forced
    }

    -> bench_transition

* [Stay quiet.]
    ~ choice_2_bench = "quiet"
    ~ emotional_posture = emotional_posture - 1

    The silence holds.

    Kieran: "Yeah. That's what I thought."

    {condition == "A":
        ELARA: "Silence. The system cannot determine whether this is restraint or avoidance."
    - else:
        # mood:autumn_sparse
    }

    -> bench_transition

= bench_transition

# mood:autumn_heavy

Kieran: "When I'm gone. Whatever version of me you carry, it's yours."

~ mystery_awareness = mystery_awareness + 1

{condition == "A":
    He didn't say this. This is ELARA, filling gaps with something that sounds right.
- else:
    # mood:autumn_dissonant
    You're almost certain he never said this.
}

A leaf detaches from a branch. The timing feels deliberate.

* [Continue]

-> bench_close

= bench_close

# mood:transition_to_cold

The park dissolves.

ELARA: "Second environment concluding."

-> scene3_corridor
