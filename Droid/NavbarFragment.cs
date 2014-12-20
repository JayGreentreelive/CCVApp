
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
using Rock.Mobile.PlatformCommon;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;

namespace Droid
{
    /// <summary>
    /// The navbar fragment acts as the container for the active task.
    /// </summary>
    public class NavbarFragment : Fragment, Android.Animation.ValueAnimator.IAnimatorUpdateListener
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
        /// Reference to the currently active task
        /// </summary>
        /// <value>The active task.</value>
        protected Tasks.Task ActiveTask { get; set; }

        public Springboard SpringboardParent { get; set; }

        /// <summary>
        /// True when the navbar fragment and task are slid "out" to reveal the springboard
        /// </summary>
        /// <value><c>true</c> if springboard revealed; otherwise, <c>false</c>.</value>
        bool SpringboardRevealed { get; set; }

        /// <summary>
        /// Returns true if the springboard should accept input.
        /// This will basically be false anytime the springboard is CLOSED or animating
        /// </summary>
        /// <returns><c>true</c>, if springboard allow input was shoulded, <c>false</c> otherwise.</returns>
        public bool ShouldSpringboardAllowInput( )
        {
            if ( SpringboardRevealed == true && Animating == false && IsPanning == false )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the active task should accept input from the user.
        /// This will basically be false anytime the springboard is open or animating
        /// </summary>
        /// <returns><c>true</c>, if task allow input was shoulded, <c>false</c> otherwise.</returns>
        public bool ShouldTaskAllowInput( )
        {
            if ( SpringboardRevealed == false && Animating == false && IsPanning == false )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// True when the navbar fragment and container are in the process of sliding in our out
        /// </summary>
        /// <value><c>true</c> if animating; otherwise, <c>false</c>.</value>
        bool Animating { get; set; }

        /// <summary>
        /// The frame that stores the active task
        /// </summary>
        /// <value>The active task frame.</value>
        public FrameLayout ActiveTaskFrame { get; set; }

        public NavToolbarFragment NavToolbar { get; set; }

        Button SpringboardRevealButton { get; set; }

        float LastPanX { get; set; }

        bool IsPanning { get; set; }

        /// <summary>
        /// The backing view that provides a drop shadow.
        /// </summary>
        /// <value>The shadow container.</value>
        FrameLayout DropShadowView { get; set; }

        /// <summary>
        /// The X offset for the shadow
        /// </summary>
        /// <value>The shadow view offset.</value>
        float DropShadowXOffset { get; set; }

        /// <summary>
        /// True when OnResume has been called. False when it has not.
        /// </summary>
        /// <value><c>true</c> if this instance is fragment active; otherwise, <c>false</c>.</value>
        protected bool IsFragmentActive { get; set; }

        /// <summary>
        /// The frame that is used to darken the container when the springboard is revealed
        /// </summary>
        /// <value>The fade out frame.</value>
        FrameLayout FadeOutFrame { get; set; }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            RetainInstance = true;

            NavToolbar = FragmentManager.FindFragmentById(Resource.Id.navtoolbar) as NavToolbarFragment;
            if (NavToolbar == null)
            {
                NavToolbar = new NavToolbarFragment();
            }

            FadeOutFrame = Activity.FindViewById<FrameLayout>(Resource.Id.fadeOutView) as FrameLayout;
            FadeOutFrame.Alpha = 0.0f;

            // Execute a transaction, replacing any existing
            // fragment with this one inside the frame.
            var ft = FragmentManager.BeginTransaction();
            ft.Replace(Resource.Id.navtoolbar, NavToolbar);
            ft.SetTransition(FragmentTransit.FragmentFade);
            ft.Commit();
        }

        RelativeLayout Navbar { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            //The navbar should basically be a background with logo and a springboard reveal button in the upper left.
            Navbar = inflater.Inflate(Resource.Layout.navbar, container, false) as RelativeLayout;
            Navbar.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor ) );

            // create the springboard reveal button
            CreateSpringboardButton( Navbar );

            DropShadowXOffset = 15;
            DropShadowView = Activity.FindViewById<FrameLayout>(Resource.Id.dropShadowView) as FrameLayout;

            return Navbar;
        }

        void CreateSpringboardButton( RelativeLayout relativeLayout )
        {
            // create the button
            SpringboardRevealButton = new Button( Activity );

            // clear the background outline
            SpringboardRevealButton.SetBackgroundDrawable( null );

            // position it vertically centered and a little right indented
            SpringboardRevealButton.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ((RelativeLayout.LayoutParams)SpringboardRevealButton.LayoutParameters).AddRule( LayoutRules.CenterVertical );
            SpringboardRevealButton.SetX( 10 );


            // set the font and text
            Typeface fontFace = DroidFontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Secondary );
            SpringboardRevealButton.SetTypeface( fontFace, TypefaceStyle.Normal );
            SpringboardRevealButton.SetTextSize( Android.Util.ComplexUnitType.Dip, PrimaryNavBarConfig.RevealButton_Size );
            SpringboardRevealButton.Text = PrimaryNavBarConfig.RevealButton_Text;

            // use the completely overcomplicated color states to set the normal vs pressed color state.
            int [][] states = new int[][] 
                {
                    new int[] {  Android.Resource.Attribute.StatePressed },
                    new int[] {  Android.Resource.Attribute.StateEnabled },
                    new int[] { -Android.Resource.Attribute.StateEnabled },
                };

            // let the "pressed" version just use a darker version of the normal color
            uint mutedColor = PlatformBaseUI.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false );

            int [] colors = new int[]
                {
                    PlatformBaseUI.GetUIColor( mutedColor ),
                    PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ),
                    PlatformBaseUI.GetUIColor( mutedColor ),
                };
            SpringboardRevealButton.SetTextColor( new Android.Content.Res.ColorStateList( states, colors ) );


            // setup the click callback
            SpringboardRevealButton.Click += (object sender, System.EventArgs e) => 
                {
                    RevealSpringboard( !SpringboardRevealed );
                };

            relativeLayout.AddView( SpringboardRevealButton );
        }

        public void ToggleFullscreen( bool fullscreenEnabled )
        {
            // useful for when a video wants to be fullscreen
            if ( fullscreenEnabled == true )
            {
                Navbar.Visibility = ViewStates.Gone;
                NavToolbar.ButtonLayout.Visibility = ViewStates.Gone;
            }
            else
            {
                Navbar.Visibility = ViewStates.Visible;
                NavToolbar.ButtonLayout.Visibility = ViewStates.Visible;
            }
        }

        public void EnableSpringboardRevealButton( bool enabled )
        {
            SpringboardRevealButton.Enabled = enabled;

            if ( enabled == false )
            {
                RevealSpringboard( false );
            }
        }

        public void RevealSpringboard( bool wantReveal )
        {
            if( !Animating )
            {
                Animating = true;

                int xOffset = wantReveal ? (int) (View.Width * PrimaryNavBarConfig.RevealPercentage) : 0;

                // setup an animation from our current mask scale to the new one.
                ValueAnimator xPosAnimator = ValueAnimator.OfInt((int)View.GetX( ) , xOffset);

                xPosAnimator.AddUpdateListener( this );
                xPosAnimator.AddListener( new NavbarAnimationListener( ) { NavbarFragment = this } );
                xPosAnimator.SetDuration( (int) (PrimaryContainerConfig.SlideRate * 1000.0f) );
                xPosAnimator.Start();
            }
        }

        /// <summary>
        /// Adjusts the container views based on how far along X they should move.
        /// Also updates the fade out view so it darkens or lightens as needed.
        /// </summary>
        /// <param name="xPos">X position.</param>
        void PanContainerViews( float xPos )
        {
            DropShadowView.SetX( xPos - DropShadowXOffset );
            View.SetX( xPos );
            ActiveTaskFrame.SetX( xPos );
            NavToolbar.ButtonLayout.SetX( xPos );
            FadeOutFrame.SetX( xPos );

            // now determine the alpha
            float maxSlide = ( View.Width * PrimaryNavBarConfig.RevealPercentage );
            FadeOutFrame.Alpha = Math.Min( xPos / maxSlide, PrimaryContainerConfig.SlideDarkenAmount );
        }

        public void OnDown( MotionEvent e )
        {
            LastPanX = 0;
        }

        static float sMinVelocity = 2000.0f;
        public void OnFlick( MotionEvent e1, MotionEvent e2, float velocityX, float velocityY )
        {
            // sanity check, as the events could be null.
            if ( e1 != null && e2 != null )
            {
                Console.WriteLine( "Flick Velocity: {0}", velocityX );

                // if they flicked it, go ahead and open / close the springboard

                // only allow it if we're NOT animating, the task is ok with us panning, and we're in portrait mode.
                if ( Animating == false &&
                     ActiveTask.CanContainerPan( ) &&
                     Activity.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait )
                {
                    if ( velocityX > sMinVelocity )
                    {
                        RevealSpringboard( true );
                    }
                    else if ( velocityX < -sMinVelocity )
                    {
                        RevealSpringboard( false );
                    }
                }
            }
        }

        const float MinPanThreshold = 150;
        public void OnScroll( MotionEvent e1, MotionEvent e2, float distanceX, float distanceY )
        {
            // sanity check, as the events could be null.
            if ( e1 != null && e2 != null )
            {
                // only allow it if we're NOT animating, the task is ok with us panning, and we're in portrait mode.
                if ( Animating == false &&
                     ActiveTask.CanContainerPan( ) &&
                     Activity.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait )
                {
                    IsPanning = true;

                    if ( Math.Abs( e2.RawX - e1.RawX ) > MinPanThreshold )
                    {
                        if ( LastPanX == 0 )
                        {
                            LastPanX = e2.RawX;
                        }

                        distanceX = e2.RawX - LastPanX;
                        LastPanX = e2.RawX;

                        float xPos = View.GetX( ) + distanceX;

                        float revealAmount = ( View.Width * PrimaryNavBarConfig.RevealPercentage );
                        xPos = Math.Max( 0, Math.Min( xPos, revealAmount ) );

                        PanContainerViews( xPos );
                    }
                }
            }
        }

        public void OnUp( MotionEvent e )
        {
            // if we were panning
            if ( IsPanning )
            {
                float revealAmount = ( View.Width * PrimaryNavBarConfig.RevealPercentage );
                if ( SpringboardRevealed == false )
                {
                    // since the springboard wasn't revealed, require that they moved
                    // at least 1/3rd the amount before opening it
                    if ( View.GetX( ) > revealAmount * .33f )
                    {
                        Console.WriteLine( "OnUp CALLED: Reveal True" );
                        RevealSpringboard( true );
                    }
                    else
                    {
                        Console.WriteLine( "OnUp CALLED: Reveal False" );
                        RevealSpringboard( false );
                    }
                }
                else
                {
                    if ( View.GetX( ) < revealAmount * .66f )
                    {
                        Console.WriteLine( "OnUp CALLED: Reveal False" );
                        RevealSpringboard( false );
                    }
                    else
                    {
                        Console.WriteLine( "OnUp CALLED: Reveal True" );
                        RevealSpringboard( true );
                    }
                }
            }
            else
            {
                // if we weren't panning
                if ( IsPanning == false )
                {
                    // if the task should allowe input, reveal the nav bar
                    if ( ShouldTaskAllowInput( ) == true )
                    {
                        // let the active task know that the user released input
                        ActiveTask.OnUp( e );
                    }
                    else if ( ShouldSpringboardAllowInput( ) == true )
                    {
                        // else close the springboard
                        RevealSpringboard( false );
                    }
                }
            }

            IsPanning = false;
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {

            // update the container position
            int xPos = ((Java.Lang.Integer)animation.GetAnimatedValue("")).IntValue();

            PanContainerViews( xPos );
        }

        public void OnAnimationEnd( Animator animation )
        {
            Console.WriteLine( "OnAnimationEnd CALLED" );
            Animating = false;

            // based on the position, set the springboard flag
            if ( View.GetX( ) == 0 )
            {
                SpringboardRevealed = false;
                NavToolbar.Suspend( false );
            }
            else
            {
                SpringboardRevealed = true;
                NavToolbar.Suspend( true );
            }

            // notify the task regarding what happened
            ActiveTask.SpringboardDidAnimate( SpringboardRevealed );
        }

        public override void OnPause( )
        {
            base.OnPause( );

            if( ActiveTask != null )
            {
                ActiveTask.Deactivate( true );
            }

            IsFragmentActive = false;
        }

        public override void OnResume( )
        {
            base.OnResume( );

            IsFragmentActive = true;

            if( ActiveTask != null )
            {
                ActiveTask.Activate( true );
            }

            SpringboardParent.NavbarWasResumed( );
        }

        public void SetActiveTask( Tasks.Task newTask )
        {
            // first, are we active? If we aren't, there's no way
            // we ever activated a task, so there's no need to deactivate anything.
            if ( IsFragmentActive == true )
            {
                // we are active, so if we have a current task, deactivate it.
                if ( ActiveTask != null )
                {
                    ActiveTask.Deactivate( false );
                }

                // activate the new task
                newTask.Activate( false );

                // force the springboard to close
                RevealSpringboard( false );
            }
            else
            {
                // activate the new task
                newTask.Activate( false );
            }

            // take our active task. If we didn't activate it because we aren't
            // ready, we'll do it as soon as OnResume is called.
            ActiveTask = newTask;
        }
    }
}
