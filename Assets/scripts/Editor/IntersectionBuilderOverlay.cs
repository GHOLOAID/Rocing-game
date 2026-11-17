using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Splines;
using UnityEditor.Splines;
using UnityEngine;
using System;

[Overlay(typeof(SceneView), "Intersection Builder", true)]
public class IntersectionBuilderOverlay : Overlay
{
    private Label _knotInfoLabel;
    private Button _buildIntersectionButton;
    private Label _intersectionLabel;
    private VisualElement _sliderArea;

    private Intersection _selectedIntersection;
    public override VisualElement CreatePanelContent()
    {
        var root = new VisualElement() { name = "Intersection Builder"};
        
        _knotInfoLabel = new Label() { text = "No Knots Selected"};
        root.Add(_knotInfoLabel);

        _buildIntersectionButton = new Button() { text = "Build Intersection" };
        _buildIntersectionButton.clicked += OnBuildIntersection;
        _buildIntersectionButton.SetEnabled(false);
        root.Add(_buildIntersectionButton);
        
        _intersectionLabel = new Label() { text = "No Intersection Selected" };
        root.Add(_intersectionLabel);
        
        _sliderArea = new VisualElement();
        root.Add(_sliderArea);

        return root;
    }
    
    public override void OnCreated()
    {
        base.OnCreated();
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += OnSelectionChanged;
        SplineToolUtility.OnSelectionChanged += OnSelectionChanged;
    }

    public override void OnWillBeDestroyed()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= OnSelectionChanged;
        
        SplineToolUtility.OnSelectionChanged -= OnSelectionChanged;
        
        if (_buildIntersectionButton != null)
            _buildIntersectionButton.clicked -= OnBuildIntersection;
        
        base.OnWillBeDestroyed();
    }
    
    private void OnSelectionChanged()
    {
        if (_knotInfoLabel == null || _sliderArea == null) 
            return;
        
        if (SplineToolUtility.HasSelection())
            UpdateSelectionInfo();
        else 
            NoSelection();
        
        if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent<roadGeneration>(out _))
        {
            _selectedIntersection = null;
            _intersectionLabel.text = "No Intersection Selected";
            _sliderArea.Clear();
        }
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
        
        roadGeneration roadGen = Selection.activeGameObject.GetComponent<roadGeneration>();
        if (roadGen == null)
            return;

        Undo.RecordObject(roadGen, "Build Intersection");
        
        roadGen.BuildIntersection(intersection);
        _selectedIntersection = intersection;
        ShowIntersection(intersection);
    }

    private void ShowIntersection(Intersection intersection)
    {
        _intersectionLabel.text = "Selected Intersection";
        
        _sliderArea.Clear();
        for (int i = 0; i < intersection.curves.Count; i++)
        {
            Slider slider = new Slider($"Curve {i}", 0f, 1f, SliderDirection.Horizontal);
            slider.labelElement.style.minWidth = 60f;
            slider.labelElement.style.maxWidth = 80f;
            slider.style.minWidth = 180f;
            slider.value = intersection.curves[i];
            int value = i; //for loop i isn't preserved in lambda callback, local variables like value here are
            slider.RegisterValueChangedCallback((x) =>
            {
                if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent<roadGeneration>(out roadGeneration roadGen))
                    return;

                if (!roadGen.Intersections.Contains(intersection))
                    return;

                Undo.RecordObject(roadGen, "Change Intersection Curve");
                intersection.curves[value] = x.newValue;
                EditorUtility.SetDirty(roadGen);

                roadGen.Rebuild();
            });
            _sliderArea.Add(slider);
        }
    }

    //used to add selectable buttons for the intersections in sceneview
    private void OnSceneGUI(SceneView sceneView)
    {
        if (Selection.activeGameObject == null) 
            return;
        
        if (!Selection.activeGameObject.TryGetComponent<roadGeneration>(out roadGeneration roadGen)) 
            return;
        
        if (roadGen.Intersections == null) 
            return;
        
        for (int i = 0; i < roadGen.Intersections.Count; i++)
        {
            Intersection intersection = roadGen.Intersections[i];
        
            int count = 0;
            Vector3 center = Vector3.zero;
            for(int j = 0; j < intersection.points.Count; j++)
            {
                float t  = intersection.points[j].knotIndex == 0 ? 0f : 1f;
                roadGen.Sampler.Sample(intersection.points[j].splineIndex, t, roadGen.Width, out Vector3 _, out Vector3 pLeft, out Vector3 pRight, true);
                center += pLeft;
                center += pRight;
                count ++;
            }

            if (count == 0)
                continue;
            
            center = center/(count*2);
            
            
            float handleSize = HandleUtility.GetHandleSize(center) * 0.25f;

            Handles.color = (_selectedIntersection == intersection) ? Color.green : Color.cyan;

            if (Handles.Button(center, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
            {
                _selectedIntersection = intersection;
                ShowIntersection(intersection);
            
                //Force the overlay UI to repaint immediately
                sceneView.Repaint(); 
            }
        }
    }
}
