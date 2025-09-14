using System;

namespace NanoUI.Components
{
    /// <summary>
    /// UIEnumDropDown<T> is just simple enum combo box with only 1 column (enum text).
    /// </summary>
    public class UIEnumDropDown<T> : UIComboBox<T> where T : struct, Enum
    {
        /// <inheritdoc />
        public UIEnumDropDown(UIWidget parent)
            : base(parent)
        {
            foreach (T value in Enum.GetValues<T>())
            {
                AddItem(value.ToString(), value);
            }
        }

        #region Methods

        /// <inheritdoc />
        public override void SetSelected(T @enum)
        {
            int index = 0;

            foreach (var item in Enum.GetValues<T>())
            {
                if (item.Equals(@enum))
                {
                    SelectedIndex = index;
                    break;
                }
                index++;
            }
        }

        #endregion
    }
}
