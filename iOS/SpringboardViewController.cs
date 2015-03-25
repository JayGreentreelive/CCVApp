using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreAnimation;
using CoreGraphics;
using System.Collections.Generic;
using Rock.Mobile.Network;
using CCVApp.Shared.Network;
using AssetsLibrary;
using System.IO;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;
using Rock.Mobile.PlatformSpecific.iOS.Graphics;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using CCVApp.Shared;
using Rock.Mobile.Animation;

namespace iOS
{
    /// <summary>
    /// The springboard acts as the core navigation for the user. From here
    /// they may launch any of the app's activities.
    /// </summary>
	partial class SpringboardViewController : UIViewController
	{
        /// <summary>
        /// Represents a selectable element on the springboard.
        /// Contains its button and the associated task.
        /// </summary>
        protected class SpringboardElement
        {
            /// <summary>
            /// The task that is launched by this element.
            /// </summary>
            /// <value>The task.</value>
            public Task Task { get; set; }

            /// <summary>
            /// The view that rests behind the button, graphic and text, and is colored when 
            /// the task is active. It is the parent for the button, text logo and seperator
            /// </summary>
            /// <value>The backing view.</value>
            UIView BackingView { get; set; }

            /// <summary>
            /// The button itself. Because we have special display needs, we
            /// break the button apart, and this ends up being an empty container that lies
            /// on top of the BackingView, LogoView and TextView.
            /// </summary>
            /// <value>The button.</value>
            UIButton Button { get; set; }

            UILabel TextLabel { get; set; }

            UILabel LogoView { get; set; }

            UIView Seperator { get; set; }

            public SpringboardElement( SpringboardViewController controller, Task task, UIView backingView, string imageChar, string labelStr )
            {
                Task = task;

                // setup the backing view
                BackingView = backingView;
                BackingView.BackgroundColor = UIColor.Clear;

                //The button should look as follows:
                // [ X Text ]
                // To make sure the icons and text are all aligned vertically,
                // we will actually create a backing view that can highlight (the []s)
                // and place a logo view (the X), and a text view (the Text) on top.
                // Finally, we'll make the button clear with no text and place it over the
                // backing view.

                // Create the logo view containing the image.
                LogoView = new UILabel();
                LogoView.Text = imageChar;
                LogoView.Font = FontManager.GetFont( ControlStylingConfig.Icon_Font_Primary, SpringboardConfig.Element_FontSize );
                LogoView.SizeToFit( );
                LogoView.BackgroundColor = UIColor.Clear;
                BackingView.AddSubview( LogoView );

                // Create the text, and populate it with the button's requested text, color and font.
                TextLabel = new UILabel();
                TextLabel.Text = labelStr;
                TextLabel.Font = FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
                TextLabel.BackgroundColor = UIColor.Clear;
                TextLabel.SizeToFit( );
                BackingView.AddSubview( TextLabel );

                // Create the seperator
                Seperator = new UIView( );
                Seperator.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                BackingView.AddSubview( Seperator );

                // Create the button
                Button = new UIButton( UIButtonType.Custom );
                Button.Layer.AnchorPoint = CGPoint.Empty;
                Button.BackgroundColor = UIColor.Clear;
                Button.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        controller.ActivateElement( this );
                    };
                BackingView.AddSubview( Button );


                // position the controls
                Button.Bounds = BackingView.Bounds;

                LogoView.Layer.Position = new CGPoint( SpringboardConfig.Element_LogoOffsetX, BackingView.Frame.Height / 2 );

                TextLabel.Layer.Position = new CGPoint( SpringboardConfig.Element_LabelOffsetX + ( TextLabel.Frame.Width / 2 ), BackingView.Frame.Height / 2 );

                Seperator.Frame = new CGRect( 0, 0, Button.Frame.Width, 1.0f );

                Deactivate( );
            }

            public void Activate( )
            {
                LogoView.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Springboard_ActiveElementColor );
                TextLabel.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Springboard_ActiveElementColor );
                BackingView.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( SpringboardConfig.Element_SelectedColor );
            }

            public void Deactivate( )
            {
                LogoView.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementColor );
                TextLabel.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementColor );
                BackingView.BackgroundColor = UIColor.Clear;
            }
        };

        /// <summary>
        /// A list of all the elements on the springboard page.
        /// </summary>
        /// <value>The elements.</value>
        protected List<SpringboardElement> Elements { get; set; }

        /// <summary>
        /// The primary navigation for activities.
        /// </summary>
        /// <value>The nav view controller.</value>
        protected MainUINavigationController NavViewController { get; set; }

        /// <summary>
        /// Storyboard for the user management area of the app (Login, Profile, etc)
        /// </summary>
        /// <value>The user management storyboard.</value>
        protected UIStoryboard UserManagementStoryboard { get; set; }

        /// <summary>
        /// Controller managing a user logging in or out
        /// </summary>
        /// <value>The login view controller.</value>
        protected LoginViewController LoginViewController { get; set; }

        /// <summary>
        /// Controller for managing user registration
        /// </summary>
        /// <value>The register view controller.</value>
        protected RegisterViewController RegisterViewController { get; set; }

        /// <summary>
        /// Controller managing the user's profile. Lets a user view or edit their profile.
        /// </summary>
        /// <value>The profile view controller.</value>
        protected ProfileViewController ProfileViewController { get; set; }

        /// <summary>
        /// Controller used for copping an image to our requirements (1:1 aspect ratio)
        /// </summary>
        /// <value>The image crop view controller.</value>
        protected ImageCropViewController ImageCropViewController { get; set; }

        /// <summary>
        /// The out of box experience view controller, used for first time setup.
        /// </summary>
        /// <value>The OOBE view controller.</value>
        protected OOBEViewController OOBEViewController { get; set; }

        /// <summary>
        /// True while the user is still being guided through the OOBE. This includes
        /// the view controllers the OOBE launches, like Login and Register
        /// </summary>
        /// <value><c>true</c> if this instance is OOBE running; otherwise, <c>false</c>.</value>
        bool IsOOBERunning { get; set; }

        /// <summary>
        /// When true, we are doing something else, like logging in, editing the profile, etc.
        /// </summary>
        /// <value><c>true</c> if modal controller visible; otherwise, <c>false</c>.</value>
        protected bool ModalControllerVisible { get; set; } 

        /// <summary>
        /// When true, we need to launch the image cropper. We have to wait
        /// until the NavBar and all sub-fragments have been pushed to the stack.
        /// </summary>
        UIImage ImageCropperPendingImage { get; set; }

        /// <summary>
        /// Stores the profile picture that is placed on the "Login Button" when the user is logged in.
        /// We use this because setting an image on a button via SetImage causes the button to size to the image,
        /// even with ContentMode set.
        /// </summary>
        /// <value>The profile image view.</value>
        UIImageView ProfileImageView { get; set; }

        /// <summary>
        /// A seperator that goes at the bottom of the Springboard Element List
        /// </summary>
        /// <value>The bottom seperator.</value>
        UIView BottomSeperator { get; set; }

        UILabel CampusSelectionText { get; set; }
        UILabel CampusSelectionIcon { get; set; }
        UIButton CampusSelectionButton { get; set; }

        NotificationBillboard Billboard { get; set; }

        /// <summary>
        /// True when the series info has been downloaded and it's safe to show the notification billboard.
        /// </summary>
        bool SeriesInfoDownloaded { get; set; }

        /// <summary>
        /// Stores the time of the last rock sync.
        /// If the user has left our app running > 24 hours we'll redownload
        /// </summary>
        /// <value>The last rock sync.</value>
        DateTime LastRockSync { get; set; }

		public SpringboardViewController (IntPtr handle) : base (handle)
		{
            UserManagementStoryboard = UIStoryboard.FromName( "UserManagement", null );

            NavViewController = Storyboard.InstantiateViewController( "MainUINavigationController" ) as MainUINavigationController;
            NavViewController.ParentSpringboard = this;

            Elements = new List<SpringboardElement>( );
		}

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            // phones should prefer a portrait mode
            if ( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone )
            {
                return UIInterfaceOrientation.Portrait;
            }
            else
            {
                // and ipads should prefer landscape.
                return UIInterfaceOrientation.LandscapeLeft;
            }
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override bool ShouldAutomaticallyForwardRotationMethods
        {
            get
            {
                return false;
            }
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            // let the task decide what orientations we support if the springboard is closed.
            if ( NavViewController.CurrentTask != null && NavViewController.IsSpringboardClosed( ) )
            {
                return NavViewController.CurrentTask.GetSupportedInterfaceOrientations( );
            }
            else
            {
                if ( IsDeviceLandscape( ) == true && IsLandscapeRegular( ) == true )
                {
                    return UIInterfaceOrientationMask.All;
                }
                else
                {
                    return UIInterfaceOrientationMask.Portrait;
                }
            }
        }

        static UITraitCollection CurrentTraitCollection { get; set; }

        static public CGSize TraitSize { get; protected set; }

        public override void WillTransitionToTraitCollection(UITraitCollection traitCollection, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.WillTransitionToTraitCollection(traitCollection, coordinator);

            CurrentTraitCollection = traitCollection;
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize( toSize, coordinator );

            TraitSize = toSize;

            if ( NavViewController != null )
            {
                NavViewController.LayoutChanging( );
            }
        }

        public static void ForcePortaitModeHack( )
        {
            if ( TraitSize.Width > TraitSize.Height )
            {
                CGSize currSize = TraitSize;

                TraitSize = new CGSize( currSize.Height, currSize.Width );
            }
        }

        public static bool IsLandscapeRegular( )
        {
            if ( IsDeviceLandscape( ) && CurrentTraitCollection.HorizontalSizeClass == UIUserInterfaceSizeClass.Regular )
            {
                return true;
            }

            return false;
        }

        public static bool IsDeviceLandscape( )
        {
            if ( TraitSize.Width > TraitSize.Height )
            {
                return true;
            }
            return false;
        }

        public static bool IsDevicePortrait( )
        {
            if ( TraitSize.Width < TraitSize.Height )
            {
                return true;
            }

            return false;
        }

        /*public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            base.WillRotate(toInterfaceOrientation, duration);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
        }*/

        /*public static void ForceDeviceToOrientation( UIInterfaceOrientation toOrientation )
        {
            // amazingly, this works.
            NSNumber value = NSNumber.FromInt32( (int)UIInterfaceOrientation.Portrait );
            UIDevice.CurrentDevice.SetValueForKey( value, new NSString( "orientation" ) );
        }*/

        public void RevealButtonClicked( )
        {
            // this will be called by the Navbar (which owns the reveal button) when
            // it's clicked. We want to make sure we alwas hide the billboard.
            Billboard.Hide( );
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad( );

            // seed the trait size with our current window size
            TraitSize = UIScreen.MainScreen.Bounds.Size;

            // create the login controller / profile view controllers
            LoginViewController = UserManagementStoryboard.InstantiateViewController( "LoginViewController" ) as LoginViewController;
            LoginViewController.Springboard = this;

            ProfileViewController = UserManagementStoryboard.InstantiateViewController( "ProfileViewController" ) as ProfileViewController;
            ProfileViewController.Springboard = this;

            ImageCropViewController = UserManagementStoryboard.InstantiateViewController( "ImageCropViewController" ) as ImageCropViewController;
            ImageCropViewController.Springboard = this;

            RegisterViewController = UserManagementStoryboard.InstantiateViewController( "RegisterViewController" ) as RegisterViewController;
            RegisterViewController.Springboard = this;

            OOBEViewController = new OOBEViewController( );
            OOBEViewController.Springboard = this;

            // Instantiate all activities
            Elements.Add( new SpringboardElement( this, new NewsTask( "NewsStoryboard_iPhone" )      , NewsElement    , SpringboardConfig.Element_News_Icon    , SpringboardStrings.Element_News_Title ) );
            Elements.Add( new SpringboardElement( this, new ConnectTask( "ConnectStoryboard_iPhone" ), ConnectElement , SpringboardConfig.Element_Connect_Icon , SpringboardStrings.Element_Connect_Title ) );
            Elements.Add( new SpringboardElement( this, new NotesTask( "NotesStoryboard_iPhone" )    , MessagesElement, SpringboardConfig.Element_Messages_Icon, SpringboardStrings.Element_Messages_Title ) );
            Elements.Add( new SpringboardElement( this, new PrayerTask( "PrayerStoryboard_iPhone" )  , PrayerElement  , SpringboardConfig.Element_Prayer_Icon  , SpringboardStrings.Element_Prayer_Title ) );
            Elements.Add( new SpringboardElement( this, new GiveTask( "GiveStoryboard_iPhone" )      , GiveElement    , SpringboardConfig.Element_Give_Icon    , SpringboardStrings.Element_Give_Title ) );
            Elements.Add( new SpringboardElement( this, new AboutTask( "AboutStoryboard_iPhone" )    , MoreElement    , SpringboardConfig.Element_More_Icon    , SpringboardStrings.Element_More_Title ) );

            // add a bottom seperator for the final element
            BottomSeperator = new UIView();
            BottomSeperator.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
            View.AddSubview( BottomSeperator );
            BottomSeperator.Frame = new CGRect( 0, 0, View.Frame.Width, 1.0f );


            // set the profile image mask so it's circular
            CALayer maskLayer = new CALayer();
            maskLayer.AnchorPoint = new CGPoint( 0, 0 );
            maskLayer.Bounds = EditPictureButton.Layer.Bounds;
            maskLayer.CornerRadius = EditPictureButton.Bounds.Width / 2;
            maskLayer.BackgroundColor = UIColor.Black.CGColor;
            EditPictureButton.Layer.Mask = maskLayer;
            //

            // setup the campus selector and settings button
            CampusSelectionText = new UILabel();
            ControlStyling.StyleUILabel( CampusSelectionText, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
            View.AddSubview( CampusSelectionText );

            CampusSelectionIcon = new UILabel();
            ControlStyling.StyleUILabel( CampusSelectionIcon, ControlStylingConfig.Icon_Font_Primary, ControlStylingConfig.Small_FontSize );
            CampusSelectionIcon.Text = SpringboardConfig.SettingsSymbol;
            CampusSelectionIcon.SizeToFit( );
            View.AddSubview( CampusSelectionIcon );

            CampusSelectionButton = new UIButton();
            View.AddSubview( CampusSelectionButton );
            CampusSelectionButton.TouchUpInside += (object sender, EventArgs e ) =>
            {
                    // use an appropriate select menu based on the device type.
                    UIAlertController actionSheet = null;
                    if( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone )
                    {
                        actionSheet = UIAlertController.Create( SpringboardStrings.SelectCampus_SourceTitle, 
                                                                SpringboardStrings.SelectCampus_SourceDescription, 
                                                                UIAlertControllerStyle.ActionSheet );
                    }
                    else
                    {
                        actionSheet = UIAlertController.Create( SpringboardStrings.SelectCampus_SourceTitle, 
                                                                SpringboardStrings.SelectCampus_SourceDescription, 
                                                                UIAlertControllerStyle.Alert );
                    }

                    // for each campus, create an entry in the action sheet, and its callback will assign
                    // that campus index to the user's viewing preference
                    for( int i = 0; i < RockGeneralData.Instance.Data.Campuses.Count; i++ )
                    {
                        UIAlertAction campusAction = UIAlertAction.Create( RockGeneralData.Instance.Data.Campuses[ i ].Name, UIAlertActionStyle.Default, delegate(UIAlertAction obj) 
                            {
                                //get the index of the campus based on the selection's title, and then set that campus title as the string
                                RockMobileUser.Instance.ViewingCampus = RockGeneralData.Instance.Data.CampusNameToId( obj.Title );

                                RefreshCampusSelection( );
                        } );

                        actionSheet.AddAction( campusAction );
                    }

                    PresentViewController( actionSheet, true, null );
            };


            // setup the image that will display when the user is logged in
            ProfileImageView = new UIImageView( );
            ProfileImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            ProfileImageView.Layer.AnchorPoint = CGPoint.Empty;
            ProfileImageView.Bounds = EditPictureButton.Bounds;
            ProfileImageView.Layer.Position = CGPoint.Empty;
            EditPictureButton.AddSubview( ProfileImageView );

            EditPictureButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Icon_Font_Primary, SpringboardConfig.ProfileSymbolFontSize );
            EditPictureButton.SetTitleColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ), UIControlState.Normal );
            EditPictureButton.Layer.BorderColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ).CGColor;
            EditPictureButton.Layer.CornerRadius = EditPictureButton.Bounds.Width / 2;
            EditPictureButton.Layer.BorderWidth = 4;

            WelcomeField.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Large_Font_Light, ControlStylingConfig.Large_FontSize );
            WelcomeField.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );

            UserNameField.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Large_Font_Bold, ControlStylingConfig.Large_FontSize );
            UserNameField.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );

            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.SpringboardBackgroundColor );

            AddChildViewController( NavViewController );
            View.AddSubview( NavViewController.View );

            SetNeedsStatusBarAppearanceUpdate( );

            EditPictureButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // don't allow launching a model view controller unless the springboard is open.
                    if ( NavViewController.IsSpringboardOpen( ) )
                    {
                        if( RockMobileUser.Instance.LoggedIn == true )
                        {
                            // they're logged in, so let them set their profile pic
                            ManageProfilePic( );
                        }
                        else
                        {
                            //otherwise this button can double as a login button.
                            PresentModalViewController( LoginViewController );
                        }
                    }
                };

            ViewProfileButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // don't allow launching a model view controller unless the springboard is open.
                    if ( NavViewController.IsSpringboardOpen( ) )
                    {
                        if( RockMobileUser.Instance.LoggedIn == true )
                        {
                            PresentModalViewController( ProfileViewController );
                        }
                        else
                        {
                            PresentModalViewController( LoginViewController );
                        }
                    }
                };

            // load our objects from disk
            Console.WriteLine( "Loading objects from device." );
            RockApi.Instance.LoadObjectsFromDevice( );
            Console.WriteLine( "Loading objects done." );

            // set the viewing campus now that their profile has loaded
            CampusSelectionText.Text = string.Format( SpringboardStrings.Viewing_Campus, RockGeneralData.Instance.Data.CampusIdToName( RockMobileUser.Instance.ViewingCampus ) );
            CampusSelectionText.SizeToFit( );

            // seed the last sync time with now, so that when OnActivated gets called we don't do it again.
            LastRockSync = DateTime.Now;

            SyncRockData( );

            // setup the Notification Banner for Taking Notes
            Billboard = new NotificationBillboard( View.Bounds.Width, View.Bounds.Height );
            Billboard.SetLabel( SpringboardStrings.TakeNotesNotificationIcon, 
                                ControlStylingConfig.Icon_Font_Primary,
                                ControlStylingConfig.Small_FontSize,
                                SpringboardStrings.TakeNotesNotificationLabel, 
                                ControlStylingConfig.Small_Font_Light,
                                ControlStylingConfig.Small_FontSize,
                                ControlStylingConfig.TextField_ActiveTextColor, 
                                SpringboardConfig.Element_SelectedColor, 
                delegate 
                {
                    // find the Notes task, activate it, and tell it to jump to the read page.
                    foreach( SpringboardElement element in Elements )
                    {
                        if ( element.Task as NotesTask != null )
                        {
                            ActivateElement( element, true );
                            PerformTaskAction( "Page.Read" );
                        }
                    }
                } 
            );

            Billboard.Layer.Position = new CGPoint( Billboard.Layer.Position.X, NavViewController.NavigationBar.Frame.Height );


            // only do the OOBE if the user hasn't seen it yet
            if ( RockMobileUser.Instance.OOBEComplete == false )
            //if( RanOOBE == false )
            {
                // sanity check for testers that didn't listen to me and delete / reinstall.
                // This will force them to be logged out so they experience the OOBE properly.
                RockMobileUser.Instance.LogoutAndUnbind( );
                
                //RanOOBE = true;
                IsOOBERunning = true;
                AddChildViewController( OOBEViewController );
                View.AddSubview( OOBEViewController.View );
            }
        }

        //static bool RanOOBE = false;
        public void OOBEOnClick( int index )
        {
            // fade out the OOBE
            SimpleAnimator_Float oobeFadeOutAnim = new SimpleAnimator_Float( 1.00f, 0.00f, .33f, delegate(float percent, object value )
                {
                    OOBEViewController.View.Layer.Opacity = (float)value;
                },
                delegate
                {
                    // if they chose register, present it
                    if ( index == 0 )
                    {
                        PresentModalViewController( RegisterViewController );
                    }
                    // if they chose login, present it!
                    else if ( index == 1 )
                    {
                        PresentModalViewController( LoginViewController );
                    }
                    else
                    {
                        // don't present anything. Instead, just wrap up the OOBE.
                        CompleteOOBE( );
                    }

                    OOBEViewController.RemoveFromParentViewController( );
                    OOBEViewController.View.RemoveFromSuperview( );
                } );
            oobeFadeOutAnim.Start( );
        }

        void CompleteOOBE( )
        {
            // kick off a timer to allow the user to see the news before revealing the springboard.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = false;
            timer.Interval = 500;
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                {
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            // then reveal the springboard
                            NavViewController.RevealSpringboard( true );
                            IsOOBERunning = false;
                            RockMobileUser.Instance.OOBEComplete = true;

                            TryDisplaySeriesBillboard( );
                        } );
                };
            timer.Start( );
        }

        void SyncRockData( )
        {
            SeriesInfoDownloaded = false;

            CCVApp.Shared.Network.RockNetworkManager.Instance.SyncRockData( 
                // first delegate is for completion of the series download. At that point we can show the notification billboard.
                delegate 
                {
                    SeriesInfoDownloaded = true;

                    TryDisplaySeriesBillboard( );
                },
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // here we know whether the initial handshake with Rock went ok or not
                    LastRockSync = DateTime.Now;

                    // we now have the latest news and all initial downloading.
                    // Tell the news it's safe to reload.
                    PerformTaskAction( "News.Reload" );
                    PerformTaskAction( "Notes.DownloadImages" );
                });
        }

        void PerformTaskAction( string action )
        {
            // notify all elements
            foreach ( SpringboardElement element in Elements )
            {
                element.Task.PerformAction( action );
            }
        }

        void ManageProfilePic( )
        {
            UIAlertController actionSheet = null;
            if ( UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone )
            {
                actionSheet = UIAlertController.Create( SpringboardStrings.ProfilePicture_SourceTitle, 
                    SpringboardStrings.ProfilePicture_SourceDescription, 
                    UIAlertControllerStyle.ActionSheet );
            }
            else
            {
                actionSheet = UIAlertController.Create( SpringboardStrings.ProfilePicture_SourceTitle, 
                    SpringboardStrings.ProfilePicture_SourceDescription, 
                    UIAlertControllerStyle.Alert );
            }

            // setup the camera
            UIAlertAction cameraAction = UIAlertAction.Create( SpringboardStrings.ProfilePicture_SourceCamera, UIAlertActionStyle.Default, delegate(UIAlertAction obj) 
                {
                    // only allow the camera if they HAVE one
                    if( Rock.Mobile.Media.PlatformCamera.Instance.IsAvailable( ) )
                    {
                        ModalControllerVisible = true;

                        // launch the camera
                        string jpgFilename = System.IO.Path.Combine ( Environment.GetFolderPath(Environment.SpecialFolder.Personal), "cameraTemp.jpg" );
                        Rock.Mobile.Media.PlatformCamera.Instance.CaptureImage( jpgFilename, this, delegate(object s, Rock.Mobile.Media.PlatformCamera.CaptureImageEventArgs args) 
                            {
                                ModalControllerVisible = false;

                                // if the result is true, they either got a picture or pressed cancel
                                bool success = false;
                                if( args.Result == true )
                                {
                                    // either way, no need for an error
                                    success = true;

                                    // if the image path is valid, they didn't cancel
                                    if ( string.IsNullOrEmpty( args.ImagePath ) == false )
                                    {
                                        // load the image for cropping
                                        ImageCropperPendingImage = UIImage.FromFile( args.ImagePath );
                                    }
                                }

                                if( success == false )
                                {
                                    DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                                }
                            });
                    }
                    else
                    {
                        // notify them they don't have a camera
                        DisplayError( SpringboardStrings.Camera_Error_Title, SpringboardStrings.Camera_Error_Message );
                    }
                } );

            // setup the photo library
            UIAlertAction photoLibraryAction = UIAlertAction.Create( SpringboardStrings.ProfilePicture_SourcePhotoLibrary, UIAlertActionStyle.Default, delegate(UIAlertAction obj) 
                {
                    ModalControllerVisible = true;

                    Rock.Mobile.Media.PlatformImagePicker.Instance.PickImage( this, delegate(object s, Rock.Mobile.Media.PlatformImagePicker.ImagePickEventArgs args) 
                        {
                            ModalControllerVisible = false;

                            if( args.Result == true )
                            {
                                ImageCropperPendingImage = (UIImage) args.Image;
                            }
                            else
                            {
                                DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                            }
                        } );
                } );

            //setup cancel
            UIAlertAction cancelAction = UIAlertAction.Create( GeneralStrings.Cancel, UIAlertActionStyle.Default, delegate{ } );

            actionSheet.AddAction( cameraAction );
            actionSheet.AddAction( photoLibraryAction );
            actionSheet.AddAction( cancelAction );
            PresentViewController( actionSheet, true, null );
        }

        void PresentModalViewController( UIViewController modelViewController )
        {
            PresentViewController( modelViewController, true, null );
            ModalControllerVisible = true;
        }

        public void ResignModelViewController( UIViewController modelViewController, object context )
        {
            // if the image cropper is resigning
            if ( modelViewController == ImageCropViewController )
            {
                // if croppedImage is null, they simply cancelled
                UIImage croppedImage = (UIImage)context;
                if ( croppedImage != null )
                {
                    NSData croppedImageData = croppedImage.AsJPEG( );

                    // if the image converts, we're good.
                    if ( croppedImageData != null )
                    {
                        MemoryStream memStream = new MemoryStream();

                        Stream nsDataStream = croppedImageData.AsStream( );

                        nsDataStream.CopyTo( memStream );
                        memStream.Position = 0;

                        RockMobileUser.Instance.SaveProfilePicture( memStream );
                        RockMobileUser.Instance.UploadSavedProfilePicture( null ); // we don't care about the response. just do it.

                        nsDataStream.Dispose( );
                    }
                    else
                    {
                        // notify them about a problem saving the profile picture
                        DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                    }
                }
            }

            modelViewController.DismissViewController( true, delegate 
                {
                    // if this resign is while the OOBE is running, it was the register or login finishing up, 
                    // so wrap up the OOBE
                    if ( IsOOBERunning == true )
                    {
                        CompleteOOBE( );
                    }
                    ModalControllerVisible = false;

                } );
        }

        public void RegisterNewUser( )
        {
            LoginViewController.PresentViewController( RegisterViewController, true, null );
        }

        public override bool PrefersStatusBarHidden()
        {
            // don't show the status bar when running this app.
            return true;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            // only needed when we were showing the status bar. Causes
            // the status bar text to be white.
            return UIStatusBarStyle.LightContent;
        }

        protected void ActivateElement( SpringboardElement activeElement, bool forceActivate = false )
        {
            // don't allow any navigation while the login controller is active.
            // If forceOpen is enabled, we'll allow it regardless.
            if ( (ModalControllerVisible == false && NavViewController.IsSpringboardOpen( ) == true) || forceActivate == true )
            {
                // make sure we're allowed to switch activities
                if ( NavViewController.ActivateTask( activeElement.Task ) == true )
                {
                    // first turn "off" the backingView selection for all but the element
                    // becoming active.
                    foreach ( SpringboardElement element in Elements )
                    {
                        if ( element != activeElement )
                        {
                            element.Deactivate( );
                        }
                    }

                    // activate the element and its associated task
                    activeElement.Activate( );
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // don't allow any navigation while the login controller is active
            if( ModalControllerVisible == false )
            {
                NavViewController.RevealSpringboard( false );
            }
        }

        public override void ViewWillAppear( bool animated )
        {
            base.ViewWillAppear( animated );

            // refresh the viewing campus
            RefreshCampusSelection( );
        }

        void RefreshCampusSelection( )
        {
            string newCampusText = string.Format( SpringboardStrings.Viewing_Campus, 
                                                  RockGeneralData.Instance.Data.CampusIdToName( RockMobileUser.Instance.ViewingCampus ) );

            if ( CampusSelectionText.Text != newCampusText )
            {
                CampusSelectionText.Text = newCampusText;

                UpdateCampusViews( );

                // let the news know it should reload
                PerformTaskAction( "News.Reload" );
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // if the image cropper is pending, launch it now.
            if( ImageCropperPendingImage != null )
            {
                ImageCropViewController.Begin( ImageCropperPendingImage, 1.0f );
                PresentModalViewController( ImageCropViewController );

                ImageCropperPendingImage = null;
            }
            else
            {
                // if we're appearing and no task is active, start one.
                // (this will only happen when the app is first launched)
                if( NavViewController.CurrentTask == null )
                {
                    // don't use the ActivateElement method because
                    // it verifies the springboard is closed, and we don't
                    // care on first run.
                    NavViewController.ActivateTask( Elements[ 0 ].Task );
                    Elements[0].Activate( );
                }
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            AdjustSpringboardLayout( );

            UpdateLoginState( );

            // add the billboard now that we're ready
            if ( Billboard.Superview == null )
            {
                View.AddSubview( Billboard );

                TryDisplaySeriesBillboard( );
            }
        }

        /// <summary>
        /// Displays the "Tap to take notes" series billboard
        /// </summary>
        void TryDisplaySeriesBillboard( )
        {
            // first make sure all initial setup is done.
            if ( SeriesInfoDownloaded == true && IsOOBERunning == false && Billboard.Superview != null )
            {
                // should we advertise the notes?
                // yes, if it's a weekend
                if ( DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday )
                {
                    if ( RockLaunchData.Instance.Data.NoteDB.SeriesList.Count > 0 )
                    {
                        // kick off a timer to reveal the billboard, because we 
                        // don't want to do it the MOMENT the view appears.
                        System.Timers.Timer timer = new System.Timers.Timer();
                        timer.AutoReset = false;
                        timer.Interval = 1;
                        timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( 
                                delegate
                                {
                                    Billboard.Reveal( );
                                } );
                        };
                        timer.Start( );
                    }
                }
            }
        }

        /// <summary>
        /// Adjusts the positioning of the springboard elements to be spaced out consistently
        /// across ios devices
        /// </summary>
        void AdjustSpringboardLayout( )
        {
            // position the login button
            EditPictureButton.Layer.AnchorPoint = CGPoint.Empty;
            EditPictureButton.Layer.Position = new CGPoint( ( PrimaryContainerConfig.SlideAmount - EditPictureButton.Bounds.Width ) / 2, View.Frame.Height * .02f );

            NewsElement.Layer.AnchorPoint = CGPoint.Empty;
            NewsElement.Layer.Position = new CGPoint( 0, View.Frame.Height * .40f );

            ConnectElement.Layer.AnchorPoint = CGPoint.Empty;
            ConnectElement.Layer.Position = new CGPoint( 0, NewsElement.Frame.Bottom );
            
            MessagesElement.Layer.AnchorPoint = CGPoint.Empty;
            MessagesElement.Layer.Position = new CGPoint( 0, ConnectElement.Frame.Bottom );

            PrayerElement.Layer.AnchorPoint = CGPoint.Empty;
            PrayerElement.Layer.Position = new CGPoint( 0, MessagesElement.Frame.Bottom );

            GiveElement.Layer.AnchorPoint = CGPoint.Empty;
            GiveElement.Layer.Position = new CGPoint( 0, PrayerElement.Frame.Bottom );

            MoreElement.Layer.AnchorPoint = CGPoint.Empty;
            MoreElement.Layer.Position = new CGPoint( 0, GiveElement.Frame.Bottom );

            BottomSeperator.Layer.AnchorPoint = CGPoint.Empty;
            BottomSeperator.Layer.Position = new CGPoint( 0, MoreElement.Frame.Bottom );

            CampusSelectionText.Layer.AnchorPoint = CGPoint.Empty;
            CampusSelectionText.Layer.Position = new CGPoint( 5, View.Frame.Height - CampusSelectionText.Frame.Height - 2 );

            UpdateCampusViews( );
        }

        void UpdateCampusViews( )
        {
            CampusSelectionText.SizeToFit( ); 
            CampusSelectionIcon.SizeToFit( );

            CampusSelectionIcon.Layer.AnchorPoint = CGPoint.Empty;

            nfloat halfPoint = ( CampusSelectionText.Frame.Height / 2 ) - ( CampusSelectionIcon.Frame.Height / 2 );
            CampusSelectionIcon.Layer.Position = new CGPoint( CampusSelectionText.Frame.Right + 4, CampusSelectionText.Frame.Top + halfPoint );

            // overlay the button across the campus text and icon
            CampusSelectionButton.Frame = new CGRect( CampusSelectionText.Frame.Left, CampusSelectionText.Frame.Top, CampusSelectionIcon.Frame.Right, CampusSelectionText.Frame.Height );
        }

        protected void UpdateLoginState( )
        {
            // are we logged in?
            if( RockMobileUser.Instance.LoggedIn )
            {
                // get their profile
                WelcomeField.Text = SpringboardStrings.LoggedIn_Prefix;
                UserNameField.Text = RockMobileUser.Instance.PreferredName( );
                ViewProfileLabel.Text = SpringboardStrings.ViewProfile;
            }
            else
            {
                WelcomeField.Text = SpringboardStrings.LoggedOut_Label;
                UserNameField.Text = "";
                ViewProfileLabel.Text = SpringboardStrings.LoggedOut_Promo;
            }

            // update the positioning of the "Welcome: Name"
            WelcomeField.SizeToFit( );
            UserNameField.SizeToFit( );

            // center the welcome and name labels within the available Springboard width
            float totalNameWidth = (float) (WelcomeField.Bounds.Width + UserNameField.Bounds.Width);
            float totalNameHeight = Math.Max( (float) WelcomeField.Bounds.Height, (float) UserNameField.Bounds.Height );

            WelcomeField.Layer.AnchorPoint = CGPoint.Empty;
            WelcomeField.Layer.Position = new CGPoint( ( PrimaryContainerConfig.SlideAmount - totalNameWidth ) / 2, EditPictureButton.Frame.Bottom + 10 );
            WelcomeField.Bounds = new CGRect( 0, 0, WelcomeField.Bounds.Width, totalNameHeight );

            UserNameField.Layer.AnchorPoint = CGPoint.Empty;
            UserNameField.Layer.Position = new CGPoint( WelcomeField.Frame.Right, WelcomeField.Frame.Y );
            UserNameField.Bounds = new CGRect( 0, 0, UserNameField.Bounds.Width, totalNameHeight );

            ViewProfileLabel.Layer.AnchorPoint = CGPoint.Empty;
            ViewProfileLabel.Font = FontManager.GetFont( ControlStylingConfig.Small_Font_Light, ControlStylingConfig.Small_FontSize );
            ViewProfileLabel.SizeToFit( );
            ViewProfileLabel.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
            ViewProfileLabel.Layer.Position = new CGPoint( EditPictureButton.Layer.Position.X + ((EditPictureButton.Bounds.Width - ViewProfileLabel.Bounds.Width) / 2), WelcomeField.Frame.Bottom );

            float totalHeight = (float) (totalNameHeight + ViewProfileLabel.Bounds.Height);

            // wrap the view profile button around the entire "Welcome: Name" phrase
            ViewProfileButton.SetTitle( "", UIControlState.Normal );
            ViewProfileButton.Layer.AnchorPoint = CGPoint.Empty;
            ViewProfileButton.Layer.Position = new CGPoint( 0, WelcomeField.Frame.Y );
            ViewProfileButton.Bounds = new CGRect( 0, 0, View.Frame.Width, totalHeight );

            UpdateProfilePic( );
        }

        public void UpdateProfilePic( )
        {
            // the image depends on the user's status.
            if( RockMobileUser.Instance.LoggedIn )
            {
                bool useNoPhotoImage = true;

                // if they have a profile image
                if( RockMobileUser.Instance.HasProfileImage == true )
                {
                    // attempt to load it, but
                    // because the profile picture is dynamic, make sure it loads correctly.
                    try
                    {
                        MemoryStream imageStream = (MemoryStream) FileCache.Instance.LoadFile( SpringboardConfig.ProfilePic );

                        NSData imageData = NSData.FromStream( imageStream );
                        UIImage image = new UIImage( imageData );

                        ProfileImageView.Image = image;

                        useNoPhotoImage = false;
                    }
                    catch(Exception)
                    {
                        Console.WriteLine( "Bad Pic! Defaulting to No Photo" );
                    }
                }

                // if we made it here and useNoPhoto is true, well, use no photo
                if( useNoPhotoImage == true )
                {
                    ProfileImageView.Image = null;
                    EditPictureButton.SetTitle( SpringboardConfig.NoPhotoSymbol, UIControlState.Normal );
                }
            }
            else
            {
                // otherwise display the no profile image.
                ProfileImageView.Image = null;
                EditPictureButton.SetTitle( SpringboardConfig.NoProfileSymbol, UIControlState.Normal );
            }
        }

        public static void DisplayError( string title, string message )
        {
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    UIAlertView alert = new UIAlertView();
                    alert.Title = title;
                    alert.Message = message;
                    alert.AddButton( GeneralStrings.Ok );
                    alert.Show( ); 
                } );
        }

        public void OnActivated( )
        {
            NavViewController.OnActivated( );

            // if it's been longer than N hours, resync rock.
            if ( DateTime.Now.Subtract( LastRockSync ).TotalHours > SpringboardConfig.SyncRockHoursFrequency )
            {
                SyncRockData( );
            }
        }

        public void WillEnterForeground( )
        {
            NavViewController.WillEnterForeground( );
        }

        public void OnResignActive( )
        {
            NavViewController.OnResignActive( );
        }

        public void DidEnterBackground( )
        {
            NavViewController.DidEnterBackground( );

            // request quick backgrounding so we can save objects
            nint taskID = UIApplication.SharedApplication.BeginBackgroundTask( () => {});

            RockApi.Instance.SaveObjectsToDevice( );

            FileCache.Instance.SaveCacheMap( );

            UIApplication.SharedApplication.EndBackgroundTask(taskID);
        }

        public void WillTerminate( )
        {
            NavViewController.WillTerminate( );

            // request quick backgrounding so we can save objects
            nint taskID = UIApplication.SharedApplication.BeginBackgroundTask( () => {});

            RockApi.Instance.SaveObjectsToDevice( );

            FileCache.Instance.SaveCacheMap( );

            UIApplication.SharedApplication.EndBackgroundTask(taskID);
        }
	}
}
