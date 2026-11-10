using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Splines;
using UnityEditor.Splines;

[Overlay(typeof(SceneView), "Intersection Builder", true)]
public class IntersectionBuilderOverlay : Overlay
{
    private Label _knotInfoLabel;
    private Button _buildIntersectionButton;

    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement() { name = "Intersection Builder"};
        
        _knotInfoLabel = new Label() { text = "No Knots Selected"};
        root.Add(_knotInfoLabel);

        _buildIntersectionButton = new Button() { text = "Build Intersection" };
        _buildIntersectionButton.clicked += OnBuildIntersection;
        _buildIntersectionButton.SetEnabled(false);
        root.Add(_buildIntersectionButton);
        
        SplineToolUtility.RegisterSelectionChangedEvent();
        SplineToolUtility.OnSelectionChanged += OnSelectionChanged;

        return root;
    }
    
    private void OnSelectionChanged()
    {
        if (SplineToolUtility.HasSelection())
            UpdateSelectionInfo();
        else 
            NoSelection();
    }

    private void NoSelection()
    {
        _knotInfoLabel.text = "No Knots Selected";
        _buildIntersectionButton.SetEnabled(false);
    }

    private void UpdateSelectionInfo()
    {
        _knotInfoLabel.text = "";
        
        List<SplineToolUtility.SelectedSplineElementInfo> info = SplineToolUtility.GetSelection();

        if (info == null || info.Count == 0)
        {
            NoSelection(); 
            return;
        }

        foreach (var element in info)
        {
            _knotInfoLabel.text += $"Spline {element.targetIndex}, Knot {element.knotIndex}\n";
        }

        _buildIntersectionButton.SetEnabled(true);
    }

    private void OnBuildIntersection()
    {
        if (!_buildIntersectionButton.enabledInHierarchy)
            return;

        if (!SplineToolUtility.HasSelection())
            return;
        
        List<SplineToolUtility.SelectedSplineElementInfo> info = SplineToolUtility.GetSelection();

        if (info == null || info.Count == 0)
            return;
        
        Intersection intersection = new Intersection();
        foreach (var element in info)
        {
            SplineContainer container = (SplineContainer)element.target;
            Spline spline = container.Splines[element.targetIndex];
            intersection.AddPoint(element.targetIndex, element.knotIndex, spline, spline.Knots.ToList()[element.knotIndex]);
        }
        
        Selection.activeGameObject.GetComponent<roadGeneration>().BuildIntersection(intersection);
    }
}
