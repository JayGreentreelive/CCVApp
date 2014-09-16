using System;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Animation;
using Android.Graphics;
using Android.App;
using Android.OS;

namespace Droid
{
    /// <summary>
    /// Forwards the finished animation notification to the actual navbar fragment
    /// </summary>
    public class NavToolbarAnimationListener : Android.Animation.AnimatorListenerAdapter
    {
        public NavToolbarFragment NavbarToolbar { get; set; }

        public override void OnAnimationEnd(Animator animation)
        {
            base.OnAnimationEnd(animation);

            // forward on this message to our parent
            NavbarToolbar.OnAnimationEnd( animation );
        }
    }

    public class NavToolbarFragment : Fragment, Android.Animation.ValueAnimator.IAnimatorUpdateListener
    {
        public RelativeLayout RelativeLayout { get; set; }

        Button BackButton { get; set; }
        bool BackButtonDisplayed { get; set; }

        Button ShareButton { get; set; }
        bool ShareButtonDisplayed { get; set; }

        bool Revealed { get; set; }

        bool Animating { get; set; }

        /// <summary>
        /// Timer monitoring the time the toolbar should be shown before auto-hiding.
        /// </summary>
        /// <value>The nav bar timer.</value>
        protected System.Timers.Timer NavBarTimer { get; set; }

        public NavToolbarFragment( ) : base( )
        {
            BackButton = new Button( Rock.Mobile.PlatformCommon.Droid.Context );
            RelativeLayout = new RelativeLayout( Rock.Mobile.PlatformCommon.Droid.Context );
        }

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // create a timer that can be used to autohide this toolbar.
            NavBarTimer = new System.Timers.Timer();
            NavBarTimer.AutoReset = false;
            NavBarTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => 
                {
                    // when the timer fires, hide the toolbar.
                    // Although the timer fires on a seperate thread, because we queue the reveal
                    // on the main (UI) thread, we don't have to worry about race conditions.
                    Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate { Reveal( false ); } );
                };

            RelativeLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );

            // set the nav subBar color (including opacity)
            Color navColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.SubNavToolbar.BackgroundColor );
            navColor.A = (Byte) ( (float) navColor.A * CCVApp.Shared.Config.SubNavToolbar.Opacity );
            RelativeLayout.SetBackgroundColor( navColor );

            RelativeLayout.LayoutParameters.Height = 150;


            // create the back button
            BackButton.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            ((RelativeLayout.LayoutParams)BackButton.LayoutParameters).AddRule( LayoutRules.CenterVertical );

            // set the back button's font
            Typeface fontFace = Typeface.CreateFromAsset( Rock.Mobile.PlatformCommon.Droid.Context.Assets, "Fonts/" + CCVApp.Shared.Config.SubNavToolbar.BackButton_Font + ".ttf" );
            BackButton.SetTypeface( fontFace, TypefaceStyle.Normal );
            BackButton.SetTextSize( Android.Util.ComplexUnitType.Dip, CCVApp.Shared.Config.SubNavToolbar.BackButton_Size );

            BackButton.Text = CCVApp.Shared.Config.SubNavToolbar.BackButton_Text;
            BackButton.SetBackgroundColor( Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( 0x00000000 ) );

            BackButton.Click += delegate{ Activity.OnBackPressed(); };

            // default to NOT enabled
            BackButton.Enabled = false;

            // use the completely overcomplicated color states to set the normal vs pressed color state.
            int [][] states = new int[][] 
                {
                    new int[] { Android.Resource.Attribute.StatePressed },
                    new int[] { Android.Resource.Attribute.StateEnabled },
                    new int[] { -Android.Resource.Attribute.StateEnabled },
                };

            int [] colors = new int[]
                {
                    Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.SubNavToolbar.BackButton_PressedColor ),
                    Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.SubNavToolbar.BackButton_EnabledColor ),
                    Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.SubNavToolbar.BackButton_DisabledColor ),
                };
            BackButton.SetTextColor( new Android.Content.Res.ColorStateList( states, colors ) );    

            return RelativeLayout;
        }

        public override void OnResume()
        {
            base.OnResume();

            RelativeLayout.SetY( 150 );

            UpdateButtons( );
        }

        public void DisplayBackButton( bool display )
        {
            BackButtonDisplayed = display;

            UpdateButtons( );
        }

        public void SetBackButtonEnabled( bool enabled )
        {
            BackButton.Enabled = enabled;
        }

        void UpdateButtons( )
        {
            if( RelativeLayout != null )
            {
                // start by resetting it
                RelativeLayout.RemoveAllViews( );

                // now add each button
                if( BackButtonDisplayed == true )
                {
                    RelativeLayout.AddView( BackButton );
                }

                if( ShareButton != null )
                {
                    RelativeLayout.AddView( ShareButton );
                }
            }
        }

        public void RevealForTime( float timeToShow )
        {
            // stop (reset) any current timer
            NavBarTimer.Stop( );

            // convert to milliseconds
            NavBarTimer.Interval = timeToShow * 1000;

            // start the timer
            NavBarTimer.Start( );

            // reveal the toolbar, and when the timer ticks, it will be hidden again.
            Reveal( true );
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            // update the mask scale
            int yPos = ((Java.Lang.Integer)animation.GetAnimatedValue("")).IntValue();

            RelativeLayout.SetY( yPos );
        }

        public void OnAnimationEnd( Animator animation )
        {
            Animating = false;

            Revealed = !Revealed;
        }

        public void Reveal( bool revealed )
        {
            if( Revealed != revealed )
            {
                // of course don't allow a change while we're animating it.
                if( Animating == false )
                {
                    Animating = true;

                    int yOffset = revealed ? 0 : RelativeLayout.Height;

                    // setup an animation from our current mask scale to the new one.
                    ValueAnimator animator = ValueAnimator.OfInt((int)RelativeLayout.GetY( ) , yOffset);

                    animator.AddUpdateListener( this );
                    animator.AddListener( new NavToolbarAnimationListener( ) { NavbarToolbar = this } );
                    animator.SetDuration( (long) (CCVApp.Shared.Config.SubNavToolbar.SlideRate * 1000.0f) );

                    animator.Start();
                }
            }
        }
    }
}

