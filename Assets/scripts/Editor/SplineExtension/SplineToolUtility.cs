using UnityEditor.Splines;
using System.Collections.Generic;
using UnityEngine.Splines;
using System;

namespace UnityEditor.Splines
{
    public static class SplineToolUtility
    {
        public static Action OnSelectionChanged;

        public static bool HasSelection()
        {
            return SplineSelection.Count > 0;
        }

        public static List<SelectedSplineElementInfo> GetSelection()
        {
            List<SelectableSplineElement> elements = SplineSelection.selection;

            List<SelectedSplineElementInfo> info = new List<SelectedSplineElementInfo>();

            foreach (SelectableSplineElement element in elements)
            {
                info.Add(new SelectedSplineElementInfo(element.target, element.targetIndex, element.knotIndex,
                    element.tangentIndex));
            }

            return info;
        }

        public static void RegisterSelectionChangedEvent() => SplineSelection.changed += OnSelectionChangedFunc;
        public static void UnRegisterSelectionChangedEvent() => SplineSelection.changed -= OnSelectionChangedFunc;

        private static void OnSelectionChangedFunc() => OnSelectionChanged?.Invoke();

        public struct SelectedSplineElementInfo
        {
            public Object target;
            public int targetIndex;
            public int knotIndex;
            public int tangentIndex;

            public SelectedSplineElementInfo(Object target, int targetIndex, int knotIndex, int tangentIndex)
            {
                this.target = target;
                this.targetIndex = targetIndex;
                this.knotIndex = knotIndex;
                this.tangentIndex = tangentIndex;
            }
        }
    }
}
