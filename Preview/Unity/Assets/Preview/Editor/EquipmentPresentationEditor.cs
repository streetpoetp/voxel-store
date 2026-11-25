using UnityEditor;
using VoxelStore.Preview.Runtime;

namespace VoxelStore.Preview.Editor
{
    [CustomEditor(typeof(EquipmentPresentation))]
    public sealed class EquipmentPresentationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
