#if FLAX_EDITOR
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace Game.CustomEditors
{
    [CustomEditor(typeof(JumpTrajectoryPreview))]
    public class JumpTrajectoryPreviewCustomEditor : GenericEditor
    {
        public override void Initialize(LayoutElementsContainer layout)
        {
            base.Initialize(layout);

            layout.Space(20);
            var button = layout.Button("New on the edge", Color.DarkGray);
            button.Button.Clicked += () => (Values[0] as JumpTrajectoryPreview)?.CreateNewOnTheEdge();
        }
    }
}
#endif
