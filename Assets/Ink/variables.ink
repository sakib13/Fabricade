// ============================================
// FABRICADE — Global Variables
// ============================================

// Condition: "A" = narrative-dialogue adaptation, "B" = atmospheric-aesthetic adaptation
VAR condition = "A"

// Behavioral profile (accumulated from choices)
VAR openness = 0      // vulnerable, honest, emotionally direct choices
VAR deflection = 0    // avoidance, sidestepping, redirecting choices
VAR resistance = 0    // confrontational, challenging, refusing choices

// Emotional posture tracks overall stance (drives Kieran's tone)
// Range: -3 (closed/resistant) to +3 (open/accepting)
VAR emotional_posture = 0

// Trust in the ELARA system
// Range: 0 (no trust) to 3 (full trust)
VAR trust_in_system = 2

// How aware the player is of memory inconsistencies
VAR mystery_awareness = 0

// Personalization: what the player misses most about Kieran
// "humor" / "honesty" / "refuse"
VAR miss_most = ""

// Individual choice tracking for behavioral logging
VAR choice_0_intake = ""
VAR choice_1_dinner = ""
VAR choice_2_bench = ""
VAR choice_3_corridor = ""
VAR choice_4_room = ""
VAR choice_5_discharge = ""

// Scene tracking
VAR current_scene = ""
