using NanoUI.Common;
using NanoUI.Layouts;
using NanoUI.Nvg;
using System;
using System.Numerics;

namespace NanoUI.Components.Dialogs
{
    // todo : set size based on message text length + font size
    // todo2: we could calculate automatically if text is single/multiline & set properties based on this
    public class UIMessageBox : UIDialog
    {
        bool _inited;
        UILabel? _iconLabel;
        // support for single & multilines
        UIScrollableLabel? _multilineText;
        UILabel? _textLabel;

        // int is button index ("Ok" = 0, "Cancel" = 1)
        // note we don't want to have reference => could be disposed (no need to create new)!!!
        Action<UIWidget, int>? _buttonClicked;

        // this is ctor for theme/layout generation (if you use this otherwise, set parent before using widget)
        public UIMessageBox()
        {
            // set defaults to theme impl - prevents circular reference
            Icon = default;
            // window related
            DragResizable = false;
            DisablePointerFocus = true;
            //Draggable = false;
        }

        // note: it is not recommended to call in your code. Instead call Screen.GetDialog<MessageBox>.
        // if you still want to call this, you are responsible to handle dispose new instance manually
        public UIMessageBox(UIScreen screen)
            : base(screen)
        {
            // todo : check
            ChildrenLayout = new StackLayout(Orientation.Vertical, LayoutAlignment.Middle);
            // todo: fixed?
            FixedSize = new Vector2(350, 163);
            
            var messagePanel = new UIWidget(this);

            messagePanel.ChildrenLayout = new GridLayout(Orientation.Horizontal, 2,
                   LayoutAlignment.Middle)
            {
                Spacing = new Vector2(15)
            };

            // we delay icon set, because we need nvg context to convert icon -> string
            _iconLabel = new UILabel(messagePanel, string.Empty);
            _iconLabel.FontFaceId = FontIconsId;
            _iconLabel.FontSize = 35;
            _iconLabel.FixedSize = new Vector2(45);

            // message - supports long message texts - valign top
            _multilineText = new UIScrollableLabel(messagePanel);
            _multilineText.FixedSize = new Vector2(240, 58);
            _multilineText.Border = false;
            _multilineText.Visible = false;

            // single line - no scrolling, valign middle
            _textLabel = new UILabel(messagePanel, string.Empty);
            _textLabel.FixedSize = new Vector2(240, 58);
            _textLabel.WrapText = false;
            _textLabel.Visible = true;
            
            // Buttons
            UIWidget buttonPanel = new UIWidget(this);
            buttonPanel.ChildrenLayout = new StackLayout(Orientation.Horizontal, LayoutAlignment.Middle) { Spacing = new Vector2(10) };
            buttonPanel.Margin = ButtonPanelMargin;

            // default button texts
            (string Ok, string Cancel) buttonTexts = GetButtonTexts();

            _okButton = new UIButton(buttonPanel, buttonTexts.Ok);
            _okButton.FixedSize = new Vector2(90, 0);
            _okButton.Clicked += () =>
            {
                if(_caller != null)
                {
                    _buttonClicked?.Invoke(_caller, 0);
                }
                
                Close();
            };

            // "cancel" button is by default invisible
            _altButton = new UIButton(buttonPanel, buttonTexts.Cancel);
            _altButton.FixedSize = new Vector2(90, 0);
            _altButton.Visible = false;
            _altButton.Clicked += () =>
            {
                if(_caller != null)
                {
                    _buttonClicked?.Invoke(_caller, 1);
                }
                
                Close();
            };
        }

        #region Properties

        MessageDialogType _dialogType;
        public MessageDialogType DialogType
        {
            get => _dialogType;
            set
            { 
                _dialogType = value;
                // note: we set default title by type, user can change title at will
                Title = _dialogType.ToString();
            }
        }

        bool _scrollText = false;
        public bool ScrollText
        {
            get => _scrollText;
            set
            {
                _scrollText = value;

                if (_multilineText != null)
                {
                    _multilineText.Visible = _scrollText;
                }

                if (_textLabel != null)
                {
                    _textLabel.Visible = !_scrollText;
                }
                
                RequestLayoutUpdate(this);
            }
        }

        public string Text
        {
            set
            {
                // set to both possible widgets (supports user sets single line property after text property)
                if (_textLabel != null)
                {
                    _textLabel.Caption = value;
                }
                    
                _multilineText?.SetText(value);
            }
        }
        
        int? _icon;
        public int Icon
        {
            get => _icon?? GetMessageIcon();
            set => _icon = value;
        }

        UIButton? _okButton;
        public UIButton? OKButton => _okButton;
        
        UIButton? _altButton;
        public UIButton? AltButton => _altButton;
        
        #endregion

        #region Methods

        // we use caller as identifier
        public void SetCallback(UIWidget caller, Action<UIWidget, int>? action)
        {
            _caller = caller;
            _buttonClicked = action;
            Visible = true;

            _inited = false;
        }

        #endregion

        #region Drawing

        /// <inheritdoc />
        public override void Draw(NvgContext ctx)
        {
            if (!_inited)
            {
                // delayed set
                if(_iconLabel != null)
                {
                    _iconLabel.Caption = char.ConvertFromUtf32(Icon);
                }
                
                _inited = true;

                ReInit(ctx);
            }

            base.Draw(ctx);
        }

        #endregion

        #region Private

        // these are default texts
        // todo: localization
        (string, string) GetButtonTexts()
        {
            switch (_dialogType)
            {
                case MessageDialogType.Question:
                    return ("Yes", "No");
                case MessageDialogType.Information:
                case MessageDialogType.Warning:
                case MessageDialogType.Error:
                default:
                    return ("OK", "Cancel");
            }
        }

        // todo: should this be in theme?
        int GetMessageIcon()
        {
            switch (_dialogType)
            {
                case MessageDialogType.Question:
                    return GetTheme().Fonts.IconQuestion;
                case MessageDialogType.Warning:
                    return GetTheme().Fonts.IconWarning;
                case MessageDialogType.Error:
                    return GetTheme().Fonts.IconError;
                case MessageDialogType.Information:
                default:
                    return GetTheme().Fonts.IconInformation;
            }
        }

        #endregion
    }
}
