
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
using Android.Animation;

namespace Droid
{
    public class NavbarAnimationListener : Android.Animation.AnimatorListenerAdapter
    {
        public NavbarFragment NavbarFragment { get; set; }

        public override void OnAnimationEnd(Animator animation)
        {
            base.OnAnimationEnd(animation);

            // forward on this message to our parent
            NavbarFragment.OnAnimationEnd( animation );
        }
    }

    public class NavbarFragment : Fragment, Android.Animation.ValueAnimator.IAnimatorUpdateListener
    {
        protected Fragment ActiveFragment { get; set; }

        protected bool SpringboardRevealed { get; set; }
        protected bool Animating { get; set; }

        public void SetActiveFragment( Fragment activeFragment )
        {
            ActiveFragment = activeFragment;

            if( ActiveFragment.View != null )
            {
                ActiveFragment.View.SetY( 300 );
            }
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

            RelativeLayout relLayout = new RelativeLayout( Activity );
            relLayout.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );

            View view = new View( Activity );
            view.SetBackgroundColor( RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( 0xFF0000FF ) );
            view.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
            view.LayoutParameters.Height = 300;
            relLayout.AddView( view );

            Button springboardReveal = new Button( Activity );
            springboardReveal.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            springboardReveal.LayoutParameters.Width = 300;
            springboardReveal.LayoutParameters.Height = 300;
            relLayout.AddView( springboardReveal );

            springboardReveal.Click += (object sender, System.EventArgs e) => 
                {
                    RevealSpringboard( !SpringboardRevealed );
                };

            if( ActiveFragment != null )
            {
                SetActiveFragment( ActiveFragment );
            }

            return relLayout;
        }

        public void RevealSpringboard( bool revealed )
        {
            if( SpringboardRevealed != revealed )
            {
                if( !Animating )
                {
                    Animating = true;

                    int xOffset = revealed ? 240 : 0;

                    // setup an animation from our current mask scale to the new one.
                    ValueAnimator animator = ValueAnimator.OfInt((int)View.GetX( ) , xOffset);

                    animator.AddUpdateListener( this );
                    animator.AddListener( new NavbarAnimationListener( ) { NavbarFragment = this } );
                    animator.SetDuration( 500 );

                    animator.Start();

                    SpringboardRevealed = revealed;
                }
            }
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            // update the mask scale
            int xPos = ((Java.Lang.Integer)animation.GetAnimatedValue("")).IntValue();

            View.SetX( xPos );

            if( ActiveFragment != null )
            {
                ActiveFragment.View.SetX( xPos );
            }
        }

        public void OnAnimationEnd( Animator animation )
        {
            Animating = false;
        }
    }
}

