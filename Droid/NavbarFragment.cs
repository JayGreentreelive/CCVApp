
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
    /// <summary>
    /// Forwards the finished animation notification to the actual navbar fragment
    /// </summary>
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

    /// <summary>
    /// The navbar fragment acts as the container for the active task.
    /// </summary>
    public class NavbarFragment : Fragment, Android.Animation.ValueAnimator.IAnimatorUpdateListener
    {
        /// <summary>
        /// Reference to the currently active task
        /// </summary>
        /// <value>The active task.</value>
        protected Tasks.Task ActiveTask { get; set; }

        /// <summary>
        /// True when the navbar fragment and task are slid "out" to reveal the springboard
        /// </summary>
        /// <value><c>true</c> if springboard revealed; otherwise, <c>false</c>.</value>
        protected bool SpringboardRevealed { get; set; }

        /// <summary>
        /// True when the navbar fragment and container are in the process of sliding in our out
        /// </summary>
        /// <value><c>true</c> if animating; otherwise, <c>false</c>.</value>
        protected bool Animating { get; set; }

        /// <summary>
        /// The frame that stores the active task
        /// </summary>
        /// <value>The active task frame.</value>
        public FrameLayout ActiveTaskFrame { get; set; }

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

            //The navbar should basically be a background with logo and a springboard reveal button in the upper left.
            RelativeLayout relativeLayout = new RelativeLayout( Activity );
            relativeLayout.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );

            // create the background
            View view = new View( Activity );
            view.SetBackgroundResource( Resource.Drawable.ccvLogo );
            view.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
            view.LayoutParameters.Height = view.Background.IntrinsicHeight;
            relativeLayout.AddView( view );

            // create the springboard reveal button
            ImageButton springboardReveal = new ImageButton( Activity );
            springboardReveal.SetImageResource(Resource.Drawable.cheeseburger);
            springboardReveal.Background = null;
            springboardReveal.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            springboardReveal.SetX( 10 );
            springboardReveal.SetY( (view.Background.IntrinsicHeight - springboardReveal.Drawable.IntrinsicHeight) / 2 );
            relativeLayout.AddView( springboardReveal );

            springboardReveal.Click += (object sender, System.EventArgs e) => 
                {
                    RevealSpringboard( !SpringboardRevealed );
                };

            // by now we should have our active task frame, so update its position.
            if ( ActiveTaskFrame == null ) throw new Exception( "ActiveTaskFrame must not be null. Set before OnCreateView()." );
            ActiveTaskFrame.SetY( view.LayoutParameters.Height );

            return relativeLayout;
        }

        public void RevealSpringboard( bool revealed )
        {
            if( SpringboardRevealed != revealed )
            {
                if( !Animating )
                {
                    Animating = true;

                    int xOffset = revealed ? View.Width / 2 : 0;

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

            ActiveTaskFrame.SetX( xPos );
        }

        public void OnAnimationEnd( Animator animation )
        {
            Animating = false;
        }

        public void PresentFragment( Fragment fragment, bool allowBack )
        {
            // get the fragment manager
            var ft = FragmentManager.BeginTransaction();

            // set this as the active visible fragment in the task frame.
            ft.Replace(Resource.Id.activetask, fragment );

            // do a nice crossfade
            ft.SetTransition(FragmentTransit.FragmentFade);

            // if back was requested, put it in our stack
            if( allowBack )
            {
                ft.AddToBackStack( fragment.ToString() );
            }

            // do the transaction
            ft.Commit();
        }

        public void SetActiveTask( Tasks.Task activeTask )
        {
            // store a ref to the task task
            ActiveTask = activeTask;

            // get its starting fragment
            Fragment startFragment = ActiveTask.StartingFragment( );

            // get the fragment manager, set the fragment, and start it
            var ft = FragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.activetask, startFragment );
            ft.SetTransition(FragmentTransit.FragmentFade);
            ft.Commit();

            // force the springboard to close
            RevealSpringboard( false );
        }
    }
}
