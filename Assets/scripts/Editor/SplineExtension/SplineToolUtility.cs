using UnityEditor.Splines;
using System.Collections.Generic;
using System;

namespace UnityEditor.Splines
{
    public static class SplineToolUtility
    {
        private static Action _onSelectionChanged;
        public static event Action OnSelectionChanged
        {
            add
            {
                if (_onSelectionChanged == null)
                    SplineSelection.changed += OnSelectionChangedFunc;
                _onSelectionChanged += value;
            }
            remove
            {
                _onSelectionChanged -= value;
                if (_onSelectionChanged == null)
                    SplineSelection.changed -= OnSelectionChangedFunc;
            }
        }

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

        private static void OnSelectionChangedFunc() => _onSelectionChanged?.Invoke();

        public struct SelectedSplineElementInfo
        {
            public UnityEngine.Object target;
            public int targetIndex;
            public int knotIndex;
            public int tangentIndex;

            public SelectedSplineElementInfo(UnityEngine.Object target, int targetIndex, int knotIndex, int tangentIndex)
            {
                this.target = target;
                this.targetIndex = targetIndex;
                this.knotIndex = knotIndex;
                this.tangentIndex = tangentIndex;
            }
        }
    }
}
