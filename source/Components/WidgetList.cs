using NanoUI.Common;
using NanoUI.Components.Scrolling;
using NanoUI.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace NanoUI.Components
{
    // todo : There could be Actions invoked when adding, removing, inserting etc
    // todo: there could be restrictions that only certain type of widgets are allowed
    // todo: find functions - do not take Widgets with Visible = false (checkVisibility flag)?

    /// <summary>
    /// WidgetList is a list to hold all widget's children. It provides also some helper functions.
    /// Note: you are responsible to call PerformLayout/RequestLayoutUpdate when list has changed.
    /// </summary>
    public class WidgetList : ArrayBuffer<UIWidget>
    {
        internal static SortKeyComparer sortKeyComparer = new();

        UIWidget _parent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parent">Owner</param>
        public WidgetList(UIWidget parent)
            :base(4)
        { 
            _parent = parent;
        }

        /// <summary>
        /// Parent (owner) of this list.
        /// </summary>
        public UIWidget Parent => _parent;

        /// <summary>
        /// Add a widget.
        /// </summary>
        /// <param name="widget">UIWidget</param>
        public override void Add(UIWidget widget)
        {
            // remove from last parent?
            if (widget.Parent != _parent)
                widget.Parent?.Children.Remove(widget);

            widget.Parent = _parent;

            base.Add(widget);
        }

        /// <summary>
        /// Add a widget.
        /// </summary>
        /// <param name="widget">UIWidget</param>
        public override void Add(ref UIWidget widget)
        {
            // remove from last parent?
            if (widget.Parent != _parent)
                widget.Parent?.Children.Remove(ref widget);

            widget.Parent = _parent;

            base.Add(ref widget);
        }

        /// <summary>
        /// Tries tod get widget in index and convert it to desired type if possible.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="index">Index</param>
        /// <param name="widget">Widget of type T</param>
        /// <returns>Success</returns>
        /// <remarks>If conversion is not needed, use Childen[index].</remarks>
        public bool TryGet<T>(int index, out T? widget) where T : UIWidget
        {
            if(index >= 0 && index < Count)
            {
                var child = this[index];
                if (child is T res)
                {
                    widget = res;
                    return true;
                }
            }

            widget = null;
            return false;
        }

        /// <summary>
        /// Finds widget by id property.
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="recursive">Recursive</param>
        /// <returns>UIWidget</returns>
        public UIWidget? FindById(Guid id, bool recursive = true)
        {
            if (_parent.Id == id)
                return _parent;

            UIWidget? found = null;

            foreach (var child in AsReadOnlySpan())
            {
                if (child.Id == id)
                    return child;

                if (recursive)
                {
                    found = child.Children.FindById(id);

                    if (found != null)
                        return found;
                }
            }

            return found;
        }

        /// <summary>
        /// Finds widget by name property.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="recursive">Recursive</param>
        /// <returns>UIWidget</returns>
        public UIWidget? FindByName(string name, bool recursive = true) 
        {
            if (_parent.Name == name)
                return _parent;

            UIWidget? found = null;

            foreach (var child in AsReadOnlySpan())
            {
                if(child.Name == name)
                    return child;

                if (recursive)
                {
                    found = child.Children.FindByName(name);

                    if (found != null)
                        return found;
                }
            }

            return found;
        }

        /// <summary>
        /// Finds widget by name property and converts it to T if possible.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="name">Name</param>
        /// <returns>Widget of type T</returns>
        public T? FindByName<T>(string name) where T : UIWidget
        {
            if (_parent.Name == name && _parent is T res)
                return res;

            foreach (UIWidget widget in AsReadOnlySpan())
            {
                if (widget.Name == name && widget is T found)
                    return found;

                var w = widget.Children.FindByName<T>(name);

                if (w != null)
                    return w;
            }

            return null;
        }

        /// <summary>
        ///  Finds first widget of type T.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="recursive">Recursive</param>
        /// <returns>Widget of type T</returns>
        public T? FindFirst<T>(bool recursive = true) where T : UIWidget
        {
            if (_parent is T parentType)
                return parentType;

            T? found = null;

            foreach (var child in AsReadOnlySpan())
            {
                if(child is T childType)
                    return childType;

                // recursive
                if (recursive)
                {
                    found = child.Children.FindFirst<T>();

                    if (found != null)
                        return found;
                }
            }

            return found;
        }

        /// <summary>
        /// Finds last widget of type T.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="recursive">Recursive</param>
        /// <returns>Widget of type T</returns>
        public T? FindLast<T>(bool recursive = true) where T : UIWidget 
        {
            T? found = null;

            if (Count > 0)
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    ref UIWidget child = ref this[i];

                    if (child is T childType)
                        return childType;

                    // recursive
                    if (recursive)
                    {
                        found = child.Children.FindLast<T>();

                        if (found != null)
                            return found;
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Finds first widget of type T with position.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="displayPosition">Display position</param>
        /// <param name="recursive">Recursive</param>
        /// <returns>Widget of type T</returns>
        public T? Find<T>(Vector2 displayPosition, bool recursive = true) where T : UIWidget
        {
            var position = ConvertPosition(displayPosition);

            foreach (var child in AsReadOnlySpan())
            {
                if (child.Contains(position) && child is T widget)
                {
                    return widget;
                }

                if (recursive)
                {
                    // note: the recursive call uses widget's coordinates (not display coordinates)
                    var found = child.Children.Find<T>(position - child.Position);

                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds first widget with position and returns also index.
        /// </summary>
        /// <param name="displayPosition">Display position</param>
        /// <param name="index">Index</param>
        /// <returns>UIWidget</returns>
        /// <remarks>Searches only current children; doesn't support hierarchial structures.</remarks>
        public UIWidget? Find(Vector2 displayPosition, out int index)
        {
            if (Count > 0)
            {
                var position = ConvertPosition(displayPosition);

                // loop backwards since we want to get topmost widget (drawn latest)
                for (int i = Count - 1; i >= 0; i--)
                {
                    UIWidget child = this[i];

                    if (child.Visible && child.Contains(position))
                    {
                        index = i;
                        return child;
                    }
                }
            }

            index = -1;
            return null;
        }

        /// <summary>
        /// Finds the topmost widget located at the given position value.
        /// Uses recursive search.
        /// </summary>
        /// <param name="displayPosition">Display position</param>
        /// <returns>UIWidget</returns>
        public UIWidget? FindTopmost(Vector2 displayPosition)
        {
            if(Count > 0)
            {
                var position = ConvertPosition(displayPosition);

                // loop backwards since we want to get topmost widget (drawn latest)
                for (int i = Count - 1; i >= 0; i--)
                {
                    UIWidget child = this[i];

                    if (child.Visible && child.Contains(position))
                    {
                        // note: the recursive call uses widget's coordinates (not display coordinates)
                        var found = child.Children.FindTopmost(position - child.Position);

                        if(found != null)
                            return found;

                        return child;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds widget by predicate.
        /// </summary>
        /// <param name="condition">Predicate</param>
        /// <param name="inchildren">In children?</param>
        /// <returns>UIWidget</returns>
        public UIWidget? Find(Predicate<UIWidget> condition, bool inchildren = true)
        {
            if (Count > 0)
            {
                // loop backwards since we want to get topmost widget (drawn latest)
                for (int i = Count - 1; i >= 0; i--)
                {
                    UIWidget child = this[i];

                    if (condition(child))
                        return child;

                    if (inchildren)
                    {
                        var w = child.Children.Find(condition, inchildren);

                        if (w != null)
                            return w;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to attach widget.
        /// This is called from the UIScreen, when it has dragwidget and pointer up event is fired.
        /// </summary>
        /// <param name="widget">UIWidget</param>
        /// <param name="displayPosition">Display position</param>
        /// <returns>Success</returns>
        /// <remarks>Calls widget's OnAttach event and if widget wants to handle event (returns true), this stops looking further.</remarks>
        public bool TryAttachWidget(UIWidget widget, Vector2 displayPosition)
        {
            if (Count > 0)
            {
                var position = ConvertPosition(displayPosition);

                // loop backwards since we want to get topmost widget (drawn latest)
                for (int i = Count - 1; i >= 0; i--)
                {
                    UIWidget child = this[i];

                    if (child.Visible && child.Contains(position) && child != widget)
                    {
                        var res = child.Children.TryAttachWidget(widget, position - child.Position);

                        if (res)
                            return res;

                        return child.OnAttach(widget, position);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Remove a widget.
        /// </summary>
        /// <param name="name">Name</param>
        public void Remove(string name)
        {
            var w = FindByName(name, false);

            if (w != null)
                Remove(w);
        }

        /// <summary>
        /// Sort widgets in the list. Uses default sorter (checks widgets' sort keys).
        /// </summary>
        public void Sort()
        {
            Sort(sortKeyComparer);
        }

        /// <summary>
        /// Move to first.
        /// </summary>
        /// <param name="child">UIWidget</param>
        /// <returns>Success</returns>
        public bool MoveToFirst(UIWidget child)
        {
            if (IndexOf(child) == 0)
            {
                // already there
                return true;
            }

            Remove(child);
            Insert(0, child);

            return true;
        }

        /// <summary>
        /// Move to last.
        /// </summary>
        /// <param name="widget">UIWidget</param>
        /// <returns>Success</returns>
        public bool MoveToLast(UIWidget widget)
        {
            if (Contains(widget))
            {
                Remove(widget);
                Add(widget);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Swap childs in indexes.
        /// </summary>
        /// <param name="index1">Index1</param>
        /// <param name="index2">Index2</param>
        /// <returns>Success</returns>
        public bool Swap(int index1, int index2)
        {
            if (index1 < 0 || index2 < 0 || index1 >= Count || index2 >= Count)
                return false;

            MathUtils.Swap(ref this[index1], ref this[index2]);
            
            return true;
        }

        /// <summary>
        /// Convert pointer position (display coordinates) to
        /// widget's local position (widget tree coordinates).
        /// </summary>
        Vector2 ConvertPosition(Vector2 pointerPos)
        {
            if (_parent is IScrollable scrollable)
            {
                // note: scroll offset is negative, so double negative makes it positive
                return pointerPos - scrollable.ScrollOffset;
            }

            return pointerPos;
        }
    }

    /// <summary>
    /// Default sort key property comparer.
    /// </summary>
    internal class SortKeyComparer : IComparer<UIWidget>
    {
        public int Compare(UIWidget? x, UIWidget? y)
        {
            if(x == null || y == null)
                return 0;

            return x.SortKey.CompareTo(y.SortKey);
        }
    }
}
