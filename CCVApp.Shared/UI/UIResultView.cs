using System;
using Rock.Mobile.PlatformUI;
using System.Drawing;
using CCVApp.Shared.Config;

namespace CCVApp.Shared.UI
{
    /// <summary>
    /// Used to display a result to a user, including a status message
    /// </summary>
    public class UIResultView
    {
        PlatformView View { get; set; }

        PlatformLabel StatusLabel { get; set; }
        PlatformView StatusBackground { get; set; }

        PlatformLabel ResultSymbol { get; set; }
        PlatformLabel ResultLabel { get; set; }
        PlatformView ResultBackground { get; set; }

        PlatformButton DoneButton { get; set; }

        public delegate void DoneClickDelegate( );

        public UIResultView( object parentView, RectangleF frame, DoneClickDelegate onClick )
        {
            View = PlatformView.Create( );
            View.AddAsSubview( parentView );
            View.UserInteractionEnabled = false;

            StatusLabel = PlatformLabel.Create( );
            StatusBackground = PlatformView.Create( );

            ResultSymbol = PlatformLabel.Create( );
            ResultLabel = PlatformLabel.Create( );
            ResultBackground = PlatformView.Create( );

            DoneButton = PlatformButton.Create( );

            // setup our UI hierarchy
            StatusBackground.AddAsSubview( parentView );
            StatusBackground.UserInteractionEnabled = false;
            StatusBackground.BorderWidth = .5f;

            StatusLabel.AddAsSubview( parentView );
            StatusLabel.UserInteractionEnabled = false;


            ResultBackground.AddAsSubview( parentView );
            ResultBackground.UserInteractionEnabled = false;
            ResultBackground.BorderWidth = .5f;

            ResultSymbol.AddAsSubview( parentView );
            ResultSymbol.UserInteractionEnabled = false;

            ResultLabel.AddAsSubview( parentView );
            ResultLabel.UserInteractionEnabled = false;

            DoneButton.AddAsSubview( parentView );
            DoneButton.ClickEvent = ( PlatformButton button ) =>
            {
                if( onClick != null )
                {
                    onClick( );
                }
            };


            // default the view size and opacity
            SetOpacity( 0.00f );

            SetBounds( frame );

            // give it a default style
            SetStyle( ControlStylingConfig.Medium_Font_Light, 
                ControlStylingConfig.Icon_Font_Secondary, 
                ControlStylingConfig.BackgroundColor,
                ControlStylingConfig.BG_Layer_Color, 
                ControlStylingConfig.BG_Layer_BorderColor, 
                ControlStylingConfig.TextField_PlaceholderTextColor,
                ControlStylingConfig.Button_BGColor, 
                ControlStylingConfig.Button_TextColor );
        }

        void SetOpacity( float opacity )
        {
            View.Opacity = opacity;

            ResultSymbol.Opacity = opacity;
            ResultLabel.Opacity = opacity;
            ResultBackground.Opacity = opacity;

            StatusLabel.Opacity = opacity;
            StatusBackground.Opacity = opacity;

            DoneButton.Opacity = opacity;
        }

        public void SetStyle( string textFont, string symbolFont, uint bgColor, uint layerBgColor, uint layerBorderColor, uint textColor, uint buttonBGColor, uint buttonTextColor )
        {
            // setup the text fonts and colors
            ResultSymbol.SetFont( symbolFont, 48 );
            ResultSymbol.TextColor = textColor;

            ResultLabel.SetFont( textFont, 14 );
            ResultLabel.TextColor = textColor;

            StatusLabel.SetFont( textFont, 14 );
            StatusLabel.TextColor = textColor;

            DoneButton.SetFont( textFont, 14 );
            DoneButton.TextColor = buttonTextColor;

            // setup the background layer colors
            ResultBackground.BackgroundColor = layerBgColor;
            ResultBackground.BorderColor = layerBorderColor;

            StatusBackground.BackgroundColor = layerBgColor;
            StatusBackground.BorderColor = layerBorderColor;

            View.BackgroundColor = bgColor;

            DoneButton.BackgroundColor = buttonBGColor;
            DoneButton.TextColor = buttonTextColor;

            // only round the button on iOS
            #if __IOS__
            DoneButton.CornerRadius = 4;
            #endif
        }

        public void Show( string statusLabel, string resultSymbol, string resultLabel, string buttonLabel )
        {
            SetHidden( false );

            // set and position the status label
            if ( string.IsNullOrEmpty( statusLabel ) == false )
            {
                StatusBackground.Hidden = false;
            }
            else
            {
                StatusBackground.Hidden = true;
            }
            StatusLabel.Text = statusLabel;
            StatusLabel.SizeToFit( );


            // set the result symbol
            ResultSymbol.Text = resultSymbol;
            ResultSymbol.SizeToFit( );


            // set the result text
            if ( string.IsNullOrEmpty( resultLabel ) == false )
            {
                ResultBackground.Hidden = false;
            }
            else
            {
                ResultBackground.Hidden = true;
            }
            ResultLabel.Text = resultLabel;

            // set the done button
            if ( string.IsNullOrEmpty( buttonLabel ) == false )
            {
                DoneButton.Hidden = false;
            }
            else
            {
                DoneButton.Hidden = true;
            }
            DoneButton.Text = buttonLabel;
            DoneButton.SizeToFit( );

            SetOpacity( 1.00f );

            SetBounds( View.Bounds );
        }

        public void Hide( )
        {
            SetHidden( true );

            SetOpacity( 0.00f );
        }

        public void SetBounds( RectangleF containerBounds )
        {
            View.Bounds = containerBounds;

            // setup the background layers
            StatusBackground.Frame = new RectangleF( View.Frame.X, 
                View.Frame.Top + Rock.Mobile.Graphics.Util.UnitToPx( 10 ), 
                View.Frame.Width, 
                Rock.Mobile.Graphics.Util.UnitToPx( 44 ) );

            ResultBackground.Frame = new RectangleF( View.Frame.X, 
                View.Frame.Height / 3, 
                View.Frame.Width, 
                Rock.Mobile.Graphics.Util.UnitToPx( 150 ) );
            

            // and the labels
            StatusLabel.Frame = new RectangleF( 0, 0, View.Frame.Width - Rock.Mobile.Graphics.Util.UnitToPx( 40 ), 0 );
            StatusLabel.SizeToFit( );
            StatusLabel.Frame = new RectangleF( ( View.Frame.Width - StatusLabel.Frame.Width ) / 2, 
                StatusBackground.Frame.Top + (( StatusBackground.Frame.Height - StatusLabel.Frame.Height ) / 2), 
                StatusLabel.Frame.Width, 
                StatusLabel.Frame.Height );
            
            ResultSymbol.Frame = new RectangleF( 0, 0, View.Frame.Width - Rock.Mobile.Graphics.Util.UnitToPx( 40 ), 0 );
            ResultSymbol.SizeToFit( );
            ResultSymbol.Frame = new RectangleF( ( View.Frame.Width - ResultSymbol.Frame.Width ) / 2, 
                ResultBackground.Frame.Top + Rock.Mobile.Graphics.Util.UnitToPx( 10 ), 
                ResultSymbol.Frame.Width, 
                ResultSymbol.Frame.Height );

            ResultLabel.Frame = new RectangleF( 0, 0, ResultBackground.Frame.Width - Rock.Mobile.Graphics.Util.UnitToPx( 40 ), 0 );
            ResultLabel.SizeToFit( );
            ResultLabel.Frame = new RectangleF( ( View.Frame.Width - ResultLabel.Frame.Width ) / 2, ResultSymbol.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 25 ), ResultLabel.Frame.Width, ResultLabel.Frame.Height );

            // lastly the button
            float doneWidth = Rock.Mobile.Graphics.Util.UnitToPx( 122 );
            DoneButton.Frame = new RectangleF( ( View.Frame.Width - doneWidth ) / 2, ResultBackground.Frame.Bottom + Rock.Mobile.Graphics.Util.UnitToPx( 10 ), doneWidth, DoneButton.Frame.Height );
        }

        void SetHidden( bool hidden )
        {
            View.Hidden = hidden;
            DoneButton.Hidden = hidden;

            StatusLabel.Hidden = hidden;
            StatusBackground.Hidden = hidden;

            ResultSymbol.Hidden = hidden;
            ResultLabel.Hidden = hidden;
            ResultBackground.Hidden = hidden;
        }
    }
}
