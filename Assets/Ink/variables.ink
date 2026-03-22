// ============================================
// FABRICADE — Global Variables
// ============================================

// Condition: "A" = narrative-dialogue adaptation, "B" = atmospheric-aesthetic adaptation
VAR condition = "A"

// Emotional posture tracks the player's overall emotional stance
// Range: -3 (closed/resistant) to +3 (open/accepting)
VAR emotional_posture = 0

// Trust in the ELARA system
// Range: 0 (no trust) to 3 (full trust)
VAR trust_in_system = 2

// How aware the player is of the memory inconsistencies
// Range: 0 (unaware) to 4 (fully aware)
VAR mystery_awareness = 0

// Individual choice tracking for behavioral logging
VAR choice_0_intake = ""
VAR choice_1_dinner = ""
VAR choice_2_bench = ""
VAR choice_3_corridor = ""
VAR choice_4_room = ""
VAR choice_5_discharge = ""

// Scene tracking
VAR current_scene = ""
