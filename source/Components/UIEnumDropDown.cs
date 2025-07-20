using System;

namespace NanoUI.Components
{
    // this is just simple enum combo box with only 1 column (enum text)
    public class UIEnumDropDown<T> : UIComboBox<T> where T : struct, Enum
    {
        public UIEnumDropDown(UIWidget parent)
            : base(parent)
        {
            foreach (T value in Enum.GetValues<T>())
            {
                AddItem(value.ToString(), value);
            }
        }

        #region Methods

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