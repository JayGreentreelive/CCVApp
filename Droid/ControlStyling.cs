using System;
using Android.Widget;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Config;
using Android.Views;
using Rock.Mobile.PlatformCommon;
using Android.Graphics;

namespace Droid
{
    public class ControlStyling
    {
        public static void StyleButton( Button button, string text, string font, uint size )
        {
            button.Text = text;
            button.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.Button_BGColor ) );

            button.SetTypeface( DroidFontManager.Instance.GetFont( font ), TypefaceStyle.Normal );
            button.SetTextSize( Android.Util.ComplexUnitType.Dip, size );
        }

        public static void StyleUILabel( TextView label, string font, uint size )
        {
            label.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.Label_TextColor ) );
            label.SetBackgroundColor( Android.Graphics.Color.Transparent );

            label.SetTypeface( DroidFontManager.Instance.GetFont( font ), TypefaceStyle.Normal );
            label.SetTextSize( Android.Util.ComplexUnitType.Dip, size );
        }

        public static void StyleBGLayer( View backgroundLayout )
        {
            backgroundLayout.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

            View borderView = backgroundLayout.FindViewById<View>( Resource.Id.top_border );
            borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

            borderView = backgroundLayout.FindViewById<View>( Resource.Id.bottom_border );
            borderView.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
        }

        public static void StyleTextField( EditText textField, string placeholderText, string font, uint size )
        {
            textField.SetBackgroundDrawable( null );
            textField.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );

            textField.Hint = placeholderText;
            textField.SetHintTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );

            textField.SetTypeface( DroidFontManager.Instance.GetFont( font ), TypefaceStyle.Normal );
            textField.SetTextSize( Android.Util.ComplexUnitType.Dip, size );
        }  
    }
}

