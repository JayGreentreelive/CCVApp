using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Drawing;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformCommon;
using Rock.Mobile.PlatformUI;

namespace iOS
{
    /// <summary>
    /// A custom toolbar used for navigation within activities.
    /// Activities may change buttons according to their needs.
    /// </summary>
    public class NavToolbar : UIToolbar
    {
        /// <summary>
        /// The button used to go back a page in an task.
        /// </summary>
        /// <value>The back button.</value>
        UIButton BackButton { get; set; }
        EventHandler BackButtonHandler { get; set; }

        /// <summary>
        /// Button used when an task wishes to let the user share something
        /// </summary>
        /// <value>The share button.</value>
        UIButton ShareButton { get; set; }
        EventHandler ShareButtonHandler { get; set; }

        /// <summary>
        /// Button used when a task wishes to let the user create content (like a new prayer)
        /// </summary>
        /// <value>The create button.</value>
        UIButton CreateButton { get; set; }
        EventHandler CreateButtonHandler { get; set; }

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
            NSString backLabel = new NSString(SubNavToolbarConfig.BackButton_Text);

            BackButton = new UIButton(UIButtonType.System);
            BackButton.Font = iOSCommon.LoadFontDynamic( SubNavToolbarConfig.BackButton_Font, SubNavToolbarConfig.BackButton_Size );
            BackButton.SetTitle( backLabel.ToString( ), UIControlState.Normal );
            BackButton.SetTitleColor( PlatformBaseUI.GetUIColor( SubNavToolbarConfig.BackButton_EnabledColor ), UIControlState.Normal );
            BackButton.SetTitleColor( PlatformBaseUI.GetUIColor( SubNavToolbarConfig.BackButton_DisabledColor ), UIControlState.Disabled );

            SizeF buttonSize = backLabel.StringSize( BackButton.Font );
            BackButton.Bounds = new RectangleF( 0, 0, buttonSize.Width, buttonSize.Height );
            //BackButton.BackgroundColor = UIColor.White;

            // create the share button
            NSString shareLabel = new NSString(SubNavToolbarConfig.ShareButton_Text);

            ShareButton = new UIButton(UIButtonType.System);
            ShareButton.Font = iOSCommon.LoadFontDynamic( SubNavToolbarConfig.ShareButton_Font, SubNavToolbarConfig.ShareButton_Size );
            ShareButton.SetTitle( shareLabel.ToString( ), UIControlState.Normal );
            ShareButton.SetTitleColor( PlatformBaseUI.GetUIColor( SubNavToolbarConfig.ShareButton_EnabledColor ), UIControlState.Normal );
            ShareButton.SetTitleColor( PlatformBaseUI.GetUIColor( SubNavToolbarConfig.ShareButton_DisabledColor ), UIControlState.Disabled );

            // determine its dimensions
            buttonSize = shareLabel.StringSize( ShareButton.Font );
            ShareButton.Bounds = new RectangleF( 0, 0, buttonSize.Width, buttonSize.Height );
            //ShareButton.BackgroundColor = UIColor.White;


            // create the create button
            NSString createLabel = new NSString(SubNavToolbarConfig.CreateButton_Text);

            CreateButton = new UIButton(UIButtonType.System);
            CreateButton.Font = iOSCommon.LoadFontDynamic( SubNavToolbarConfig.CreateButton_Font, SubNavToolbarConfig.CreateButton_Size );
            CreateButton.SetTitle( createLabel.ToString( ), UIControlState.Normal );
            CreateButton.SetTitleColor( PlatformBaseUI.GetUIColor( SubNavToolbarConfig.CreateButton_EnabledColor ), UIControlState.Normal );
            CreateButton.SetTitleColor( PlatformBaseUI.GetUIColor( SubNavToolbarConfig.CreateButton_DisabledColor ), UIControlState.Disabled );

            // determine its dimensions
            buttonSize = createLabel.StringSize( CreateButton.Font );
            CreateButton.Bounds = new RectangleF( 0, 0, buttonSize.Width, buttonSize.Height );
            //CreateButton.BackgroundColor = UIColor.White;

            UpdateButtons( );
        }

        public void SetBackButtonAction( EventHandler handler )
        {
            if ( BackButtonHandler != null )
            {
                BackButton.TouchUpInside -= BackButtonHandler;
            }

            BackButton.TouchUpInside += handler;

            BackButtonHandler = handler;
        }

        public void SetBackButtonEnabled( bool enabled )
        {
            BackButton.Enabled = enabled;
        }

        public void SetShareButtonEnabled( bool enabled, EventHandler handler = null )
        {
            ShareButton.Enabled = enabled;

            if ( ShareButtonHandler != null )
            {
                ShareButton.TouchUpInside -= ShareButtonHandler; 
            }

            if( handler != null )
            {
                ShareButton.TouchUpInside += handler;
            }

            ShareButtonHandler = handler;
        }

        public void SetCreateButtonEnabled( bool enabled, EventHandler handler = null )
        {
            CreateButton.Enabled = enabled;

            if ( CreateButtonHandler != null )
            {
                CreateButton.TouchUpInside -= CreateButtonHandler; 
            }

            if( handler != null )
            {
                CreateButton.TouchUpInside += handler;
            }

            CreateButtonHandler = handler;
        }

        void UpdateButtons( )
        {
            // This sets the valid buttons TO the toolbar.
            // Since an task could request one, the other, or both,
            // we build a list and then add that list to the toolbar.
            List<UIBarButtonItem> itemList = new List<UIBarButtonItem>( );

            UIBarButtonItem spacer = new UIBarButtonItem( UIBarButtonSystemItem.FixedSpace );
            spacer.Width = SubNavToolbarConfig.iOS_ButtonSpacing;

            itemList.Add( new UIBarButtonItem( BackButton ) );
            itemList.Add( spacer );
            itemList.Add( new UIBarButtonItem( ShareButton ) );
            itemList.Add( spacer );
            itemList.Add( new UIBarButtonItem( CreateButton ) );

            // for some reason, it will not accept a new array of items
            // until we clear the existing.
            SetItems( new UIBarButtonItem[0], false );

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
                    UIView.Animate( SubNavToolbarConfig.SlideRate, 0, UIViewAnimationOptions.CurveEaseInOut, 
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
    