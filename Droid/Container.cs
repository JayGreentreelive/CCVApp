
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
    public class Container : Fragment
    {
        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            RelativeLayout relLayout = new RelativeLayout( Activity );
            relLayout.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );

            View view = new View( Activity );
            view.SetBackgroundColor( RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0xFFFF00FF ) );
            view.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
            view.LayoutParameters.Width = 800;
            view.LayoutParameters.Height = 600;
            view.SetY( 300 );

            relLayout.AddView( view );

            return relLayout;
        }
    }
}

