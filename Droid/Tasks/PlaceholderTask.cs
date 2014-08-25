using System;
using Android.OS;
using Android.Views;

namespace Droid
{
    namespace Tasks
    {
        namespace Placeholder
        {
            public class PlaceholderFragment : TaskFragment
            {
                public PlaceholderFragment( Task parentTask ) : base( parentTask )
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

                    View view = new View( RockMobile.PlatformCommon.Droid.Context );
                    return view;
                }
            }

            public class PlaceholderTask : Task
            {
                PlaceholderFragment MainPage { get; set; }

                public PlaceholderTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    MainPage = new PlaceholderFragment( this );
                }

                public override Android.App.Fragment StartingFragment()
                {
                    return MainPage;
                }
            }
        }
    }
}

