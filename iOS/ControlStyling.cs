using System;
using MonoTouch.UIKit;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Config;
using MonoTouch.Foundation;

namespace iOS
{
    public class ControlStyling
    {
        public static void StyleButton( UIButton button, string text, string font, uint size )
        {
            button.SetTitle( text, UIControlState.Normal );

            button.SetTitleColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Button_TextColor ), UIControlState.Normal );
            button.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Button_BGColor );

            button.Layer.CornerRadius = ControlStylingConfig.Button_CornerRadius;

            button.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( font, size );
        }

        public static void StyleUILabel( UILabel label, string font, uint size )
        {
            label.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
            label.BackgroundColor = UIColor.Clear;

            label.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( font, size );
        }

        public static void StyleBGLayer( UIView view )
        {
            view.Layer.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
            view.Layer.BorderColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ).CGColor;
            view.Layer.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;
        }

        public static void StyleTextField( UITextField textField, string placeholderText, string font, uint size )
        {
            textField.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
            textField.AttributedPlaceholder = new NSAttributedString( placeholderText, null, Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
            textField.BackgroundColor = UIColor.Clear;

            textField.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( font, size );
        }  
    }
}

