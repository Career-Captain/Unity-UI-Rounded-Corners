using UnityEngine;
using UnityEngine.UI;

namespace Nobi.UiRoundedCorners {
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class ImageWithRoundedCorners : MonoBehaviour, IMaterialModifier {
		internal const string ShaderName = "UI/RoundedCorners/RoundedCorners";

		private static readonly int Props = Shader.PropertyToID("_WidthHeightRadius");
		private static readonly int prop_OuterUV = Shader.PropertyToID("_OuterUV");

		public float radius = 40f;
		private Material material;
		private Vector4 outerUV = new Vector4(0, 0, 1, 1);

		[HideInInspector, SerializeField] private MaskableGraphic image;

		private void OnValidate() {
			Validate();
			Refresh();
			SetMaterialDirty();
		}

		private void OnDestroy() {
			//Nothing was ever written to the Graphic, so removing this component only has to
			//drop the material and ask the Graphic to resolve its own again.
			RoundedCornersMaterial.Release(image, ref material);
			image = null;
		}

		private void OnEnable() {
			Validate();
			Refresh();
			SetMaterialDirty();
		}

		//You can only add either ImageWithRoundedCorners or ImageWithIndependentRoundedCorners
		//It will replace the other component when added into the object. This sits in Reset and
		//not OnEnable because under [ExecuteAlways] OnEnable also runs inside a Prefab Mode stage
		//while the editor is playing, where destroying the sibling would edit the open Prefab.
		//Consequence: a runtime AddComponent no longer replaces the other variant.
		private void Reset() {
			var other = GetComponent<ImageWithIndependentRoundedCorners>();
			if (other != null) {
				radius = other.r.x;                 //When it does, transfer the radius value to this script
				DestroyHelper.Destroy(other);
			}

			Validate();
			Refresh();
			SetMaterialDirty();
		}

		private void OnDisable() {
			//A disabled corner rounder stops contributing its material, so the Graphic has to be
			//told to resolve the unrounded one again.
			SetMaterialDirty();
		}

		private void OnRectTransformDimensionsChange() {
			if (enabled && material != null) {
				Refresh();
			}
		}

		public void Validate() {
			if (material == null) {
				material = RoundedCornersMaterial.Create(ShaderName);
			}

			if (image == null) {
				TryGetComponent(out image);
			}

			if (image is Image uiImage && uiImage.sprite != null) {
				outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(uiImage.sprite);
			}
		}

		//Hands the material to the Graphic at render time instead of assigning
		//Graphic.material. m_Material is a serialized field, so assigning it registered an
		//m_Material prefab override on every instance - one that reappeared immediately after
		//Revert, because OnValidate wrote it straight back. Nothing reaches serialized state
		//through this path.
		public Material GetModifiedMaterial(Material baseMaterial) {
			if (!enabled) {
				return baseMaterial;
			}

			if (material == null) {
				material = RoundedCornersMaterial.Create(ShaderName);
				if (material == null) {
					return baseMaterial;
				}

				Refresh();
			}

			//MaskableGraphic is an IMaterialModifier on this same GameObject and generally runs
			//first, so baseMaterial already carries the stencil state a parent Mask depends on.
			RoundedCornersMaterial.CopyMaskState(baseMaterial, material);
			return material;
		}

		private void SetMaterialDirty() {
			if (image != null) {
				image.SetMaterialDirty();
			}
		}

		public void Refresh() {
			if (material == null) {
				return;
			}

			var rect = ((RectTransform)transform).rect;

			//Multiply radius value by 2 to make the radius value appear consistent with ImageWithIndependentRoundedCorners script.
			//Right now, the ImageWithIndependentRoundedCorners appears to have double the radius than this.
			material.SetVector(Props, new Vector4(rect.width, rect.height, radius * 2, 0));
			material.SetVector(prop_OuterUV, outerUV);
		}
	}
}