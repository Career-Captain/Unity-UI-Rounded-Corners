# CareerCaptain fork

Fork of [kirevdokimov/Unity-UI-Rounded-Corners](https://github.com/kirevdokimov/Unity-UI-Rounded-Corners) (MIT).

- The runtime material is no longer saved into the scene file.
- Handing it to the Graphic no longer creates an `m_Material` prefab override.
- `UiRoundedCorners/Resources/RoundedCornersShaders.shadervariants` keeps both shaders in player builds. Nothing references them from a saved material anymore, so without it they are stripped and `Shader.Find` fails. Do not delete.
