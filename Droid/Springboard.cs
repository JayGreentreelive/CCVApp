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

        protected ImageButton LoginButton { get; set; }
        protected TextView UserNameField { get; set; }

        protected int ActiveElementIndex { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            // get the navbar
            NavbarFragment = FragmentManager.FindFragmentById(Resource.Id.navbar) as NavbarFragment;
            if ( NavbarFragment == null )
            {
                NavbarFragment = new NavbarFragment( );
            }

            LoginFragment = FragmentManager.FindFragmentByTag( "Droid.LoginFragment" ) as LoginFragment;
            if( LoginFragment == null )
            {
                LoginFragment = new LoginFragment( );
            }

            ProfileFragment = FragmentManager.FindFragmentByTag( "Droid.ProfileFragment" ) as ProfileFragment;
            if( ProfileFragment == null )
            {
                ProfileFragment = new ProfileFragment( );
            }

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

            ActiveElementIndex = 3;
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

            // setup our login button
            LoginButton = view.FindViewById<ImageButton>( Resource.Id.springboard_profile_image );
            LoginButton.Click += (object sender, EventArgs e) => 
                {
                    // replace the entire screen with a user management fragment
                    var ft = FragmentManager.BeginTransaction();
                    ft.SetTransition(FragmentTransit.FragmentFade);

                    // if we're logged in, it'll be the profile one
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        ft.Replace(Resource.Id.fragment_container, ProfileFragment);
                        ft.AddToBackStack( ProfileFragment.ToString() );
                    }
                    else
                    {
                        // else it'll be the login one
                        ft.Replace(Resource.Id.fragment_container, LoginFragment);
                        ft.AddToBackStack( LoginFragment.ToString() );
                    }

                    ft.Commit();
                };

            UserNameField = view.FindViewById<TextView>( Resource.Id.springboard_login_text );

            return view;
        }

        public override void OnPause()
        {
            base.OnPause();

            RockApi.Instance.SaveObjectsToDevice( );
        }

        public override void OnResume()
        {
            base.OnResume();

            UpdateLoginState( );
        }

        protected void UpdateLoginState( )
        {
            // are we logged in?
            if( RockMobileUser.Instance.LoggedIn )
            {
                // get their profile
                UserNameField.Text = RockMobileUser.Instance.PreferredName( ) + " " + RockMobileUser.Instance.Person.LastName;
            }
            else
            {
                UserNameField.Text = "Login to enable additional features.";
            }

            // the image depends on the user's status.
            if( RockMobileUser.Instance.LoggedIn )
            {
                // todo: get their pic, else...
                LoginButton.SetImageResource( Resource.Drawable.addphoto );
            }
            else
            {
                // otherwise display the no profile image.
                LoginButton.SetImageResource( Resource.Drawable.noProfile );
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
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
