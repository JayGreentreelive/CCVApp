
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
    namespace Tasks
    {
        namespace Give
        {
            public class GivePrimaryFragment : TaskFragment
            {
                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = inflater.Inflate(Resource.Layout.Give_Primary, container, false);
                    view.SetOnTouchListener( this );

                    // set the text to the version and build time
                    //TextView giveText = view.FindViewById<TextView>(Resource.Id.about_PrimaryFragmentText);
                    //aboutText.Text = string.Format( "CCV App Version {0}\nBuilt on {1}", CCVApp.Shared.Strings.Build.Version, CCVApp.Shared.Strings.Build.BuildTime );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.DisplayShareButton( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );
                }
            }
        }
    }
}

