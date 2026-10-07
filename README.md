
# Overview
This project implements an interactive water simulation in Unity using a procedurally generated grid mesh, a custom water shader, four superposed Gerstner-style waves, a floating object that samples the same wave field on the CPU, and a user-controllable orbit camera. The goal was to preserve the main graphics ideas from the original OpenGL capstone while translating them into Unity systems such as C# mesh generation, ShaderLab/HLSL, scene setup, and runtime control.

The water is not a Unity plane or premade asset. The surface is generated at runtime in C#, displaced in the shader, lit using a custom water material, and animated over time. The floating object uses the same wave parameters as the shader so that it rises, falls, and tilts with the surface instead of following an unrelated sine curve.

# Files Included
- GridMeshGenerator.cs
Generates the dense water mesh procedurally at runtime.

- WaterController.cs
Stores wave parameters and sends them to the shader every frame.

- FloatingObjectController.cs
Samples the same wave field on the CPU so the object follows the water height and surface tilt.

- CameraController.cs
Implements a user-controllable orbit camera.

- WaterShader.shader
Custom ShaderLab/HLSL shader for wave displacement, normals, crest highlights, fresnel response, and water colouring.

- MainScene.unity
Main demonstration scene.

- WaterMaterial
Material using the custom shader.

# Build / Run Instructions
Open the project in Unity 2022 LTS or newer
Open the main scene
Make sure the project uses the Build-In Render Pipeline
If camera input does not work, go to:
    Edit -> Project Settings -> Player -> Other Settings
    Set Active Input Handling to Both or Input Manager (Old)
Press Play in Unity to run in either Scene or Game

# Control Scheme
- Camera mode is selected in the CameraController inspector:
    Orbit or FreeFly

- Orbit mode:
    Hold Right Mouse Button: rotate camera
    Mouse Scroll Wheel: zoom in and out

- Free-fly mode:
    Right Mouse Button (optional, based on inspector setting): look around
    W / A / S / D: move forward / left / backward / right
    Q / E: move down / up
    Left Shift: move faster

This satisfies the user-controllable camera requirement by allowing interactive scene inspection from multiple viewpoints.

# Design Choices

- Procedural Mesh Generation
I used a GridMeshGenerator component to create the water surface as a dense regular grid over the x-z plane at runtime. I exposed grid resolution, world size, and UV-related control through script variables so the mesh could be adjusted without remodeling anything by hand. This directly matches the requirement that the water mesh be generated procedurally in C# rather than authored in a modelling package.

- Wave Displacement
The water surface is animated with four Gerstner-style waves. Each wave has its own:
    direction
    amplitude
    frequency
    speed
    steepness
The shader applies both vertical and horizontal displacement, not just vertical sine motion. This was important because the assignment specifically requires Gerstner-style motion and warns against using only simple up-and-down animation.

- Surface Normals
Normals are reconstructed from neighboring displaced sample points in the vertex stage. I sample nearby displaced positions, compute tangent directions, and calculate the normal from their cross product. I used this method instead of leaving the mesh normals flat, because the assignment requires normals to respond to the displaced surface.

- Water Appearance
The shader uses:
    shallow/deep colour blending
    crest highlights
    fresnel-based grazing tint
    smoothness/specular response
This was done to make the water look more like a reflective surface instead of a flat colored mesh. I also experimented with additional detail layers and texture-based approaches while trying to make the surface look more convincing.

- Floating Object
The floating object samples the same wave field on the CPU that the shader uses on the GPU. This keeps the object synchronized with the water instead of making it follow an unrelated sine function. The object also tilts based on sampled height differences so it responds to surface orientation instead of only bobbing straight up and down.

- Camera
I used an orbit/globe camera instead of the third-person view because it made it easier to inspect the water, crest motion, and floating object from multiple viewpoints

# C and D (Core Requirement Explanation)

- C. Detail enrichment (secondary visible layer)
Beyond the four main Gerstner-style waves, I added a secondary animated detail layer in the shader so the surface has finer motion on top of the primary swell. I implemented this by combining two moving procedural terms (`detailA` and `detailB`) in `GetDisplacedPosition`, blending them into `detailWave`, and adding a small vertical perturbation with `_DetailAmplitude`. I also carry that same detail signal into shading (detail tint/specular/gloss influence) so the detail is visibly moving and remains consistent with the displaced surface motion instead of looking static.

- D. Surface normals and lighting
Normals are not left as flat mesh normals. They are reconstructed from displaced positions in the vertex stage by sampling neighboring displaced points, building tangent vectors, and computing the cross product normal. This makes lighting follow the actual wave geometry. Lighting is handled with a custom lighting model (`LightingWater`) that explicitly computes ambient, diffuse, and specular terms. The surface stage provides `Albedo`, `Specular`, `Gloss`, and `Alpha`, and the custom lighting function combines these with light direction/view direction to produce the final response.

# OpenGL to Unity Mapping
The original OpenGL version of this assignment focused on dense surface generation, shader-based displacement, normals, lighting, and a movable camera. In Unity, I mapped those ideas as follows:
- Dense tessellated / generated surface in OpenGL
became procedural grid mesh generation in C# using GridMeshGenerator.

- Vertex-stage displacement in OpenGL shaders
became vertex displacement in a custom Unity water shader.

- Wave logic from the OpenGL water surface
became four Gerstner-style wave layers controlled by WaterController.

- Geometry/tessellation-stage surface detail idea
was approximated by using a sufficiently dense procedural grid and shader displacement, since Unity Built-In in this project did not use a geometry-shader-first pipeline.

- Explicit geometry-shader-first adaptation note
The original pipeline assumption of geometry-stage-first perturbation was adapted in Unity by generating a dense runtime mesh in C# and performing displacement in the custom vertex stage (`vert`). This preserves the intended effect (high-detail deforming surface) without requiring a geometry shader stage in the Built-In pipeline configuration used by this project.

- Normal reconstruction in OpenGL
became finite-difference-style normal reconstruction using nearby displaced samples.

- Phong-like fragment lighting in OpenGL
became custom water shading using colour blending, fresnel response, crest highlights, and smoothness/specular control in ShaderLab/HLSL.

- CPU/GPU synchronization problem
was handled by implementing matching wave sampling in FloatingObjectController so the floating object tracks the same water function used by the shader.

- Movable camera in OpenGL
became an orbit camera in Unity using CameraController.

This preserves the main graphics ideas of the original assignment while adapting them to Unity’s mesh, material, and scene workflow

# Known Bugs / Problems Encountered
- Pink Water Surface
At an early stage the water showed up as bright pink. This happened because the mesh was rendering without a valid material/shader assigned. I fixed it by creating a water material and assigning the custom shader correctly

- Camera Input Errors
The camera script originally produced input errors because the project was using the new Input System only, while the camera controller was written with Input.GetKey() and Input.GetAxis(). I fixed this by changing Active Input Handling in Player Settings to Both.

- Floating Object Looked Like It Was Only Touching the Water
The floating object was following the wave height, but visually it looked like it was barely resting on top of the surface. I fixed this by lowering the height offset so the object sat more naturally in the water.

- Lighting / Plastic-Looking Water
A major issue during development was that the water often looked like plastic instead of water. Previous versions had little lighting and didn't have deep and shallow colours. I experimented with colour, gloss, metallic values, fresnel response, crest shading, and different ways of computing normals. Some attempts improved highlights but created artifacts, and other attempts looked too flat. The final version uses reconstructed normals plus fresnel and crest response, which gave a better overall result.

- Rough Pixelated / Faceted Surface
When the normal method and mesh density were changed, the water sometimes looked faceted or overly pixelated. Increasing mesh density helped somewhat, but some normal methods caused visual artifacts at high density. I settled on a denser procedural mesh with a more stable normal reconstruction approach.

- Bad Detail-Layer Experiments
I tested several approaches for the secondary detail layer:
    procedural ripple patterns that looked like checkerboards
    high-frequency detail that looked like bubbles
    grayscale noise textures that looked like static
    water textures that looked pasted directly on top of the surface
These were useful experiments, but many of them did not look natural. I kept the approaches that improved the look and removed ones that made the water look noisy, tiled, or fake.

- Dense Grid / Shader Artifact Issue
When the segment count was pushed too high, the shading became unstable and produced broken-looking patches. I reduced the mesh back to a more stable density and kept the surface detailed enough for the assignment while avoiding those artifacts.

# Work Log
Below is the chronological order of how I did the assignment

# Initial Setup
- Created a new Unity project using the Built-In Render Pipeline.
- Created the folder structure for scenes, scripts, shaders, materials, textures, and prefabs.
- Saved a clean main scene.
- Added the basic scene objects: main camera, directional light, water root, and floating object.

# Procedural Water Mesh
- Added MeshFilter and MeshRenderer to WaterRoot.
- Created GridMeshGenerator.cs.
- Implemented a dense runtime-generated grid mesh over the x-z plane.
- Tested the mesh in Play mode and confirmed it rendered properly.

# First Shader Setup
- Created WaterController.cs.
- Created WaterShader.shader.
- Connected the material and controller.
- Confirmed the custom shader was rendering the water instead of using a default Unity material.

# Wave Animation
- Began with a very simple animated sine wave in the vertex stage.
- Extended that to four wave layers.
- Replaced simple vertical motion with Gerstner-style horizontal and vertical displacement.
- Moved wave parameters into WaterController so the shader was driven from C# instead of hardcoding everything.

# Floating Object
- Created FloatingObjectController.cs.
- Implemented CPU-side wave sampling using the same wave settings as the shader.
- Made the floating object rise and fall with the water.
- Added tilt/orientation response by sampling nearby heights and constructing a surface normal.
- Adjusted the object’s height offset so it looked more naturally placed in the water.

# Normals and Lighting
- Tried multiple methods to make lighting respond to the displaced water surface.
- Tested simple tilted normals, derivative-based approaches, and finally a more stable finite-difference style reconstruction using displaced sample points.
- Tuned water color, gloss, metallic, fresnel, and crest response to improve the appearance.

# Detail Layer Experiments
- Compared the project visually against other examples.
- Tested different ways to add finer surface detail:
    procedural ripples
    noise-like patterns
    grayscale textures
    visible water textures
- Removed approaches that created checkerboard patterns, bubbles, static, or pasted-photo artifacts.
- Kept the approaches that gave useful visual breakup without ruining the main wave motion.

# Camera
- Created CameraController.cs.
- Implemented orbit-style controls with arrow keys and mouse scroll zoom.
- Fixed input errors by changing Unity’s active input handling settings.

# Final Scene State
- Saved a working version of the project with:
    procedural water mesh
    four Gerstner-style waves
    responsive normals
    floating object following the surface
    orbit camera
    custom water shading

# Time Spent / Hardest Parts
The hardest parts were:
    making the water look like water instead of plastic
    finding a stable normal reconstruction method
    getting the floating object to feel synchronized with the surface
    experimenting with detail layers that improved the look instead of making it worse
    considered using a grayscale texture for foam, but later decided against it because the visual result looked unnatural
The implementation itself was straightforward in structure, but the visual tuning took the most time because small shader changes had a big impact on the final appearance.
