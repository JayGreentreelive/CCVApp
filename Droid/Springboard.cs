﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using CCVApp.Shared.Network;
using Android.Graphics;
using DroidContext = Rock.Mobile.PlatformCommon.Droid;
using Java.IO;
using Droid.Tasks;
using System.IO;
using Rock.Mobile.PlatformCommon;
using CCVApp.Shared;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;

namespace Droid
{
    class SpringboardElement
    {
        public Tasks.Task Task { get; set; }

        public RelativeLayout Layout { get; set; }
        public int LayoutId { get; set; }

        public Button Button { get; set; }
        public int ButtonId { get; set; }

        public TextView Icon { get; set; }
        public int IconId { get; set; }
        public string IconStr { get; set; }

        public SpringboardElement( Tasks.Task task, int layoutId, int buttonId, int iconId, string iconStr )
        {
            Task = task;
            LayoutId = layoutId;
            ButtonId = buttonId;
            IconId = iconId;
            IconStr = iconStr;
        }

        public void OnCreateView( View parentView )
        {
            Layout = parentView.FindViewById<RelativeLayout>( LayoutId );
            Icon = parentView.FindViewById<TextView>( IconId );
            Button = parentView.FindViewById<Button>( ButtonId );

            Typeface fontFace = DroidFontManager.Instance.GetFont( SpringboardConfig.Element_Font );
            Icon.SetTypeface( fontFace, TypefaceStyle.Normal );
            Icon.SetTextSize( Android.Util.ComplexUnitType.Dip, SpringboardConfig.Element_FontSize );
            Icon.SetX( Icon.GetX() - Icon.Width / 2 );
            Icon.Text = IconStr;

            //Button.SetBackgroundDrawable( null );
            Button.Background = null;
        }
    }

    /// <summary>
    /// The springboard acts as the core navigation for the user. From here
    /// they may launch any of the app's activities.
    /// </summary>
    public class Springboard : Fragment, View.IOnTouchListener
    {
        List<SpringboardElement> Elements { get; set; }

        /// <summary>
        /// The top navigation bar that acts as the container for Tasks
        /// </summary>
        /// <value>The navbar fragment.</value>
        protected NavbarFragment NavbarFragment { get; set; }
        protected LoginFragment LoginFragment { get; set; }
        protected ProfileFragment ProfileFragment { get; set; }
        protected ImageCropFragment ImageCropFragment { get; set; }

        protected ImageButton ProfileImageButton { get; set; }
        protected Button LoginProfileButton { get; set; }

        protected int ActiveElementIndex { get; set; }

        /// <summary>
        /// When true, we need to launch the image cropper. We have to wait
        /// until the NavBar and all sub-fragments have been pushed to the stack.
        /// </summary>
        /// <value><c>true</c> if image cropper pending launch; otherwise, <c>false</c>.</value>
        string ImageCropperPendingFilePath { get; set; }

        Bitmap ProfileMask { get; set; }

        Bitmap ProfileMaskedImage { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            RetainInstance = true;

            // setup our fragments (checking for these to be created might be unnecessary, since we'll retain this fragment)
            NavbarFragment = FragmentManager.FindFragmentById(Resource.Id.navbar) as NavbarFragment;
            if ( NavbarFragment == null )
            {
                NavbarFragment = new NavbarFragment( );
                NavbarFragment.SpringboardParent = this;
            }

            LoginFragment = FragmentManager.FindFragmentByTag( "Droid.LoginFragment" ) as LoginFragment;
            if ( LoginFragment == null )
            {
                LoginFragment = new LoginFragment( );
                LoginFragment.SpringboardParent = this;
            }

            ProfileFragment = FragmentManager.FindFragmentByTag( "Droid.ProfileFragment" ) as ProfileFragment;
            if( ProfileFragment == null )
            {
                ProfileFragment = new ProfileFragment( );
                ProfileFragment.SpringboardParent = this;
            }

            ImageCropFragment = FragmentManager.FindFragmentByTag( "Droid.ImageCropFragment" ) as ImageCropFragment;
            if( ImageCropFragment == null )
            {
                ImageCropFragment = new ImageCropFragment( );
                ImageCropFragment.SpringboardParent = this;
            }

            // get the mask used for the profile pic
            ProfileMask = BitmapFactory.DecodeResource( DroidContext.Context.Resources, Resource.Drawable.androidPhotoMask );

            // Execute a transaction, replacing any existing
            // fragment with this one inside the frame.
            var ft = FragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.navbar, NavbarFragment);
            ft.SetTransition(FragmentTransit.FragmentFade);
            ft.Commit();

            // create our tasks
            Elements = new List<SpringboardElement>();
            Elements.Add( new SpringboardElement( new Droid.Tasks.News.NewsTask( NavbarFragment ), Resource.Id.springboard_news_frame, Resource.Id.springboard_news_button, Resource.Id.springboard_news_icon, SpringboardConfig.Element_News_Icon ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Placeholder.PlaceholderTask( NavbarFragment ), Resource.Id.springboard_groupfinder_frame, Resource.Id.springboard_groupfinder_button, Resource.Id.springboard_groupfinder_icon, SpringboardConfig.Element_Connect_Icon ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Notes.NotesTask( NavbarFragment ), Resource.Id.springboard_notes_frame, Resource.Id.springboard_notes_button, Resource.Id.springboard_notes_icon, SpringboardConfig.Element_Messages_Icon ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Prayer.PrayerTask( NavbarFragment ), Resource.Id.springboard_prayer_frame, Resource.Id.springboard_prayer_button, Resource.Id.springboard_prayer_icon, SpringboardConfig.Element_Prayer_Icon ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Give.GiveTask( NavbarFragment ), Resource.Id.springboard_give_frame, Resource.Id.springboard_give_button, Resource.Id.springboard_give_icon, SpringboardConfig.Element_Give_Icon ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.About.AboutTask( NavbarFragment ), Resource.Id.springboard_about_frame, Resource.Id.springboard_about_button, Resource.Id.springboard_about_icon, SpringboardConfig.Element_More_Icon ) );

            ActiveElementIndex = 0;
            if( savedInstanceState != null )
            {
                // grab the last active element
                ActiveElementIndex = savedInstanceState.GetInt( "LastActiveElement" );
            }

            CCVApp.Shared.Network.RockNetworkManager.Instance.Connect( 
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // here we know whether the initial handshake with Rock went ok or not
                });
        }

        public override void OnSaveInstanceState( Bundle outState )
        {
            base.OnSaveInstanceState( outState );

            // store the last activity we were in
            outState.PutInt( "LastActiveElement", ActiveElementIndex );
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // grab our resource file
            View view = inflater.Inflate(Resource.Layout.Springboard, container, false);

            // let the springboard elements setup their buttons
            foreach( SpringboardElement element in Elements )
            {
                element.OnCreateView( view );

                element.Button.SetOnTouchListener( this );
            }

            view.SetOnTouchListener( this );
            view.SetBackgroundColor( PlatformBaseUI.GetUIColor( SpringboardConfig.BackgroundColor ) );

            // set the task we wish to have active
            ActivateElement( Elements[ ActiveElementIndex ] );

            // setup our profile pic button
            ProfileImageButton = view.FindViewById<ImageButton>( Resource.Id.springboard_profile_image );
            ProfileImageButton.Click += (object sender, EventArgs e) => 
                {
                    ManageProfilePic( );
                };

            // setup our login button
            LoginProfileButton = view.FindViewById<Button>( Resource.Id.springboard_login_button );
            LoginProfileButton.Click += (object sender, EventArgs e) => 
                {
                    // replace the entire screen with a user management fragment
                    var ft = FragmentManager.BeginTransaction();
                    ft.SetTransition(FragmentTransit.FragmentFade);

                    // if we're logged in, it'll be the profile one
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        ft.Replace(Resource.Id.fullscreen, ProfileFragment);
                        ft.AddToBackStack( ProfileFragment.ToString() );
                    }
                    else
                    {
                        // else it'll be the login one
                        ft.Replace(Resource.Id.fullscreen, LoginFragment);
                        ft.AddToBackStack( LoginFragment.ToString() );
                    }

                    ft.Commit();
                };

            return view;
        }

        public void ModalFragmentFinished( Fragment fragment, object context )
        {
            // called by modal (full screen) fragments that Springboard launches
            // when the fragments are done and ok to be closed.
            // (Login, Profile Editing, Image Cropping, etc.)
            if( LoginFragment == fragment )
            {
                Activity.OnBackPressed( );
                UpdateLoginState( );
            }
            else if ( ProfileFragment == fragment )
            {
                Activity.OnBackPressed( );
                UpdateLoginState( );
            }
            else if ( ImageCropFragment == fragment )
            {
                // take the newly cropped image and write it to disk
                Bitmap croppedImage = (Bitmap)context;

                bool success = false;
                MemoryStream memStream = new MemoryStream( );
                try
                {
                    // compress the image into our memory stream
                    if( croppedImage.Compress( Bitmap.CompressFormat.Jpeg, 100, memStream ) )
                    {
                        RockMobileUser.Instance.SetProfilePicture( memStream );
                        success = true;

                        SetProfileImage( );
                    }
                }
                catch( Exception )
                {
                }

                if( memStream != null )
                {
                    memStream.Dispose( );
                }

                if( success == false )
                {
                    DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                }
            }
        }

        void ManageProfilePic( )
        {
            // only allow picture taking if they're logged in
            if( RockMobileUser.Instance.LoggedIn )
            {
                // setup the chooser dialog so they can pick the photo source
                AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                builder.SetTitle( SpringboardStrings.ProfilePicture_SourceTitle );

                Java.Lang.ICharSequence [] strings = new Java.Lang.ICharSequence[]
                    {
                        new Java.Lang.String( SpringboardStrings.ProfilePicture_SourcePhotoLibrary ),
                        new Java.Lang.String( SpringboardStrings.ProfilePicture_SourceCamera ),
                        new Java.Lang.String( GeneralStrings.Cancel )
                    };

                builder.SetItems( strings, delegate(object sender, DialogClickEventArgs clickArgs) 
                    {
                        Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                            {
                                switch( clickArgs.Which )
                                {
                                    // Photo Library
                                    case 0:
                                    {
                                        Rock.Mobile.Media.PlatformImagePicker.Instance.PickImage( DroidContext.Context, delegate(object s, Rock.Mobile.Media.PlatformImagePicker.ImagePickEventArgs args) 
                                            {
                                                // android returns a path TO the image
                                                if( args.Image != null )
                                                {
                                                    ImageCropperPendingFilePath = (string) args.Image;
                                                }
                                            });
                                        break;
                                    }

                                    // Camera
                                    case 1:
                                    {
                                        if( Rock.Mobile.Media.PlatformCamera.Instance.IsAvailable( ) )
                                        {
                                            // start up the camera and get our picture.
                                            string jpgFilename = Rock.Mobile.PlatformCommon.Droid.Context.GetExternalFilesDir( null ).ToString( ) + "cameratemp.jpg";
                                            Rock.Mobile.Media.PlatformCamera.Instance.CaptureImage( new Java.IO.File( jpgFilename ), null, 

                                                delegate(object s, Rock.Mobile.Media.PlatformCamera.CaptureImageEventArgs args) 
                                                {
                                                    // flag that we want the cropper to start up on resume.
                                                    // we cannot launch it now because we need to wait for the camera
                                                    // activity to end and the navBar fragment to resume
                                                    if( args.Result == true )
                                                    {
                                                        // if the image path is valid, we have a picture.
                                                        // Otherwise, they pressed cancel, so don't do anything.
                                                        if( args.ImagePath != null )
                                                        {
                                                            ImageCropperPendingFilePath = args.ImagePath;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // couldn't get the picture
                                                        DisplayError( SpringboardStrings.ProfilePicture_Error_Title, SpringboardStrings.ProfilePicture_Error_Message );
                                                    }
                                                });
                                        }
                                        else
                                        {
                                            // nope
                                            DisplayError( SpringboardStrings.Camera_Error_Title, SpringboardStrings.Camera_Error_Message );
                                        }
                                        break;
                                    }

                                    // Cancel
                                    case 2:
                                    {
                                        break;
                                    }
                                }
                            });
                    });

                builder.Show( );
            }
        }

        void LaunchImageCropper( string filePath )
        {
            // create the crop fragment
            ImageCropFragment.Begin( filePath, 1.00f );

            // launch the image cropper
            var ft = FragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.fullscreen, ImageCropFragment);
            ft.AddToBackStack( ImageCropFragment.ToString() );
            ft.Commit( );
        }

        public override void OnPause()
        {
            base.OnPause();

            System.Console.WriteLine( "Springboard OnPause()" );

            RockApi.Instance.SaveObjectsToDevice( );
        }

        public override void OnResume()
        {
            base.OnResume();

            System.Console.WriteLine( "Springboard OnResume()" );

            UpdateLoginState( );
        }

        public void NavbarWasResumed()
        {
            // once the navbar has resumed, we're safe to launch any pending
            // fullscreen activities.
            if( ImageCropperPendingFilePath != null )
            {
                LaunchImageCropper( ImageCropperPendingFilePath );
                ImageCropperPendingFilePath = null;
            }
        }

        protected void UpdateLoginState( )
        {
            // are we logged in?
            if( RockMobileUser.Instance.LoggedIn )
            {
                // get their profile
                LoginProfileButton.Text = SpringboardStrings.LoggedIn_Prefix + " " + RockMobileUser.Instance.PreferredName( );
            }
            else
            {
                LoginProfileButton.Text = SpringboardStrings.LoggedOut_Promo;
            }

            SetProfileImage( );
        }

        public void SetProfileImage( )
        {
            // the image depends on the user's status.
            if( RockMobileUser.Instance.LoggedIn )
            {
                // if they have an profile pic
                if( RockMobileUser.Instance.HasProfileImage == true )
                {
                    ProfileImageButton.SetImageBitmap( null );

                    // Load the profile pic
                    Bitmap image = BitmapFactory.DecodeFile( RockMobileUser.Instance.ProfilePicturePath );

                    // scale the image to the size of the mask
                    Bitmap scaledImage = Bitmap.CreateScaledBitmap( image, ProfileMask.Width, ProfileMask.Height, false );

                    // dump the source image
                    image.Dispose( );
                    image = null;

                    // if we already have a final image, dispose of it
                    if( ProfileMaskedImage != null )
                    {
                        ProfileMaskedImage.Dispose( );
                        ProfileMaskedImage = null;
                    }

                    // generate the masked image
                    ProfileMaskedImage = Rock.Mobile.PlatformCommon.Droid.ApplyMaskToBitmap( scaledImage, ProfileMask );

                    scaledImage.Dispose( );
                    scaledImage = null;

                    // set the final result
                    ProfileImageButton.SetImageBitmap( ProfileMaskedImage );
                }
                else
                {
                    ProfileImageButton.SetImageResource( Resource.Drawable.addphoto );
                }
            }
            else
            {
                // otherwise display the no profile image.
                ProfileImageButton.SetImageResource( Resource.Drawable.noProfile );
            }
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            switch( e.Action )
            {
                case MotionEventActions.Up:
                {
                    // only allow changing tasks via button press if the springboard is open 
                    if( NavbarFragment.ShouldSpringboardAllowInput( ) == true )
                    {
                        // no matter what, close the springboard
                        NavbarFragment.RevealSpringboard( false );

                        // did we tap a button?
                        SpringboardElement element = Elements.Where( el => el.Button == v ).SingleOrDefault();
                        if( element != null )
                        {
                            // did we tap within the revealed springboard area?
                            float visibleButtonWidth = NavbarFragment.View.Width * PrimaryNavBarConfig.RevealPercentage;
                            if( e.GetX() < visibleButtonWidth )
                            {
                                // we did, so activate the element associated with that button
                                ActiveElementIndex = Elements.IndexOf( element ); 
                                ActivateElement( element );
                                return true;
                            }
                        }
                    }
                    break;
                }
            }
            return false;
        }

        public void SetActiveTaskFrame( FrameLayout layout )
        {
            // once we receive the active task frame, we can start our task
            NavbarFragment.ActiveTaskFrame = layout;
        }

        void ActivateElement( SpringboardElement activeElement )
        {
            foreach( SpringboardElement element in Elements )
            {
                if( activeElement != element )
                {
                    element.Layout.SetBackgroundColor( PlatformBaseUI.GetUIColor( 0x00000000 ) );
                }
            }

            activeElement.Layout.SetBackgroundColor( PlatformBaseUI.GetUIColor( SpringboardConfig.Element_SelectedColor ) );
            NavbarFragment.SetActiveTask( activeElement.Task );
        }

        public override void OnStop()
        {
            base.OnStop();
            System.Console.WriteLine( "Springboard OnStop()" );
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            System.Console.WriteLine( "Springboard OnDestroy()" );
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            System.Console.WriteLine( "Springboard OnAtach()" );
        }

        public override void OnDetach()
        {
            base.OnDetach();
            System.Console.WriteLine( "Springboard OnDetach()" );
        }

        public static void DisplayError( string title, string message )
        {
            Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate
                {
                    AlertDialog.Builder dlgAlert = new AlertDialog.Builder( DroidContext.Context );                      
                    dlgAlert.SetTitle( title ); 
                    dlgAlert.SetMessage( message ); 
                    dlgAlert.SetPositiveButton( "Ok", delegate(object sender, DialogClickEventArgs ev )
                        {
                        } );
                    dlgAlert.Create( ).Show( );
                } );
        }
    }
}
