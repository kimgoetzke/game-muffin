# Muffin

Muffin is a top-down, 3D pixel art, action RPG using 2D sprites, developed in C#/Unity. It was created in 2022 as a project to improve my skills in C#, Unity Engine, shaders, render pipeline management, simple AI design, and many other topics. The game is a pre-prototype stage. It is playable but is not feature complete and has no content. It was put on hold when I started focussing on Java backend development.

_This repo only contains the C# scripts and no other files or assets related to the game. Plastic SCM was used for full version control for this project as it natively integrates with Unity._

![Muffin](https://user-images.githubusercontent.com/120580433/216383698-a3a3d70a-f862-4b9d-8830-1b55c7f3cd4a.png)

### Technical highlights
+ Unified, hierarchical state machine AI model that powers friends and foes alike and allows them to interact with one another
+ Faction management that allows changing NPC behaviours real-time (towards player and other NPCs)
+ (Behavioural tree model was created but overkill)
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
