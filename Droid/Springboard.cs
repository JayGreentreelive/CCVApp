
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

    public class Task
    {
    }

    public class Springboard : Fragment
    {
        protected NavbarFragment NavbarFragment { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            /*var container = FragmentManager.FindFragmentById(Resource.Id.container) as Container;
            if (container == null)
            {
                container = new Container();

                // Make new fragment to show this selection.
                //details = DetailsFragment.NewInstance(playId);

                // Execute a transaction, replacing any existing
                // fragment with this one inside the frame.
                var ft = FragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.container, container);
                ft.SetTransition(FragmentTransit.FragmentFade);
                ft.Commit();
            }*/

            var note = FragmentManager.FindFragmentById(Resource.Id.notes) as NotesFragment;
            if (note == null)
            {
                note = new NotesFragment();

                // Execute a transaction, replacing any existing
                // fragment with this one inside the frame.
                var ft = FragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.notes, note);
                ft.SetTransition(FragmentTransit.FragmentFade);
                ft.Commit();
            }

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

            NavbarFragment.SetActiveFragment( note );
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.Springboard, container, false);
            view.Focusable = false;
            view.FocusableInTouchMode = false;

            return view;
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

