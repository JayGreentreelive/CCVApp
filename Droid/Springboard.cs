
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

namespace Droid
{
    /// <summary>
    /// The springboard acts as the core navigation for the user. From here
    /// they may launch any of the app's activities.
    /// </summary>
    public class Springboard : Fragment, View.IOnTouchListener
    {
        protected class SpringboardElement
        {
            public Tasks.Task Task { get; set; }

            public RelativeLayout Layout { get; set; }
            public int LayoutId { get; set; }

            public Button Button { get; set; }
            public int ButtonId { get; set; }

            public ImageView Icon { get; set; }
            public int IconId { get; set; }

            public SpringboardElement( Tasks.Task task, int layoutId, int iconId, int buttonId )
            {
                Task = task;
                LayoutId = layoutId;
                ButtonId = buttonId;
                IconId = iconId;
            }

            public void OnCreateView( View parentView )
            {
                Layout = parentView.FindViewById<RelativeLayout>( LayoutId );
                Icon = parentView.FindViewById<ImageView>( IconId );
                Button = parentView.FindViewById<Button>( ButtonId );

                Icon.SetX( Icon.GetX() - Icon.Drawable.IntrinsicWidth / 2 );

                Button.Background = null;
            }
        }
        protected List<SpringboardElement> Elements { get; set; }

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
        bool ImageCropperPendingLaunch { get; set; }

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
            Elements.Add( new SpringboardElement( new Droid.Tasks.News.NewsTask( NavbarFragment ), Resource.Id.springboard_news_frame, Resource.Id.springboard_news_icon, Resource.Id.springboard_news_button ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Placeholder.PlaceholderTask( NavbarFragment ), Resource.Id.springboard_groupfinder_frame, Resource.Id.springboard_groupfinder_icon, Resource.Id.springboard_groupfinder_button ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Placeholder.PlaceholderTask( NavbarFragment ), Resource.Id.springboard_prayer_frame, Resource.Id.springboard_prayer_icon, Resource.Id.springboard_prayer_button ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Notes.NotesTask( NavbarFragment ), Resource.Id.springboard_notes_frame, Resource.Id.springboard_notes_icon, Resource.Id.springboard_notes_button ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Placeholder.PlaceholderTask( NavbarFragment ), Resource.Id.springboard_about_frame, Resource.Id.springboard_about_icon, Resource.Id.springboard_about_button ) );

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
            view.SetBackgroundColor( Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.Springboard.BackgroundColor ) );

            // set the task we wish to have active
            ActivateElement( Elements[ ActiveElementIndex ] );

            // setup our profile pic button
            ProfileImageButton = view.FindViewById<ImageButton>( Resource.Id.springboard_profile_image );
            ProfileImageButton.Click += (object sender, EventArgs e) => 
                {
                    // only allow picture taking if they're logged in
                    if( RockMobileUser.Instance.LoggedIn )
                    {
                        if( Rock.Mobile.Media.PlatformCamera.Instance.IsAvailable( ) )
                        {
                            // we'll request the image be stored in AppData/userPhoto.jpg
                            File imageFile = new File( DroidContext.Context.GetExternalFilesDir( null ), CCVApp.Shared.Config.Springboard.ProfilePic );

                            // start up the camera and get our picture.
                            Rock.Mobile.Media.PlatformCamera.Instance.CaptureImage( imageFile, null, 

                                delegate(object s, Rock.Mobile.Media.PlatformCamera.CaptureImageEventArgs args) 
                                {
                                    // flag that we want the cropper to start up on resume.
                                    // we cannot launch it now because we need to wait for the camera
                                    // activity to end and the navBar fragment to resume
                                    if( args.Result == true )
                                    {
                                        ImageCropperPendingLaunch = true;
                                    }
                                    else
                                    {
                                        // couldn't get the picture
                                    }
                                });
                        }
                        else
                        {
                            // nope
                        }
                    }
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

        public void ModalFragmentFinished( Fragment fragment )
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
        }

        void LaunchImageCropper( )
        {
            File imageFile = new File( DroidContext.Context.GetExternalFilesDir( null ), CCVApp.Shared.Config.Springboard.ProfilePic );

            // load the image
            Bitmap image = BitmapFactory.DecodeFile( imageFile.AbsolutePath );

            // create the crop fragment
            ImageCropFragment.Begin( image, delegate( Bitmap croppedImage )
                { 
                    image.Dispose( );
                    image = null;

                    bool success = false;

                    System.IO.FileStream fileOpenStream = null;
                    try
                    {
                        // open the existing full image
                        fileOpenStream = System.IO.File.OpenWrite( imageFile.AbsolutePath );

                        // overwrite it with the cropped image 
                        if( croppedImage.Compress( Bitmap.CompressFormat.Jpeg, 100, fileOpenStream ) )
                        {
                            success = true;
                            RockMobileUser.Instance.HasProfileImage = true;
                            SetProfileImage( );

                            //todo: Upload the image to Rock.
                            //   on confirmation, set User.HasProfileImage to true.
                        }
                    }
                    catch( Exception )
                    {
                    }

                    if( fileOpenStream != null )
                    {
                        fileOpenStream.Close( );
                    }

                    if( success == false )
                    {
                        // warn the user
                    }
                });

            // replace the entire screen the image cropper
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
            if( ImageCropperPendingLaunch == true )
            {
                LaunchImageCropper( );
                ImageCropperPendingLaunch = false;
            }
        }

        protected void UpdateLoginState( )
        {
            // are we logged in?
            if( RockMobileUser.Instance.LoggedIn )
            {
                // get their profile
                LoginProfileButton.Text = RockMobileUser.Instance.PreferredName( ) + " " + RockMobileUser.Instance.Person.LastName;
            }
            else
            {
                LoginProfileButton.Text = "Login to enable additional features.";
            }

            SetProfileImage( );
        }

        protected void SetProfileImage( )
        {
            // the image depends on the user's status.
            if( RockMobileUser.Instance.LoggedIn )
            {
                // if they have an profile pic
                if( RockMobileUser.Instance.HasProfileImage == true )
                {
                    ProfileImageButton.SetImageBitmap( null );

                    // Load the profile pic
                    File imageFile = new File( DroidContext.Context.GetExternalFilesDir( null ), CCVApp.Shared.Config.Springboard.ProfilePic );
                    Bitmap image = BitmapFactory.DecodeFile( imageFile.AbsolutePath );

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
                    if( NavbarFragment.SpringboardRevealed == true )
                    {
                        // no matter what, close the springboard
                        NavbarFragment.RevealSpringboard( false );

                        // did we tap a button?
                        SpringboardElement element = Elements.Where( el => el.Button == v ).SingleOrDefault();
                        if( element != null )
                        {
                            // did we tap within the revealed springboard area?
                            float visibleButtonWidth = NavbarFragment.View.Width * CCVApp.Shared.Config.PrimaryNavBar.RevealPercentage;
                            if( e.GetX() < visibleButtonWidth )
                            {
                                // we did, so activate the element associated with that button
                                ActiveElementIndex = Elements.IndexOf( element ); 
                                ActivateElement( element );
                            }
                        }
                    }
                    break;
                }
            }
            return true;
        }

        public void SetActiveTaskFrame( FrameLayout layout )
        {
            // once we receive the active task frame, we can start our task
            NavbarFragment.ActiveTaskFrame = layout;
        }

        protected void ActivateElement( SpringboardElement activeElement )
        {
            foreach( SpringboardElement element in Elements )
            {
                if( activeElement != element )
                {
                    element.Layout.SetBackgroundColor( Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( 0x00000000 ) );
                }
            }

            activeElement.Layout.SetBackgroundColor( Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.Springboard.Element_SelectedColor ) );
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
    }
}
