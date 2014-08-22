
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
    public class Springboard : Fragment
    {
        protected NavbarFragment NavbarFragment { get; set; }
        protected Tasks.News.NewsTask News { get; set; }
        protected Tasks.Notes.NotesTask Notes { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            // get the navbar
            NavbarFragment = FragmentManager.FindFragmentById(Resource.Id.navbar) as NavbarFragment;
            if (NavbarFragment == null)
            {
                NavbarFragment = new NavbarFragment();

                // Execute a transaction, replacing any existing
                // fragment with this one inside the frame.
                var ft = FragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.navbar, NavbarFragment);
                ft.SetTransition(FragmentTransit.FragmentFade);
                ft.Commit();
            }

            // create our tasks
            News = new Droid.Tasks.News.NewsTask( NavbarFragment );
            Notes = new Droid.Tasks.Notes.NotesTask( NavbarFragment );
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // grab our resource file
            View view = inflater.Inflate(Resource.Layout.Springboard, container, false);

            // setup the buttons
            Button newsButton = view.FindViewById<Button>( Resource.Id.news_button );
            Button notesButton = view.FindViewById<Button>( Resource.Id.episodes_button );

            newsButton.Click += (object sender, EventArgs e) => 
                {
                    NavbarFragment.SetActiveTask( News );
                };

            notesButton.Click += (object sender, EventArgs e) => 
                {
                    NavbarFragment.SetActiveTask( Notes );
                };

            return view;
        }

        public void SetActiveTaskFrame( FrameLayout layout )
        {
            // once we receive the active task frame, we can start our task
            NavbarFragment.ActiveTaskFrame = layout;
            NavbarFragment.SetActiveTask( News );
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

