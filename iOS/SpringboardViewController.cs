using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.CoreAnimation;
using System.Drawing;
using System.Collections.Generic;
using Rock.Mobile.PlatformCommon;
using Rock.Mobile.Network;
using CCVApp.Shared.Network;
using MonoTouch.AssetsLibrary;
using System.IO;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;

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
                LogoView.Font = iOSCommon.LoadFontDynamic( SpringboardConfig.Font, SpringboardConfig.Element_FontSize );
                LogoView.TextColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_FontColor );
                LogoView.SizeToFit( );
                LogoView.BackgroundColor = UIColor.Clear;
                BackingView.AddSubview( LogoView );

                // Create the text, and populate it with the button's requested text, color and font.
                TextLabel = new UILabel();
                TextLabel.Text = labelStr;
                TextLabel.Font = TextLabel.Font.WithSize( 15.0f );
                TextLabel.BackgroundColor = UIColor.Clear;
                TextLabel.SizeToFit( );
                BackingView.AddSubview( TextLabel );

                // Create the seperator
                Seperator = new UIView( );
                Seperator.BackgroundColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_SeperatorColor );
                BackingView.AddSubview( Seperator );

                // Create the button
                Button = new UIButton( UIButtonType.Custom );
                Button.Layer.AnchorPoint = PointF.Empty;
                Button.BackgroundColor = UIColor.Clear;
                Button.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        controller.ActivateElement( this );
                    };
                BackingView.AddSubview( Button );


                // position the controls
                Button.Bounds = BackingView.Bounds;

                LogoView.Layer.Position = new PointF( SpringboardConfig.Element_LogoOffsetX, BackingView.Frame.Height / 2 );

                TextLabel.Layer.Position = new PointF( SpringboardConfig.Element_LabelOffsetX + ( TextLabel.Frame.Width / 2 ), BackingView.Frame.Height / 2 );

                Seperator.Frame = new RectangleF( 0, 0, Button.Frame.Width, 1.0f );
            }

            public void Activate( )
            {
                LogoView.TextColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_SelectedFontColor );
                TextLabel.TextColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_SelectedFontColor );
                BackingView.BackgroundColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_SelectedColor );
            }

            public void Deactivate( )
            {
                LogoView.TextColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_FontColor );
                TextLabel.TextColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_FontColor );
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

		public SpringboardViewController (IntPtr handle) : base (handle)
		{
            UserManagementStoryboard = UIStoryboard.FromName( "UserManagement", null );

            NavViewController = Storyboard.InstantiateViewController( "MainUINavigationController" ) as MainUINavigationController;

            Elements = new List<SpringboardElement>( );
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
                // otherwise demand portrait
                return UIInterfaceOrientationMask.Portrait;
            }
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            return UIInterfaceOrientation.Portrait;
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            base.WillRotate(toInterfaceOrientation, duration);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
        }

        public override bool ShouldAutomaticallyForwardRotationMethods
        {
            get
            {
                return false;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad( );

            // create the login controller / profile view controllers
            LoginViewController = UserManagementStoryboard.InstantiateViewController( "LoginViewController" ) as LoginViewController;
            LoginViewController.Springboard = this;

            ProfileViewController = UserManagementStoryboard.InstantiateViewController( "ProfileViewController" ) as ProfileViewController;
            ProfileViewController.Springboard = this;

            ImageCropViewController = UserManagementStoryboard.InstantiateViewController( "ImageCropViewController" ) as ImageCropViewController;
            ImageCropViewController.Springboard = this;

            // Instantiate all activities
            Elements.Add( new SpringboardElement( this, new NewsTask( "NewsStoryboard_iPhone" )              , NewsElement    , SpringboardConfig.Element_News_Icon    , SpringboardStrings.Element_News_Title ) );
            Elements.Add( new SpringboardElement( this, new GroupFinderTask( "GroupFinderStoryboard_iPhone" ), ConnectElement , SpringboardConfig.Element_Connect_Icon , SpringboardStrings.Element_Connect_Title ) );
            Elements.Add( new SpringboardElement( this, new NotesTask( "NotesStoryboard_iPhone" )            , MessagesElement, SpringboardConfig.Element_Messages_Icon, SpringboardStrings.Element_Messages_Title ) );
            Elements.Add( new SpringboardElement( this, new PrayerTask( "PrayerStoryboard_iPhone" )          , PrayerElement  , SpringboardConfig.Element_Prayer_Icon  , SpringboardStrings.Element_Prayer_Title ) );
            Elements.Add( new SpringboardElement( this, new GiveTask( "GiveStoryboard_iPhone" )              , GiveElement    , SpringboardConfig.Element_Give_Icon    , SpringboardStrings.Element_Give_Title ) );
            Elements.Add( new SpringboardElement( this, new AboutTask( "AboutStoryboard_iPhone" )            , MoreElement    , SpringboardConfig.Element_More_Icon    , SpringboardStrings.Element_More_Title ) );

            // add a bottom seperator for the final element
            BottomSeperator = new UIView();
            BottomSeperator.BackgroundColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_SeperatorColor );
            View.AddSubview( BottomSeperator );
            BottomSeperator.Frame = new RectangleF( 0, 0, View.Frame.Width, 1.0f );


            // set the profile image mask so it's circular
            CALayer maskLayer = new CALayer();
            maskLayer.AnchorPoint = new PointF( 0, 0 );
            maskLayer.Bounds = LoginButton.Layer.Bounds;
            maskLayer.CornerRadius = LoginButton.Bounds.Width / 2;
            maskLayer.BackgroundColor = UIColor.Black.CGColor;
            LoginButton.Layer.Mask = maskLayer;
            //

            // setup the campus selector and settings button
            CampusButton.SetTitleColor( PlatformBaseUI.GetUIColor( SpringboardConfig.Element_FontColor ), UIControlState.Normal );

            SettingsButton.SetTitleColor( PlatformBaseUI.GetUIColor( SpringboardConfig.Element_FontColor ), UIControlState.Normal );
            SettingsButton.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( SpringboardConfig.Font, SpringboardConfig.SettingsSymbolSize );
            SettingsButton.SetTitle( SpringboardConfig.SettingsSymbol, UIControlState.Normal );
            SettingsButton.SizeToFit( );

            // setup the image that will display when the user is logged in
            ProfileImageView = new UIImageView( );
            ProfileImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            ProfileImageView.Layer.AnchorPoint = PointF.Empty;
            ProfileImageView.Bounds = LoginButton.Bounds;
            ProfileImageView.Layer.Position = PointF.Empty;
            LoginButton.AddSubview( ProfileImageView );

            LoginButton.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( SpringboardConfig.Font, SpringboardConfig.ProfileSymbolFontSize );
            LoginButton.SetTitleColor( PlatformBaseUI.GetUIColor( SpringboardConfig.ProfileSymbolColor ), UIControlState.Normal );
            LoginButton.Layer.BorderColor = PlatformBaseUI.GetUIColor( SpringboardConfig.ProfileOutlineCircleColor ).CGColor;
            LoginButton.Layer.CornerRadius = LoginButton.Bounds.Width / 2;
            LoginButton.Layer.BorderWidth = 4;

            View.BackgroundColor = PlatformBaseUI.GetUIColor( SpringboardConfig.BackgroundColor );

            AddChildViewController( NavViewController );
            View.AddSubview( NavViewController.View );

            SetNeedsStatusBarAppearanceUpdate( );

            LoginButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        // they're logged in, so let them set their profile pic
                        ManageProfilePic( );
                    }
                    else
                    {
                        PresentModelViewController( LoginViewController );
                    }
                };

            ViewProfileButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    PresentModelViewController( ProfileViewController );
                };

            CCVApp.Shared.Network.RockNetworkManager.Instance.Connect( 
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // here we know whether the initial handshake with Rock went ok or not
                });
        }

        void ManageProfilePic( )
        {
            UIAlertController actionSheet = UIAlertController.Create( SpringboardStrings.ProfilePicture_SourceTitle, 
                                                                      SpringboardStrings.ProfilePicture_SourceDescription, 
                                                                      UIAlertControllerStyle.ActionSheet );

            // setup the camera
            UIAlertAction cameraAction = UIAlertAction.Create( SpringboardStrings.ProfilePicture_SourceCamera, UIAlertActionStyle.Default, delegate(UIAlertAction obj) 
                {
                    // only allow the camera if they HAVE one
                    if( Rock.Mobile.Media.PlatformCamera.Instance.IsAvailable( ) )
                    {
                        // launch the camera
                        string jpgFilename = System.IO.Path.Combine ( Environment.GetFolderPath(Environment.SpecialFolder.Personal), "cameraTemp.jpg" );
                        Rock.Mobile.Media.PlatformCamera.Instance.CaptureImage( jpgFilename, this, delegate(object s, Rock.Mobile.Media.PlatformCamera.CaptureImageEventArgs args) 
                            {
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
                    Rock.Mobile.Media.PlatformImagePicker.Instance.PickImage( this, delegate(object s, Rock.Mobile.Media.PlatformImagePicker.ImagePickEventArgs args) 
                        {
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

        void PresentModelViewController( UIViewController modelViewController )
        {
            PresentViewController( modelViewController, true, null );
            ModalControllerVisible = true;
        }

        public void ResignModelViewController( UIViewController modelViewController, object context )
        {
            // if the image cropper is resigning
            if( modelViewController == ImageCropViewController )
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
                        RockMobileUser.Instance.SetProfilePicture( memStream );

                        nsDataStream.Dispose( );
                    }
                    else
                    {
                        // notify them about a problem saving the profile picture
                        DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                    }
                }
            }

            modelViewController.DismissViewController( true, null );
            ModalControllerVisible = false;
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

        protected void ActivateElement( SpringboardElement activeElement )
        {
            // don't allow any navigation while the login controller is active
            if( ModalControllerVisible == false )
            {
                // make sure we're allowed to switch activities
                if( NavViewController.ActivateTask( activeElement.Task ) == true )
                {
                    // first turn "off" the backingView selection for all but the element
                    // becoming active.
                    foreach( SpringboardElement element in Elements )
                    {
                        if( element != activeElement )
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

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // if the image cropper is pending, launch it now.
            if( ImageCropperPendingImage != null )
            {
                ImageCropViewController.Begin( ImageCropperPendingImage, 1.0f );
                PresentModelViewController( ImageCropViewController );

                ImageCropperPendingImage = null;
            }
            else
            {
                // if we're appearing and no task is active, start one.
                // (this will only happen when the app is first launched)
                if( NavViewController.CurrentTask == null )
                {
                    ActivateElement( Elements[0] );
                }
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            AdjustSpringboardLayout( );

            UpdateLoginState( );
        }

        /// <summary>
        /// Adjusts the positioning of the springboard elements to be spaced out consistently
        /// across ios devices
        /// </summary>
        void AdjustSpringboardLayout( )
        {
            // position the login button
            LoginButton.Layer.AnchorPoint = PointF.Empty;
            LoginButton.Layer.Position = new PointF( ( PrimaryContainerConfig.SlideAmount - LoginButton.Bounds.Width ) / 2, View.Frame.Height * .05f );

            NewsElement.Layer.AnchorPoint = PointF.Empty;
            NewsElement.Layer.Position = new PointF( 0, View.Frame.Height * .40f );

            ConnectElement.Layer.AnchorPoint = PointF.Empty;
            ConnectElement.Layer.Position = new PointF( 0, NewsElement.Frame.Bottom );

            MessagesElement.Layer.AnchorPoint = PointF.Empty;
            MessagesElement.Layer.Position = new PointF( 0, ConnectElement.Frame.Bottom );

            PrayerElement.Layer.AnchorPoint = PointF.Empty;
            PrayerElement.Layer.Position = new PointF( 0, MessagesElement.Frame.Bottom );

            GiveElement.Layer.AnchorPoint = PointF.Empty;
            GiveElement.Layer.Position = new PointF( 0, PrayerElement.Frame.Bottom );

            MoreElement.Layer.AnchorPoint = PointF.Empty;
            MoreElement.Layer.Position = new PointF( 0, GiveElement.Frame.Bottom );

            BottomSeperator.Layer.AnchorPoint = PointF.Empty;
            BottomSeperator.Layer.Position = new PointF( 0, MoreElement.Frame.Bottom );

            CampusButton.Layer.AnchorPoint = PointF.Empty;
            CampusButton.Layer.Position = new PointF( 5, View.Frame.Height - CampusButton.Frame.Height - 2 );

            SettingsButton.Layer.AnchorPoint = PointF.Empty;
            SettingsButton.Layer.Position = new PointF( PrimaryContainerConfig.SlideAmount - SettingsButton.Bounds.Width, View.Frame.Height - SettingsButton.Frame.Height - 2 );
        }

        protected void UpdateLoginState( )
        {
            // are we logged in?
            if( RockMobileUser.Instance.LoggedIn )
            {
                // get their profile
                WelcomeField.Text = SpringboardStrings.LoggedIn_Prefix;
                UserNameField.Text = RockMobileUser.Instance.PreferredName( );
            }
            else
            {
                WelcomeField.Text = SpringboardStrings.LoggedOut_Promo;
                UserNameField.Text = "";
            }

            // update the positioning of the "Welcome: Name"
            WelcomeField.SizeToFit( );
            UserNameField.SizeToFit( );

            // center the welcome and name labels within the available Springboard width
            float totalWidth = WelcomeField.Bounds.Width + UserNameField.Bounds.Width;
            float totalHeight = Math.Max( WelcomeField.Bounds.Height, UserNameField.Bounds.Height );

            WelcomeField.Layer.AnchorPoint = PointF.Empty;
            WelcomeField.Layer.Position = new PointF( ( PrimaryContainerConfig.SlideAmount - totalWidth ) / 2, LoginButton.Frame.Bottom + 10 );
            WelcomeField.Bounds = new RectangleF( 0, 0, WelcomeField.Bounds.Width, totalHeight );

            UserNameField.Layer.AnchorPoint = PointF.Empty;
            UserNameField.Layer.Position = new PointF( WelcomeField.Frame.Right, WelcomeField.Frame.Y );
            UserNameField.Bounds = new RectangleF( 0, 0, UserNameField.Bounds.Width, totalHeight );

            // wrap the view profile button around the entire "Welcome: Name" phrase
            ViewProfileButton.SetTitle( "", UIControlState.Normal );
            ViewProfileButton.Layer.AnchorPoint = PointF.Empty;
            ViewProfileButton.Layer.Position = new PointF( WelcomeField.Frame.Left, WelcomeField.Frame.Y );
            ViewProfileButton.Bounds = new RectangleF( 0, 0, totalWidth, totalHeight );

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
                        UIImage image = new UIImage( RockMobileUser.Instance.ProfilePicturePath );
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
                    LoginButton.SetTitle( SpringboardConfig.NoPhotoSymbol, UIControlState.Normal );
                }

                // if we're logged in, also display the View Profile button
                ViewProfileButton.Enabled = true;
                ViewProfileButton.Hidden = false;
            }
            else
            {
                // otherwise display the no profile image.
                ProfileImageView.Image = null;
                LoginButton.SetTitle( SpringboardConfig.NoProfileSymbol, UIControlState.Normal );

                // if we're logged out, hide the view profile button
                ViewProfileButton.Enabled = false;
                ViewProfileButton.Hidden = true;
            }
        }

        public static void DisplayError( string title, string message )
        {
            Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
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
            int taskID = UIApplication.SharedApplication.BeginBackgroundTask( () => {});

            RockApi.Instance.SaveObjectsToDevice( );

            UIApplication.SharedApplication.EndBackgroundTask(taskID);
        }

        public void WillTerminate( )
        {
            NavViewController.WillTerminate( );
        }
	}
}
