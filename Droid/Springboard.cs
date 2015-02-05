
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
using Java.IO;
using Droid.Tasks;
using System.IO;
using CCVApp.Shared;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;
using Android.Graphics.Drawables;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using Rock.Mobile.PlatformSpecific.Android.UI;


namespace Droid
{
    class SpringboardElement
    {
        public Tasks.Task Task { get; set; }

        RelativeLayout Layout { get; set; }
        int LayoutId { get; set; }

        public Button Button { get; set; }

        TextView Icon { get; set; }
        string IconStr { get; set; }

        string ElementLabel { get; set; }
        TextView Text { get; set; }

        public SpringboardElement( Tasks.Task task, int layoutId, string iconStr, string elementLabel )
        {
            Task = task;
            LayoutId = layoutId;
            IconStr = iconStr;
            ElementLabel = elementLabel;
        }

        public void OnCreateView( View parentView )
        {
            Layout = parentView.FindViewById<RelativeLayout>( LayoutId );
            Icon = Layout.FindViewById<TextView>( Resource.Id.icon );
            Button = Layout.FindViewById<Button>( Resource.Id.button );
            Text = Layout.FindViewById<TextView>( Resource.Id.text );

            Typeface fontFace = FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Primary );
            Icon.SetTypeface( fontFace, TypefaceStyle.Normal );
            Icon.SetTextSize( Android.Util.ComplexUnitType.Dip, SpringboardConfig.Element_FontSize );
            Icon.SetX( Icon.GetX() - Icon.Width / 2 );
            Icon.Text = IconStr;

            Text.Text = ElementLabel;
            Text.SetTypeface( FontManager.Instance.GetFont( ControlStylingConfig.Medium_Font_Regular ), TypefaceStyle.Normal );
            Text.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );

            Button.Background = null;

            // setup the seperator color
            View seperator = Layout.FindViewById<View>( Resource.Id.seperator );
            seperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );
        }

        public void Deactivate( )
        {
            Icon.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementColor ) );

            Text.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Springboard_InActiveElementColor ) );

            Layout.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( 0x00000000 ) );
        }

        public void Activate( )
        {
            Icon.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Springboard_ActiveElementColor ) );

            Text.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Springboard_ActiveElementColor ) );

            Layout.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( SpringboardConfig.Element_SelectedColor ) );
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


        protected Button ProfileImageButton { get; set; }

        protected Button LoginProfileButton { get; set; }
        protected TextView ProfileName { get; set; }
        protected TextView ViewProfileLabel { get; set; }

        protected int ActiveElementIndex { get; set; }

        bool DisplayingModalFragment { get; set; }

        /// <summary>
        /// When true, we need to launch the image cropper. We have to wait
        /// until the NavBar and all sub-fragments have been pushed to the stack.
        /// </summary>
        /// <value><c>true</c> if image cropper pending launch; otherwise, <c>false</c>.</value>
        string ImageCropperPendingFilePath { get; set; }

        Bitmap ProfileMask { get; set; }

        Bitmap ProfileMaskedImage { get; set; }

        TextView ProfilePrefix { get; set; }

        NotificationBillboard Billboard { get; set; }

        /// <summary>
        /// True when launch data is finished downloading
        /// </summary>
        bool LaunchDataFinished { get; set; }

        /// <summary>
        /// Stores the time of the last rock sync.
        /// If the user has left our app running > 24 hours we'll redownload
        /// </summary>
        /// <value>The last rock sync.</value>
        DateTime LastRockSync { get; set; }

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
            ProfileMask = BitmapFactory.DecodeResource( Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources, Resource.Drawable.androidPhotoMask );

            // Execute a transaction, replacing any existing
            // fragment with this one inside the frame.
            var ft = FragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.navbar, NavbarFragment);
            ft.SetTransition(FragmentTransit.FragmentFade);
            ft.Commit();

            // create our tasks
            Elements = new List<SpringboardElement>();
            Elements.Add( new SpringboardElement( new Droid.Tasks.News.NewsTask( NavbarFragment ), Resource.Id.springboard_news_frame, SpringboardConfig.Element_News_Icon, SpringboardStrings.Element_News_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Connect.ConnectTask( NavbarFragment ), Resource.Id.springboard_connect_frame, SpringboardConfig.Element_Connect_Icon, SpringboardStrings.Element_Connect_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Notes.NotesTask( NavbarFragment ), Resource.Id.springboard_notes_frame, SpringboardConfig.Element_Messages_Icon, SpringboardStrings.Element_Messages_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Prayer.PrayerTask( NavbarFragment ), Resource.Id.springboard_prayer_frame, SpringboardConfig.Element_Prayer_Icon, SpringboardStrings.Element_Prayer_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.Give.GiveTask( NavbarFragment ), Resource.Id.springboard_give_frame, SpringboardConfig.Element_Give_Icon, SpringboardStrings.Element_Give_Title ) );
            Elements.Add( new SpringboardElement( new Droid.Tasks.About.AboutTask( NavbarFragment ), Resource.Id.springboard_about_frame, SpringboardConfig.Element_More_Icon, SpringboardStrings.Element_More_Title ) );

            ActiveElementIndex = 0;
            if( savedInstanceState != null )
            {
                // grab the last active element
                ActiveElementIndex = savedInstanceState.GetInt( "LastActiveElement" );
            }

            // load our objects from disk
            System.Console.WriteLine( "Loading objects from device." );
            RockApi.Instance.LoadObjectsFromDevice( );
            System.Console.WriteLine( "Loading objects done." );

            // seed the last sync time with now, so that when OnResume gets called we don't do it again.
            LastRockSync = DateTime.Now;

            SyncRockData( );
        }

        void SyncRockData( )
        {
            LaunchDataFinished = false;

            CCVApp.Shared.Network.RockNetworkManager.Instance.SyncRockData( 
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // here we know whether the initial handshake with Rock went ok or not
                    LaunchDataFinished = true;

                    // if the billboard has been added, show it.
                    // Otherwise, it'll be shown when the view is finished setting up.
                    if( Billboard.Parent != null )
                    {
                        DisplaySeriesBillboard( );
                    }

                    LastRockSync = DateTime.Now;
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
            view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.SpringboardBackgroundColor ) );

            // set the task we wish to have active
            ActivateElement( Elements[ ActiveElementIndex ] );


            // setup our profile pic button, which displays either their profile picture or an icon if they're not logged in / don't have a pic
            ProfileImageButton = view.FindViewById<Button>( Resource.Id.springboard_profile_image );
            ProfileImageButton.Click += (object sender, EventArgs e) => 
                {
                    ManageProfilePic( );
                };
            Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Primary );
            ProfileImageButton.SetTypeface( fontFace, TypefaceStyle.Normal );
            ProfileImageButton.SetTextSize( Android.Util.ComplexUnitType.Dip, SpringboardConfig.ProfileSymbolFontSize );
            ProfileImageButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
            ProfileImageButton.LayoutParameters.Width = (int)Rock.Mobile.Graphics.Util.UnitToPx( 140 );
            ProfileImageButton.LayoutParameters.Height = (int)Rock.Mobile.Graphics.Util.UnitToPx( 140 );
            ProfileImageButton.SetBackgroundColor( Color.Transparent );


            // create and add a simple circle to border the image
            RelativeLayout layout = view.FindViewById<RelativeLayout>( Resource.Id.springboard_profile_image_layout );
            layout.SetBackgroundColor( Color.Transparent );

            Rock.Mobile.PlatformSpecific.Android.Graphics.CircleView circle = new Rock.Mobile.PlatformSpecific.Android.Graphics.CircleView( Activity.BaseContext );

            //note: these are converted from dp to pixels, so don't do it here.
            circle.Radius = 70;
            circle.StrokeWidth = 4;

            circle.Color = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
            circle.SetBackgroundColor( Color.Transparent );
            circle.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ( (RelativeLayout.LayoutParams)circle.LayoutParameters ).AddRule( LayoutRules.CenterInParent );
            circle.LayoutParameters.Width = (int)Rock.Mobile.Graphics.Util.UnitToPx( 150 );
            circle.LayoutParameters.Height = (int)Rock.Mobile.Graphics.Util.UnitToPx( 150 );
            layout.AddView( circle );


            // setup our login button
            LoginProfileButton = view.FindViewById<Button>( Resource.Id.springboard_login_button );
            LoginProfileButton.Click += (object sender, EventArgs e) => 
                {
                    // if we're logged in, it'll be the profile one
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        StartModalFragment( ProfileFragment );
                    }
                    else
                    {
                        // else it'll be the login one
                        StartModalFragment( LoginFragment );
                    }
                };


            // setup the textView for rendering the user's name when they're logged in "Welcome: Jered"
            ProfilePrefix = view.FindViewById<TextView>( Resource.Id.profile_prefix );
            ProfilePrefix.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Light ), TypefaceStyle.Normal );
            ProfilePrefix.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
            ProfilePrefix.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );

            ProfileName = view.FindViewById<TextView>( Resource.Id.profile_name );
            ProfileName.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
            ProfileName.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Bold ), TypefaceStyle.Normal );
            ProfileName.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );

            // setup the textView for rendering either "Tap to Personalize" or "View Profile"
            ViewProfileLabel = view.FindViewById<TextView>( Resource.Id.view_profile );
            ViewProfileLabel.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
            ViewProfileLabel.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Light ), TypefaceStyle.Normal );
            ViewProfileLabel.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );


            // get the size of the display. We will use this rather than Resources.DeviceManager because this
            // is absolute and won't change based on orientation
            Point displaySize = new Point( );
            Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
            float displayWidth = displaySize.X;


            // setup the width of the springboard area and campus selector
            LinearLayout profileContainer = view.FindViewById<LinearLayout>( Resource.Id.springboard_profile_image_container );
            profileContainer.LayoutParameters.Width = (int) ( displayWidth * PrimaryNavBarConfig.RevealPercentage );

            View campusContainer = view.FindViewById<View>( Resource.Id.campus_container );
            campusContainer.LayoutParameters.Width = (int) ( displayWidth * PrimaryNavBarConfig.RevealPercentage );
            campusContainer.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.SpringboardBackgroundColor ) );

            View seperator = view.FindViewById<View>( Resource.Id.end_seperator );
            seperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );


            // setup the bottom campus / settings selector
            TextView campusText = campusContainer.FindViewById<TextView>( Resource.Id.campus_selection_text );
            campusText.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
            campusText.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
            campusText.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
            campusText.SetTextSize(Android.Util.ComplexUnitType.Dip,  ControlStylingConfig.Small_FontSize );
            campusText.SetSingleLine( );

            TextView settingsIcon = campusContainer.FindViewById<TextView>( Resource.Id.campus_selection_icon );
            settingsIcon.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Primary ), TypefaceStyle.Normal );
            settingsIcon.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
            settingsIcon.SetTextSize( Android.Util.ComplexUnitType.Dip, SpringboardConfig.SettingsSymbolSize );
            settingsIcon.Text = SpringboardConfig.SettingsSymbol;

            // set the campus text to whatever their profile has set for viewing.
            campusText.Text = string.Format( SpringboardStrings.Viewing_Campus, RockGeneralData.Instance.Data.Campuses[ RockMobileUser.Instance.ViewingCampus ] );

            // setup the campus selection button.
            Button campusSelectionButton = campusContainer.FindViewById<Button>( Resource.Id.campus_selection_button );
            campusSelectionButton.Click += (object sender, EventArgs e ) =>
                {
                    // build an alert dialog containing all the campus choices
                    AlertDialog.Builder builder = new AlertDialog.Builder( Activity );
                    Java.Lang.ICharSequence [] campusStrings = new Java.Lang.ICharSequence[ RockGeneralData.Instance.Data.Campuses.Count ];
                    for( int i = 0; i < RockGeneralData.Instance.Data.Campuses.Count; i++ )
                    {
                        campusStrings[ i ] = new Java.Lang.String( CCVApp.Shared.Network.RockGeneralData.Instance.Data.Campuses[ i ] );
                    }

                    // launch the dialog, and on selection, update the viewing campus text.
                    builder.SetItems( campusStrings, delegate(object s, DialogClickEventArgs clickArgs) 
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    RockMobileUser.Instance.ViewingCampus = clickArgs.Which;
                                    campusText.Text = string.Format( SpringboardStrings.Viewing_Campus, RockGeneralData.Instance.Data.Campuses[ RockMobileUser.Instance.ViewingCampus ] );
                                });
                        });

                    builder.Show( );
                };

            Billboard = new NotificationBillboard( displayWidth, Rock.Mobile.PlatformSpecific.Android.Core.Context );
            Billboard.SetLabel( SpringboardStrings.TakeNotesNotificationIcon, 
                                SpringboardStrings.TakeNotesNotificationLabel, 
                                ControlStylingConfig.TextField_ActiveTextColor, 
                                SpringboardConfig.Element_SelectedColor, 
                delegate
                {
                    // find the Notes task, activate it, and tell it to jump to the read page.
                    foreach( SpringboardElement element in Elements )
                    {
                        if ( element.Task as Droid.Tasks.Notes.NotesTask != null )
                        {
                            ActivateElement( element );
                            NavbarFragment.PerformTaskAction( "Page.Read" );
                            Billboard.Hide( );
                        }
                    }
                } );

            return view;
        }

        public void RevealButtonClicked( )
        {
            // this will be called by the Navbar (which owns the reveal button) when
            // it's clicked. We want to make sure we alwas hide the billboard.
            Billboard.Hide( );
        }

        public void StartModalFragment( Fragment fragment )
        {
            // replace the entire screen with a modal fragment
            var ft = FragmentManager.BeginTransaction();
            ft.SetTransition(FragmentTransit.FragmentFade);

            ft.Replace(Resource.Id.fullscreen, fragment);
            ft.AddToBackStack( fragment.ToString() );

            ft.Commit();
        }

        public void ModalFragmentOpened( Fragment fragment )
        {
            DisplayingModalFragment = true;
        }

        public void ModalFragmentClosed( Fragment fragment )
        {
            DisplayingModalFragment = false;
        }

        public void ModalFragmentDone( Fragment fragment, object context )
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
                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                            {
                                switch( clickArgs.Which )
                                {
                                    // Photo Library
                                    case 0:
                                    {
                                        Rock.Mobile.Media.PlatformImagePicker.Instance.PickImage( Rock.Mobile.PlatformSpecific.Android.Core.Context, delegate(object s, Rock.Mobile.Media.PlatformImagePicker.ImagePickEventArgs args) 
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
                                            string jpgFilename = Rock.Mobile.PlatformSpecific.Android.Core.Context.GetExternalFilesDir( null ).ToString( ) + "cameratemp.jpg";
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

            StartModalFragment( ImageCropFragment );
        }

        public override void OnPause()
        {
            base.OnPause();

            System.Console.WriteLine( "Springboard OnPause()" );
        }

        public override void OnResume()
        {
            base.OnResume();

            System.Console.WriteLine( "Springboard OnResume()" );

            // if it's been longer than N hours, resync rock.
            if ( DateTime.Now.Subtract( LastRockSync ).Hours > SpringboardConfig.SyncRockHoursFrequency )
            {
                SyncRockData( );
            }

            UpdateLoginState( );

            // Manage the notification billboard.
            // This is the only chance we have to kick it off. We have
            // to wait till onResume because we need all fragment views created.
            if ( Billboard.Parent == null )
            {
                // First add it 
                ( (FrameLayout)NavbarFragment.ActiveTaskFrame ).AddView( Billboard );

                // if we finished getting launch data, process the billboard
                if ( LaunchDataFinished == true )
                {
                    DisplaySeriesBillboard( );
                }            
            }
        }

        /// <summary>
        /// Displays the "Tap to take notes" series billboard
        /// </summary>
        void DisplaySeriesBillboard( )
        {
            // should we advertise the notes?
            // yes, if it's a weekend and we're at CCV (that part will come later)
            //if ( DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday )
            {
                if ( RockLaunchData.Instance.Data.Series.Count > 0 )
                {
                    // kick off a timer to reveal the billboard, because we 
                    // don't want to do it the MOMENT the view appears.
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.AutoReset = false;
                    timer.Interval = 1000;
                    timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e ) =>
                        {
                            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                                {
                                    Billboard.Reveal( );
                                } );
                        };
                    timer.Start( );
                }
            }
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
                ProfilePrefix.Text = SpringboardStrings.LoggedIn_Prefix;
                ProfileName.Text = RockMobileUser.Instance.PreferredName( );
                ViewProfileLabel.Text = SpringboardStrings.ViewProfile;
            }
            else
            {
                ProfilePrefix.Text = SpringboardStrings.LoggedOut_Label;
                ProfileName.Text = "";
                ViewProfileLabel.Text = SpringboardStrings.LoggedOut_Promo;
            }

            SetProfileImage( );
        }

        public void SetProfileImage( )
        {
            ProfileImageButton.SetBackgroundDrawable( null );

            // the image depends on the user's status.
            if( RockMobileUser.Instance.LoggedIn )
            {
                // if they have an profile pic
                if( RockMobileUser.Instance.HasProfileImage == true )
                {
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
                    ProfileMaskedImage = Rock.Mobile.PlatformSpecific.Android.Graphics.Util.ApplyMaskToBitmap( scaledImage, ProfileMask, 0, 0 );

                    scaledImage.Dispose( );
                    scaledImage = null;

                    // set the final result
                    ProfileImageButton.Text = "";
                    ProfileImageButton.SetBackgroundDrawable( new BitmapDrawable( ProfileMaskedImage ) );
                }
                else
                {
                    // display the "No Photo" icon
                    ProfileImageButton.Text = SpringboardConfig.NoPhotoSymbol;
                }
            }
            else
            {
                // display the "Not Logged In" icon
                ProfileImageButton.Text = SpringboardConfig.NoProfileSymbol;
            }
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            switch( e.Action )
            {
                case MotionEventActions.Up:
                {
                    // only allow changing tasks via button press if the springboard is open 
                    // and we're not showing a modal fragment (like the Login screen)
                    if( NavbarFragment.ShouldSpringboardAllowInput( ) == true && DisplayingModalFragment == false )
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
                    element.Deactivate( );
                }
            }

            activeElement.Activate( );
            NavbarFragment.SetActiveTask( activeElement.Task );
        }

        public override void OnStop()
        {
            base.OnStop();

            // save any final changes that may have been performed by the OnPause of other Fragments
            RockApi.Instance.SaveObjectsToDevice( );

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
            Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                {
                    AlertDialog.Builder dlgAlert = new AlertDialog.Builder( Rock.Mobile.PlatformSpecific.Android.Core.Context );                      
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
