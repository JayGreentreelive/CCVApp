
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

                NavToolbar.DisplayBackButton( true );

                // Execute a transaction, replacing any existing
                // fragment with this one inside the frame.
                var ft = FragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.navtoolbar, NavToolbar);
                ft.SetTransition(FragmentTransit.FragmentFade);
                ft.Commit();
            }
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
            CreateSpringboardButton( relativeLayout );

            // by now we should have our active task frame, so update its position.
            if ( ActiveTaskFrame == null ) throw new Exception( "ActiveTaskFrame must not be null. Set before OnCreateView()." );
            ActiveTaskFrame.SetY( view.LayoutParameters.Height );

            return relativeLayout;
        }

        void CreateSpringboardButton( RelativeLayout relativeLayout )
        {
            // create the button
            Button springboardReveal = new Button( Activity );

            // clear the background outline
            springboardReveal.Background = null;

            // position it vertically centered and a little right indented
            springboardReveal.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ((RelativeLayout.LayoutParams)springboardReveal.LayoutParameters).AddRule( LayoutRules.CenterVertical );
            springboardReveal.SetX( 10 );


            // set the font and text
            Typeface fontFace = Typeface.CreateFromAsset( RockMobile.PlatformCommon.Droid.Context.Assets, "Fonts/" + CCVApp.Config.PrimaryNavBar.RevealButton_Font + ".ttf" );
            springboardReveal.SetTypeface( fontFace, TypefaceStyle.Normal );
            springboardReveal.SetTextSize( Android.Util.ComplexUnitType.Dip, CCVApp.Config.PrimaryNavBar.RevealButton_Size );
            springboardReveal.Text = CCVApp.Config.PrimaryNavBar.RevealButton_Text;

            // use the completely overcomplicated color states to set the normal vs pressed color state.
            int [][] states = new int[][] 
                {
                    new int[] { Android.Resource.Attribute.StatePressed },
                    new int[] { Android.Resource.Attribute.StateEnabled },
                };

            int [] colors = new int[]
                {
                    RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Config.PrimaryNavBar.RevealButton_PressedColor ),
                    RockMobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Config.PrimaryNavBar.RevealButton_DepressedColor ),
                };
            springboardReveal.SetTextColor( new Android.Content.Res.ColorStateList( states, colors ) );


            // setup the click callback
            springboardReveal.Click += (object sender, System.EventArgs e) => 
                {
                    RevealSpringboard( !SpringboardRevealed );
                };

            relativeLayout.AddView( springboardReveal );
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
