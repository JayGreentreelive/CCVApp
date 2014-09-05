using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Drawing;

namespace iOS
{
    /// <summary>
    /// A custom toolbar used for navigation within activities.
    /// Activities may change buttons according to their needs.
    /// </summary>
    public class NavToolbar : UIToolbar
    {
        // JHM: Don't think we need a mutex because we're simply invoking on the main thread,
        // therefore there cannot be a race condition, since everything gets serialized to a single thread.

        /// <summary>
        /// The button used to go back a page in an task.
        /// </summary>
        /// <value>The back button.</value>
        UIButton BackButton { get; set; }

        /// <summary>
        /// True if the back button should exist on the toolbar. False if not.
        /// </summary>
        /// <value><c>true</c> if back button displayed; otherwise, <c>false</c>.</value>
        bool BackButtonDisplayed { get; set; }

        /// <summary>
        /// Button used when an task wishes to let the user share something
        /// </summary>
        /// <value>The share button.</value>
        UIBarButtonItem ShareButton { get; set; }

        /// <summary>
        /// True when this toolbar is showing. False when it is hidden.
        /// </summary>
        /// <value><c>true</c> if revealed; otherwise, <c>false</c>.</value>
        bool Revealed { get; set; }

        /// <summary>
        /// True when the toolbar is in the process of sliding up or down.
        /// </summary>
        /// <value><c>true</c> if animating; otherwise, <c>false</c>.</value>
        bool Animating { get; set; }

        /// <summary>
        /// Timer monitoring the time the toolbar should be shown before auto-hiding.
        /// </summary>
        /// <value>The nav bar timer.</value>
        protected System.Timers.Timer NavBarTimer { get; set; }

        public NavToolbar( ) : base()
        {
            // create a timer that can be used to autohide this toolbar.
            NavBarTimer = new System.Timers.Timer();
            NavBarTimer.AutoReset = false;
            NavBarTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                {
                    // when the timer fires, hide the toolbar.
                    // Although the timer fires on a seperate thread, because we queue the reveal
                    // on the main (UI) thread, we don't have to worry about race conditions.
                    Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate { Reveal( false ); } );
                };


            // create the back button
            NSString buttonLabel = new NSString(CCVApp.Shared.Config.SubNavToolbar.BackButton_Text);

            BackButton = new UIButton(UIButtonType.System);
            BackButton.Font = Rock.Mobile.PlatformCommon.iOS.LoadFontDynamic( CCVApp.Shared.Config.SubNavToolbar.BackButton_Font, CCVApp.Shared.Config.SubNavToolbar.BackButton_Size );
            BackButton.SetTitle( buttonLabel.ToString( ), UIControlState.Normal );
            BackButton.SetTitleColor( Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.SubNavToolbar.BackButton_EnabledColor ), UIControlState.Normal );
            BackButton.SetTitleColor( Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.SubNavToolbar.BackButton_DisabledColor ), UIControlState.Disabled );

            // determine its dimensions
            SizeF buttonSize = buttonLabel.StringSize( BackButton.Font );
            BackButton.Bounds = new RectangleF( 0, 0, buttonSize.Width, buttonSize.Height );
        }

        public void DisplayBackButton( bool display, EventHandler handler )
        {
            BackButtonDisplayed = display;

            BackButton.TouchUpInside += handler;

            UpdateButtons( );
        }

        public void SetBackButtonEnabled( bool enabled )
        {
            BackButton.Enabled = enabled;
        }

        void UpdateButtons( )
        {
            // This sets the valid buttons TO the toolbar.
            // Since an task could request one, the other, or both,
            // we build a list and then add that list to the toolbar.
            List<UIBarButtonItem> itemList = new List<UIBarButtonItem>( );

            if( BackButtonDisplayed == true )
            {
                itemList.Add( new UIBarButtonItem( BackButton ) );
            }

            if( ShareButton != null )
            {
                itemList.Add( ShareButton );
            }

            SetItems( itemList.ToArray( ), false );
        }

        public void RevealForTime( float timeToShow )
        {
            // stop (reset) any current timer
            NavBarTimer.Stop( );

            // convert to milliseconds
            NavBarTimer.Interval = timeToShow * 1000;

            // start the timer
            NavBarTimer.Start( );

            // reveal the toolbar, and when the timer ticks, it will be hidden again.
            Reveal( true );
        }

        public void Reveal( bool revealed )
        {
            if( Revealed != revealed )
            {
                // of course don't allow a change while we're animating it.
                if( Animating == false )
                {
                    Animating = true;

                    // Animate the front panel out
                    UIView.Animate( CCVApp.Shared.Config.SubNavToolbar.SlideRate, 0, UIViewAnimationOptions.CurveEaseInOut, 
                        new NSAction( 
                            delegate 
                            { 
                                float deltaPosition = revealed ? -Frame.Height : Frame.Height;

                                Layer.Position = new PointF( Layer.Position.X, Layer.Position.Y + deltaPosition);
                            })

                        , new NSAction(
                            delegate
                            {
                                Animating = false;

                                Revealed = revealed;
                            })
                    );
                }
            }
        }
    }
}
    