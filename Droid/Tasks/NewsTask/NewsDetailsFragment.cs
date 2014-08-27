
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
        namespace News
        {
            public class NewsDetailsFragment : TaskFragment
            {
                public NewsDetailsFragment( ) : base( )
                {
                }

                public NewsDetailsFragment( Task parentTask ) : base( parentTask )
                {
                }

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

                    View view = inflater.Inflate(Resource.Layout.News_Details, container, false);
                    view.SetOnTouchListener( this );

                    Button modeDetailsButton = view.FindViewById<Button>(Resource.Id.moreDetailsButton);

                    modeDetailsButton.Click += (object sender, EventArgs e) => 
                        {
                            // move to the next page..somehow.
                            ParentTask.OnClick( this, modeDetailsButton.Id );
                        };

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.00f );

                    return view;
                }

                protected override void TouchUpInside(View v)
                {
                    // reveal the nav bar temporarily
                    ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.00f );
                }
            }
        }
    }
}
