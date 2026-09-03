using UnityEngine;
using UnityEngine.UI;

namespace Nobi.UiRoundedCorners {
	//Shared plumbing for the two rounded-corner components. Both hand their material to the
	//Graphic through IMaterialModifier instead of assigning Graphic.material, so nothing they
	//create is ever written into a scene or a prefab.
	internal static class RoundedCornersMaterial {
		//The material lives in memory only: it is rebuilt whenever the component is enabled, so a
		//serialized copy is dead weight that Unity rewrites with a fresh fileID on every save,
		//churning the scene file and leaving orphaned Materials behind it.
		private const HideFlags RuntimeMaterialFlags =
			HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

		//Mask support properties, declared by both rounded-corner shaders. MaskableGraphic writes
		//these onto the material it returns from its own GetModifiedMaterial, so they have to be
		//carried across when the rounded-corners material replaces it - otherwise a parent Mask
		//silently stops clipping this Graphic.
		private static readonly int[] MaskProps = {
			Shader.PropertyToID("_StencilComp"),
			Shader.PropertyToID("_Stencil"),
			Shader.PropertyToID("_StencilOp"),
			Shader.PropertyToID("_StencilWriteMask"),
			Shader.PropertyToID("_StencilReadMask"),
			Shader.PropertyToID("_ColorMask"),
			Shader.PropertyToID("_UseUIAlphaClip"),
		};

		internal static Material Create(string shaderName) {
			Shader shader = Shader.Find(shaderName);
			if (shader == null) {
				Debug.LogError($"[UiRoundedCorners] Shader not found: {shaderName}");
				return null;
			}

			return new Material(shader) { hideFlags = RuntimeMaterialFlags };
		}

		internal static void CopyMaskState(Material from, Material to) {
			if (from == null || to == null || from == to) {
				return;
			}

			for (int i = 0; i < MaskProps.Length; i++) {
				int prop = MaskProps[i];
				if (from.HasFloat(prop) && to.HasFloat(prop)) {
					to.SetFloat(prop, from.GetFloat(prop));
				}
			}
		}

		internal static void Release(MaskableGraphic image, ref Material material) {
			if (image != null) {
				image.SetMaterialDirty();
			}

			DestroyHelper.Destroy(material);
			material = null;
		}
	}
}
