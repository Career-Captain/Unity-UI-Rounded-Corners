using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Nobi.UiRoundedCorners.Editor {
    [CustomEditor(typeof(ImageWithRoundedCorners)), CanEditMultipleObjects]
    public class ImageWithRoundedCornersInspector : UnityEditor.Editor {
        private ImageWithRoundedCorners script;

        private void OnEnable() {
            script = (ImageWithRoundedCorners)target;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "radius");

            //The radius row shows what is actually drawn: Max Radius plus the clamped result, or the computed value when Fully Round ignores it.
            var radiusProp = serializedObject.FindProperty("radius");
            var limitProp = serializedObject.FindProperty("radiusLimit");
            var limit = limitProp.hasMultipleDifferentValues ? ImageWithRoundedCorners.RadiusLimit.None : (ImageWithRoundedCorners.RadiusLimit)limitProp.enumValueIndex;

            if (limit == ImageWithRoundedCorners.RadiusLimit.FullyRound) {
                EffectiveRadiusField("Radius");
            } else if (limit == ImageWithRoundedCorners.RadiusLimit.ClampToSize) {
                EditorGUILayout.PropertyField(radiusProp, new GUIContent("Max Radius"));
                EffectiveRadiusField("Effective Radius");
            } else {
                EditorGUILayout.PropertyField(radiusProp);
            }

            serializedObject.ApplyModifiedProperties();

            if (!script.TryGetComponent<MaskableGraphic>(out var _)) {
                EditorGUILayout.HelpBox("This script requires an MaskableGraphic (Image or RawImage) component on the same gameobject", MessageType.Warning);
            }
        }

        private void EffectiveRadiusField(string label) {
            var value = script.EffectiveRadius;
            var mixed = false;
            foreach (var t in targets) {
                mixed |= !Mathf.Approximately(((ImageWithRoundedCorners)t).EffectiveRadius, value);
            }

            using (new EditorGUI.DisabledScope(true)) {
                EditorGUI.showMixedValue = mixed;
                EditorGUILayout.FloatField(label, value);
                EditorGUI.showMixedValue = false;
            }
        }
    }
}
