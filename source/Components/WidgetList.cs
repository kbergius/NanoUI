using NanoUI.Common;
using NanoUI.Components.Scrolling;
using NanoUI.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace NanoUI.Components
{
    // List to hold all widget's children & some helper functions

    // todo : There could be Actions invoked when adding, removing, inserting etc
    // todo: there could be restrictions that only certain type of widgets are allowed
    // note: user is responsible to call RequestLayoutUpdate when list has changed
    // note: all positions here are treated as display positions & converted to widget positions

    // todo: find functions - do not take Widgets with Visible = false (checkVisibility flag)?
    public class WidgetList : ArrayBuffer<UIWidget>
    {
        internal static SortKeyComparer sortKeyComparer = new();

        UIWidget _parent;

        public WidgetList(UIWidget parent)
            :base(4)
        { 
            _parent = parent;
        }

        public UIWidget Parent => _parent;

        // override base implementation to set parent & theme
        public override void Add(UIWidget widget)
        {
            // remove from last parent?
            if (widget.Parent != _parent)
                widget.Parent?.Children.Remove(widget);

            widget.Parent = _parent;

            base.Add(widget);
        }

        // override base implementation to set parent & theme
        public override void Add(ref UIWidget widget)
        {
            // remove from last parent?
            if (widget.Parent != _parent)
                widget.Parent?.Children.Remove(ref widget);

            widget.Parent = _parent;

            base.Add(ref widget);
        }

        // try get widget in index & convert it to desired type if possible
        // note: if conversion is not needed, use Childen[index]
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

        // name is Widget's Name
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

        // name is Widget's Name
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

        // we search only current children (this doesn't support hierarcial structures)
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

        // Determine the topmost widget located at the given position value (recursive)
        // Check childs first
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

        // this is called from screen, when it has dragwidget & pointer button is up
        // it calls widget's OnAttach event and if widget want to handle event, this stops looping further
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

        public void Remove(string name)
        {
            var w = FindByName(name, false);

            if (w != null)
                Remove(w);
        }

        // Default sort with widgets SortKey
        // sort with spesified comparer is in base class
        public void Sort()
        {
            Sort(sortKeyComparer);
        }

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

        // swap childs
        public bool Swap(int index1, int index2)
        {
            if (index1 < 0 || index2 < 0 || index1 >= Count || index2 >= Count)
                return false;

            MathUtils.Swap(ref this[index1], ref this[index2]);
            
            return true;
        }

        // when dealing with pointer pos (display coordinates) we have to convert position to
        // widget position (widget tree coordinates)
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

    // default comparer that uses sort key
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