# Muffin

Muffin is a top-down, 3D pixel art, action RPG using 2D sprites, developed in C#/Unity. It was created in 2022 as a project to improve my skills in C#, Unity Engine, shaders, render pipeline management, simple AI design, and many other topics. The game is a pre-prototype stage. It is playable but is not feature complete and has no content. It was put on hold when I started focussing on Java backend development.

_This repo only contains the C# scripts and no other files or assets related to the game. Plastic SCM was used for full version control for this project as it natively integrates with Unity._

![Muffin](https://user-images.githubusercontent.com/120580433/216383698-a3a3d70a-f862-4b9d-8830-1b55c7f3cd4a.png)

### Technical features
+ Unified, hierarchical state machine AI model that powers friends and foes alike and allows them to interact with one another
+ Faction management that allows changing NPC behaviours real-time (towards player and other NPCs)
+ (Behavioural tree model for AI in repo but not used as it was not required)
+ Swappable equipment in real-time using scriptable objects
+ 4 directional player sprite movement while maintaining full 360 degrees aim
+ Object pools to pre-spawn a number of frequently spawned objects
+ Universal Render Pipeline post-processing features, incl. bloom, depth of field, color grading and more
+ 2D dynamic lightning (e.g. zone based), cloud shadows, etc.
+ Extensive use of shaders to simulate water, fog, sprite lightning, and more
+ Basic 3D modelling of rocks, crates and other objects
+ Cinemachine with camera zones and target groups to enable look-ahead
+ Scalable quest and dialogue system (allowing for 4 quest types) incl. UI (using Inky for dialogue scripts and variables)
+ Unity Terrain and GPU instancing for grass/flower sprites


![Muffin banner](https://user-images.githubusercontent.com/120580433/216386856-19505ac1-d8bb-4381-a31c-9745ab74dbd5.png)

#### Quest system

+ A quest is a collection of tasks which can be linked to a story 
+ Stories are text assets which are arranged into a flow using the third party tool Ink 
+ C# classes share data with Ink through `DialogueVariables`, allowing dialogue to influence game behaviours and vice versa
+ The system supports any task types such as "reach location x", "defeat x number of type y enemies", or "do x" 
+ The active quest, it's task and their states are shown on the in-game UI to the player
+ Quest states and dialogue variables (which include story progression) are, by default, saved on scene change

![Quest system](Assets\Settings\Readme\Quest_system.png)

![Flying through space](Assets\Settings\Readme\Flying_through_space.gif)

#### NPC state machine

+ `NPC` state machine is at the core
+ `NpcBehaviour` sets default behavior: anxious, neutral, indifferent, observant, defensive, or aggressive
+ `NpcMovement` sets default movement type: wander, patrol or idle
+ Combination of `AnimationController` and `AudioController` allow customising the visual appear and audio (humanoid and two-legged dinsaur implemented)
+ Separate, optional Game Objects with sphere colliders determine:
    1. Awareness: trigger intermediate action such as observing
    2. Action: switch from default movement to default behaviour
    3. Collaboration: support others of same faction or flee together
    4. Interaction: allow player to interact e.g. trigger dialogue
+ `FactionIdentity` component determines which other factions the NPC is hostile, friendly or neutral towards
+ Factions can change at runtime
+ `HealthManager` determines health and triggers disabling of NPC class
+ `AttackController` determines damage, range, etc.
