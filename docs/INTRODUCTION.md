This document is a brief introduction to the NanoUI. You can find in code and samples, how the things described here are implemented and can be used.


# Drawing Layer

This is the real engine in NanoUI. It handles all commands passed to it and creates valid draw commands. When you are going to render the ui, you just loop through draw commands that are created and get vertices, indices and uniforms, that you send to your graphics engine.

Functions in drawing layer are mostly internal and you should consider it as a black box.

You issue commands to the drawing layer through its API, that is in **NvgContext** class.


# UI layer

UI layer is the extendable part of the NanoUI. It basically consist widgets, that are arranged in tree.

### UIWidget
**UIWidget** is the base class in UI layer. It provides most common properties and functions. All other widgets are derived from this component.
When you extend/override functionality in this class, you probably should also call same function in the base class, since there is often functionality that needs to be executed to keep UI layer consistent.

### WidgetList
**WidgetList** is a special class that holds all (parent) widget's childs. There are some helper methods to interact with this list, but the most important part is the order of child widgets in this list:

- when NanoUI tries to find which child widget should handle user input (mouse, keyboard events etc), it loops the list **backwards** (from the last to the first)
- in drawing phase the looping is done **forwards** (from the first to the last). This ensures that you can have any kind of transparency with overlapping widgets.

**WidgetList** is a property in **UIWidget** class, so every widget can act as a container or layer.

### UIWindow
**UIWindow** is the only widget that provides the opportunity to reposition and resize widget with mouse dragging.

### UIScreen
**UIScreen** is the root widget in the widget tree. It is itself also derived from the **UIWidget**, but its main purpose is to orchestrate all the widgets in the widget tree. When you want to send user input events to NanoUI, you send the event to the **UIScreen** and it passes it to the correct widget.

# Layouting

NanoUI uses relative positioning system and uses top-left coordinate as position "anchor". That means that every widget knows only their position in their parent's space. So for example position (0, 0) means that widget is positioned in the top-left corner of its parent space.

Layouts are defined in separate classes and can be dynamically changed at the runtime. When you attach specific layout class to widget, you must also call either **PerformLayout** or **RequestLayoutUpdate** in order to really process layout calculations and set child widgets' positions & sizes.

# Theming / styling

NanoUI uses dynamic styling. The **UITheme** class, that consists all styling information is only stored in the **UIScreen**. When widget wants to use its styling property, it must ask that from the **UIScreen's** theme class.

However you can "hard code" styling information also directly to any individual widget.
