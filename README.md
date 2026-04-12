# Fabricade

Fabricade is a text-based interactive fiction game built as a research instrument for a Master's thesis investigating player experience of emotionally adaptive game narratives. The game is developed in Unity using the Ink narrative scripting language.

## About

The player takes on the role of someone who has recently lost a close friend, Kieran. They enrol in a clinical programme run by ELARA (Emotional Landscape and Retrieval Architecture), an AI system that reconstructs shared memories as a form of grief therapy. Across six scenes, the player makes narrative choices that shape how the system responds.

### Scenes

1. **Intake** - Initial session with ELARA
2. **The Dinner** - A reconstructed memory of a shared meal
3. **The Park Bench** - A reflective moment in an autumn park
4. **The Hospital Corridor** - A disorienting clinical environment
5. **The Room** - The revelation scene
6. **Discharge** - Final session and resolution

### Two Conditions of Adaptation

The game implements two distinct modes of emotional adaptation for experimental comparison:

**Condition A: Narrative Dialogue Adaptation**
Dialogue spoken by Kieran and ELARA, and the player's internal monologue, adapt based on accumulated behavioural variables. Adaptation is expressed through *what* is said.

**Condition B: Atmospheric Aesthetic Adaptation**
All narrative text remains identical regardless of choices. Instead, the atmospheric presentation adapts through vignette and glow overlays that shift in intensity and warmth, text glitch effects at moments of narrative disruption, a screen glitch effect at a key scene threshold, and minimal procedural ELARA dialogue. Adaptation is expressed through *how* the text feels to read.

Both conditions share the same six scenes, choice points, and narrative backbone. Player choices accumulate into behavioural variables (openness, deflection, resistance, emotional posture, trust in the system) that drive each condition's adaptive responses.

## Tech Stack

- **Unity** (URP)
- **Ink** (narrative scripting via inkle's Ink-Unity integration)
- **TextMeshPro** (text rendering)
- **Share Tech Mono** (typeface)

## Project Structure

```
Assets/
  Ink/              # Narrative scripts (main.ink, variables.ink, scene0-5)
  Scripts/          # C# game logic
    AtmosphericController.cs   # Mood profiles, vignette/glow, transitions
    GlitchController.cs        # Screen and text glitch effects
    NarrativeManager.cs        # Ink runtime bridge
    UIManager.cs               # Text display, typewriter, choices
    BehavioralLogger.cs        # Session logging (JSON)
    NarrativeScroller.cs       # Scroll handling
  Audio/            # Ambient audio tracks
  Fonts/            # Share Tech Mono typeface
  Scenes/           # Unity scene
```

## How to Run

### From Unity Editor
1. Open the project in Unity (URP)
2. Open `Assets/Scenes/SampleScene.unity`
3. Press Play
4. Select Condition A or Condition B from the start screen

### From Build
1. Download the build for your platform
2. Run the executable
3. Select your condition and play

## Session Logging

The game automatically logs each session to `SessionLogs/` in JSON format, recording every choice made, timestamps, hesitation times, and condition assignment.

## Author

Sakib Ahsan Dipto
Master's in Design for Creative and Immersive Technology
Stockholm University, Department of Computer and Systems Sciences
