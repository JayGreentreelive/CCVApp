
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

            public void OnCreateView( View parentView, EventHandler buttonDelegate )
            {
                Layout = parentView.FindViewById<RelativeLayout>( LayoutId );
                Icon = parentView.FindViewById<ImageView>( IconId );
                Button = parentView.FindViewById<Button>( ButtonId );

                Icon.SetX( Icon.GetX() - Icon.Drawable.IntrinsicWidth / 2 );

                Button.Background = null;
                Button.Click += buttonDelegate;
            }
        }
        protected List<SpringboardElement> Elements { get; set; }

        /// <summary>
        /// The top navigation bar that acts as the container for Tasks
        /// </summary>
        /// <value>The navbar fragment.</value>
        protected NavbarFragment NavbarFragment { get; set; }

        protected int ActiveElementIndex { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            // get the navbar
            NavbarFragment = FragmentManager.FindFragmentById(Resource.Id.navbar) as NavbarFragment;
            if (NavbarFragment == null)
            {
                NavbarFragment = new NavbarFragment();
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

            ActiveElementIndex = 0;
            if( savedInstanceState != null )
            {
                // grab the last active element
                ActiveElementIndex = savedInstanceState.GetInt( "LastActiveElement" );
            }
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
                element.OnCreateView( view, delegate { ActiveElementIndex = Elements.IndexOf( element ); ActivateElement( element ); } );
            }

            view.SetOnTouchListener( this );
            view.SetBackgroundColor( RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Config.Springboard.BackgroundColor ) );

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            ActivateElement( Elements[ 3 ] );
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            switch( e.Action )
            {
                case MotionEventActions.Up:
                {
                    NavbarFragment.RevealSpringboard( false );

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
                    element.Layout.SetBackgroundColor( RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0x00000000 ) );
                }
            }

            activeElement.Layout.SetBackgroundColor( RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Config.Springboard.Element_SelectedColor ) );
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
