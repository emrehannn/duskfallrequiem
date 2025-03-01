// Copyright © Sascha Graeff/13Pixels.

#if UNITY_2021_2_OR_NEWER
namespace ThirteenPixels.Placr
{
    using UnityEditor;
    using UnityEditor.Overlays;
    using UnityEngine.UIElements;

    [Overlay(typeof(SceneView), "Placr", true)]
    public class PlacrOverlay : Overlay, ITransientOverlay
    {
        public bool visible => Placr.isActive;

        public override VisualElement CreatePanelContent()
        {
            return new IMGUIContainer(() => Placr.instance?.DrawOverlayGUI());
        }
    }
}
#endif