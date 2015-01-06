using System;
using Android.Widget;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Config;
using Android.Views;
using Android.Graphics;

namespace Droid
{
    public class ControlStyling
    {
        public static void StyleButton( Button button, string text, string font, uint size )
        {
            button.Text = text;
            button.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Button_BGColor ) );

            button.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( font ), TypefaceStyle.Normal );
            button.SetTextSize( Android.Util.ComplexUnitType.Dip, size );
        }

        public static void StyleUILabel( TextView label, string font, uint size )
        {
            label.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
            label.SetBackgroundColor( Android.Graphics.Color.Transparent );

            label.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( font ), TypefaceStyle.Normal );
            label.SetTextSize( Android.Util.ComplexUnitType.Dip, size );
        }

        public static void StyleBGLayer( View backgroundLayout )
        {
            backgroundLayout.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

            View borderView = backgroundLayout.FindViewById<View>( Resource.Id.top_border );
            borderView.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            borderView = backgroundLayout.FindViewById<View>( Resource.Id.bottom_border );
            borderView.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
        }

        public static void StyleTextField( EditText textField, string placeholderText, string font, uint size )
        {
            textField.SetBackgroundDrawable( null );
            textField.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );

            textField.Hint = placeholderText;
            textField.SetHintTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );

            textField.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( font ), TypefaceStyle.Normal );
            textField.SetTextSize( Android.Util.ComplexUnitType.Dip, size );
        }  
    }
}

