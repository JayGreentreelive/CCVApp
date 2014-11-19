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
            /// Reference to our parent view controller
            /// </summary>
            /// <value>The springboard view controller.</value>
            public SpringboardViewController SpringboardViewController { get; set; }

            /// <summary>
            /// The task that is launched by this element.
            /// </summary>
            /// <value>The task.</value>
            public Task Task { get; set; }

            /// <summary>
            /// The view that rests behind the button, graphic and text, and is colored when 
            /// the task is active.
            /// </summary>
            /// <value>The backing view.</value>
            public UIView BackingView { get; set; }

            /// <summary>
            /// The button itself. Because we have special display needs, we
            /// break the button apart, and this ends up being an empty container that lies
            /// on top of the BackingView, LogoView and TextView.
            /// </summary>
            /// <value>The button.</value>
            public UIButton Button { get; set; }

            public SpringboardElement( SpringboardViewController controller, Task task, UIButton button, string imageChar )
            {
                UIView parentView = button.Superview;

                SpringboardViewController = controller;
                Task = task;
                Button = button;

                Button.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        SpringboardViewController.ActivateElement( this );
                    };


                //The button should look as follows:
                // [ X Text ]
                // To make sure the icons and text are all aligned vertically,
                // we will actually create a backing view that can highlight (the []s)
                // and place a logo view (the X), and a text view (the Text) on top.
                // Finally, we'll make the button clear with no text and place it over the
                // backing view.

                // Create the backing view
                BackingView = new UIView( );
                BackingView.Frame = Button.Frame;
                BackingView.BackgroundColor = UIColor.Clear;
                parentView.AddSubview( BackingView );

                // Create the logo view containing the image.
                UILabel logoView = new UILabel( );
                logoView.Font = iOSCommon.LoadFontDynamic( SpringboardConfig.Element_Font, SpringboardConfig.Element_FontSize );
                logoView.TextColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_FontColor );
                logoView.Text = imageChar;
                logoView.SizeToFit( );
                logoView.Layer.Position = new PointF( SpringboardConfig.Element_LogoOffsetX, Button.Layer.Position.Y );
                logoView.BackgroundColor = UIColor.Clear;
                parentView.AddSubview( logoView );

                // Create the text, and populate it with the button's requested text, color and font.
                UILabel TextLabel = new UILabel( );
                TextLabel.Text = Button.Title( UIControlState.Normal );
                TextLabel.TextColor = Button.TitleColor( UIControlState.Normal );
                TextLabel.Font = Button.Font;
                TextLabel.BackgroundColor = UIColor.Clear;
                TextLabel.SizeToFit( );
                TextLabel.Layer.Position = new PointF( SpringboardConfig.Element_LabelOffsetX + (TextLabel.Frame.Width / 2), Button.Layer.Position.Y );
                parentView.AddSubview( TextLabel );

                // now clear out the button so it just lays on top of the contents
                Button.SetTitle( "", UIControlState.Normal );
                Button.BackgroundColor = UIColor.Clear;

                parentView.BringSubviewToFront( Button );
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
            Elements.Add( new SpringboardElement( this, new NewsTask( "NewsStoryboard_iPhone" )              , NewsButton       , SpringboardConfig.Element_News_Icon ) );
            Elements.Add( new SpringboardElement( this, new GroupFinderTask( "GroupFinderStoryboard_iPhone" ), GroupFinderButton, SpringboardConfig.Element_Connect_Icon ) );
            Elements.Add( new SpringboardElement( this, new NotesTask( "NotesStoryboard_iPhone" )            , MessagesButton   , SpringboardConfig.Element_Messages_Icon ) );
            Elements.Add( new SpringboardElement( this, new PrayerTask( "PrayerStoryboard_iPhone" )          , PrayerButton     , SpringboardConfig.Element_Prayer_Icon ) );
            Elements.Add( new SpringboardElement( this, new GiveTask( "GiveStoryboard_iPhone" )              , GiveButton       , SpringboardConfig.Element_Give_Icon ) );
            Elements.Add( new SpringboardElement( this, new AboutTask( "AboutStoryboard_iPhone" )            , AboutButton      , SpringboardConfig.Element_More_Icon ) );

            // set the profile image mask so it's circular
            CALayer maskLayer = new CALayer();
            maskLayer.AnchorPoint = new PointF( 0, 0 );
            maskLayer.Bounds = LoginButton.Layer.Bounds;
            maskLayer.CornerRadius = LoginButton.Layer.Bounds.Width / 2;
            maskLayer.BackgroundColor = UIColor.Black.CGColor;
            LoginButton.Layer.Mask = maskLayer;
            //

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
                            element.BackingView.BackgroundColor = UIColor.Clear;
                        }
                    }

                    // activate the element and its associated task
                    activeElement.BackingView.BackgroundColor = PlatformBaseUI.GetUIColor( SpringboardConfig.Element_SelectedColor );
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

                UpdateLoginState( );
            }
        }

        protected void UpdateLoginState( )
        {
            // are we logged in?
            if( RockMobileUser.Instance.LoggedIn )
            {
                // get their profile
                UserNameField.Text = SpringboardStrings.LoggedIn_Prefix + " " + RockMobileUser.Instance.PreferredName( );
            }
            else
            {
                UserNameField.Text = SpringboardStrings.LoggedOut_Promo;
            }

            UpdateProfilePic( );
        }

        public void UpdateProfilePic( )
        {
            // the image depends on the user's status.
            UIImage image = null;
            string imagePath = NSBundle.MainBundle.BundlePath + "/";

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
                        image = new UIImage( RockMobileUser.Instance.ProfilePicturePath );
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
                    image = new UIImage( imagePath + SpringboardConfig.NoPhotoFile );
                }

                // if we're logged in, also display the View Profile button
                ViewProfileButton.Enabled = true;
                ViewProfileButton.Hidden = false;
            }
            else
            {
                // otherwise display the no profile image.
                image = new UIImage( imagePath + SpringboardConfig.NoProfileFile );

                // if we're logged out, hide the view profile button
                ViewProfileButton.Enabled = false;
                ViewProfileButton.Hidden = true;
            }

            // set the final image
            LoginButton.SetImage( image, UIControlState.Normal );
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
