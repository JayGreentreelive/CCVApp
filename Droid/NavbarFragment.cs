
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
using Droid.Tasks;
using Android.Graphics;

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

        public NavToolbarFragment NavToolbar { get; set; }

        protected Button SpringboardReveal { get; set; }

        public NavbarFragment( ) : base( )
        {
        }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            NavToolbar = FragmentManager.FindFragmentById(Resource.Id.navtoolbar) as NavToolbarFragment;
            if (NavToolbar == null)
            {
                NavToolbar = new NavToolbarFragment();
            }

            // Execute a transaction, replacing any existing
            // fragment with this one inside the frame.
            var ft = FragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.navtoolbar, NavToolbar);
            ft.SetTransition(FragmentTransit.FragmentFade);
            ft.Commit();

            NavToolbar.DisplayBackButton( true );
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            //The navbar should basically be a background with logo and a springboard reveal button in the upper left.
            var relativeLayout = inflater.Inflate(Resource.Layout.Navbar, container, false) as RelativeLayout;
            relativeLayout.SetBackgroundColor( Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.PrimaryNavBar.BackgroundColor ) );

            // create the springboard reveal button
            CreateSpringboardButton( relativeLayout );

            return relativeLayout;
        }

        void CreateSpringboardButton( RelativeLayout relativeLayout )
        {
            // create the button
            SpringboardReveal = new Button( Activity );

            // clear the background outline
            SpringboardReveal.Background = null;

            // position it vertically centered and a little right indented
            SpringboardReveal.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ((RelativeLayout.LayoutParams)SpringboardReveal.LayoutParameters).AddRule( LayoutRules.CenterVertical );
            SpringboardReveal.SetX( 10 );


            // set the font and text
            Typeface fontFace = Typeface.CreateFromAsset( Rock.Mobile.PlatformCommon.Droid.Context.Assets, "Fonts/" + CCVApp.Shared.Config.PrimaryNavBar.RevealButton_Font + ".ttf" );
            SpringboardReveal.SetTypeface( fontFace, TypefaceStyle.Normal );
            SpringboardReveal.SetTextSize( Android.Util.ComplexUnitType.Dip, CCVApp.Shared.Config.PrimaryNavBar.RevealButton_Size );
            SpringboardReveal.Text = CCVApp.Shared.Config.PrimaryNavBar.RevealButton_Text;

            // use the completely overcomplicated color states to set the normal vs pressed color state.
            int [][] states = new int[][] 
                {
                    new int[] {  Android.Resource.Attribute.StatePressed },
                    new int[] {  Android.Resource.Attribute.StateEnabled },
                    new int[] { -Android.Resource.Attribute.StateEnabled },
                };

            int [] colors = new int[]
                {
                    Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.PrimaryNavBar.RevealButton_PressedColor ),
                    Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.PrimaryNavBar.RevealButton_DepressedColor ),
                    Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.PrimaryNavBar.RevealButton_DisabledColor ),
                };
            SpringboardReveal.SetTextColor( new Android.Content.Res.ColorStateList( states, colors ) );


            // setup the click callback
            SpringboardReveal.Click += (object sender, System.EventArgs e) => 
                {
                    RevealSpringboard( !SpringboardRevealed );
                };

            relativeLayout.AddView( SpringboardReveal );
        }

        public void EnableSpringboardRevealButton( bool enabled )
        {
            SpringboardReveal.Enabled = enabled;

            if ( enabled == false )
            {
                RevealSpringboard( false );
            }
        }

        public void RevealSpringboard( bool revealed )
        {
            if( SpringboardRevealed != revealed )
            {
                if( !Animating )
                {
                    Animating = true;

                    int xOffset = revealed ? (int) (View.Width * .65f) : 0;

                    // setup an animation from our current mask scale to the new one.
                    ValueAnimator animator = ValueAnimator.OfInt((int)View.GetX( ) , xOffset);

                    animator.AddUpdateListener( this );
                    animator.AddListener( new NavbarAnimationListener( ) { NavbarFragment = this } );
                    animator.SetDuration( 500 );

                    animator.Start();
                }
            }
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            // update the mask scale
            int xPos = ((Java.Lang.Integer)animation.GetAnimatedValue("")).IntValue();

            View.SetX( xPos );

            ActiveTaskFrame.SetX( xPos );
            NavToolbar.RelativeLayout.SetX( xPos );
        }

        public void OnAnimationEnd( Animator animation )
        {
            Animating = false;
            SpringboardRevealed = !SpringboardRevealed;

            if( SpringboardRevealed == true )
            {
                EnableControls( false );
            }
            else
            {
                EnableControls( true );
            }
        }

        public void EnableControls( bool enabled )
        {
            // toggle the task frame and all its children
            EnableViews( ActiveTaskFrame, enabled );

            // toggle the sub nav bar
            EnableViews( NavToolbar.RelativeLayout, enabled );
        }

        public void EnableViews( ViewGroup view, bool enabled )
        {
            view.Enabled = enabled;

            int i;
            for( i = 0; i < view.ChildCount; i++ )
            {
                // if the child view is itself a view group, recursively toggle them
                View childView = view.GetChildAt( i );
                if( (childView as ViewGroup) != null )
                {
                    EnableViews( (ViewGroup)childView, enabled );
                }
                else
                {
                    childView.Enabled = enabled;
                }
            }
        }

        public void SetActiveTask( Tasks.Task activeTask )
        {
            // deactivate any current task
            if( ActiveTask != null )
            {
                ActiveTask.Deactivate( );
            }

            // store a ref to the task task
            ActiveTask = activeTask;

            // activate it
            ActiveTask.Activate( );

            // force the springboard to close
            RevealSpringboard( false );
        }
    }
}
