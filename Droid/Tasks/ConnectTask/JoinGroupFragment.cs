
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
using Android.Webkit;

using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Android.Graphics;
using Android.GoogleMaps;
using Android.Gms.Maps;
using CCVApp.Shared;
using CCVApp.Shared.Network;
using CCVApp.Shared.Analytics;
using Rock.Mobile.Animation;
using Android.Gms.Maps.Model;
using CCVApp.Shared.UI;

namespace Droid
{
    namespace Tasks
    {
        namespace Connect
        {
            public class JoinGroupFragment : TaskFragment
            {
                public string GroupTitle { get; set; }
                public string Distance { get; set; }
                public string MeetingTime { get; set; }
                public int GroupID { get; set; }

                UIJoinGroup JoinGroupView { get; set; }
                
                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );

                    JoinGroupView = new UIJoinGroup();
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = inflater.Inflate(Resource.Layout.JoinGroup, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    RelativeLayout backgroundView = view.FindViewById<RelativeLayout>( Resource.Id.view_background );

                    JoinGroupView.Create( backgroundView, new System.Drawing.RectangleF( 0, 0, this.Resources.DisplayMetrics.WidthPixels, this.Resources.DisplayMetrics.HeightPixels ) );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.0f );

                    JoinGroupView.DisplayView( GroupTitle, MeetingTime, Distance, GroupID );
                }
            }
        }
    }
}

