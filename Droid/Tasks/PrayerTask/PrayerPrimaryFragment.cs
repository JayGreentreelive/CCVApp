
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
using CCVApp.Shared.Network;
using Android.Graphics;
using Rock.Mobile.PlatformUI;
using Rock.Mobile.PlatformCommon;
using System.Drawing;

namespace Droid
{
    namespace Tasks
    {
        namespace Prayer
        {
            public class PrayerPrimaryFragment : TaskFragment, View.IOnTouchListener
            {
                class PrayerCard
                {
                    PlatformView View { get; set; }
                    PlatformLabel Name { get; set; }
                    PlatformLabel Date { get; set; }
                    PlatformLabel Prayer { get; set; }
                    Button Pray { get; set; }

                    public PrayerCard( ref PlatformView cardView, RectangleF bounds )
                    {
                        View = PlatformView.Create( );
                        View.Bounds = bounds;

                        Name = PlatformLabel.Create( );
                        Date = PlatformLabel.Create( );
                        Prayer = PlatformLabel.Create( );
                        Pray = new Button( Rock.Mobile.PlatformCommon.Droid.Context );

                        RelativeLayout prayButtonLayout = new RelativeLayout( Rock.Mobile.PlatformCommon.Droid.Context );
                        prayButtonLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                        ((RelativeLayout.LayoutParams)prayButtonLayout.LayoutParameters).AddRule( LayoutRules.AlignParentBottom );

                        Pray.Text = "Pray";
                        Pray.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                        ((RelativeLayout.LayoutParams)Pray.LayoutParameters).AddRule( LayoutRules.CenterHorizontal );
                        prayButtonLayout.AddView( Pray );

                        // set the text color
                        Name.TextColor = 0xFFFFFFFF;
                        Date.TextColor = 0xFFFFFFFF;
                        Prayer.TextColor = 0xFFFFFFFF;

                        // set the outline for the card
                        View.BorderColor = 0x777777FF;
                        View.CornerRadius = 4;
                        View.BorderWidth = 1;

                        // add the controls
                        ViewGroup nativeView = View.PlatformNativeObject as ViewGroup;

                        Name.AddAsSubview( nativeView );
                        Date.AddAsSubview( nativeView );
                        Prayer.AddAsSubview( nativeView );
                        nativeView.AddView( prayButtonLayout );

                        cardView = View;
                    }

                    const int ViewPadding = 10;
                    public void SetPrayer( Rock.Client.PrayerRequest prayer )
                    {
                        // set the text for the name, size it so we get the height, then
                        // restrict its bounds to the card itself
                        Name.Text = prayer.FirstName;
                        Name.Frame = new RectangleF( ViewPadding, ViewPadding, View.Bounds.Width - (ViewPadding * 2), Name.Bounds.Height );
                        Name.SizeToFit( );


                        // set the date text, set the width to be the card itself,
                        // and let SizeToFit measure the height.
                        Date.Text = string.Format( "{0:dddd MMMM d}, {2:yyyy}", prayer.CreatedDateTime, prayer.CreatedDateTime, prayer.CreatedDateTime );
                        Date.Frame = new RectangleF( ViewPadding, Name.Frame.Bottom, View.Bounds.Width - (ViewPadding * 2), 0 );
                        Date.SizeToFit( );

                        // set the prayer text, allow multiple lines, set the width to be the card itself,
                        // and let SizeToFit measure the height.
                        Prayer.Text = prayer.Text;
                        Prayer.Frame = new RectangleF( ViewPadding, Date.Frame.Bottom, View.Bounds.Width - (ViewPadding * 2), 0 );
                        Prayer.SizeToFit( );
                    }
                }

                List<Rock.Client.PrayerRequest> PrayerRequests { get; set; }

                PlatformCardCarousel Carousel { get; set; }

                PrayerCard SubLeftPrayer { get; set; }
                PrayerCard LeftPrayer { get; set; }
                PrayerCard CenterPrayer { get; set; }
                PrayerCard RightPrayer { get; set; }
                PrayerCard PostRightPrayer { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.Prayer_Primary, container, false);

                    float viewRealHeight = this.Resources.DisplayMetrics.HeightPixels;

                    float cardSizePerc = .80f;
                    float cardWidth = this.Resources.DisplayMetrics.WidthPixels * cardSizePerc;
                    float cardHeight = viewRealHeight * cardSizePerc;

                    // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
                    float cardYOffset = ((viewRealHeight - cardHeight) / 2);

                    Carousel = PlatformCardCarousel.Create( cardWidth, cardHeight, new RectangleF( 0, cardYOffset, this.Resources.DisplayMetrics.WidthPixels, viewRealHeight ), UpdatePrayerCards );

                    // create our cards
                    SubLeftPrayer = new PrayerCard( ref Carousel.SubLeftCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );
                    LeftPrayer = new PrayerCard( ref Carousel.LeftCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );
                    CenterPrayer = new PrayerCard( ref Carousel.CenterCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );
                    RightPrayer = new PrayerCard( ref Carousel.RightCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );
                    PostRightPrayer = new PrayerCard( ref Carousel.PostRightCard, new RectangleF( 0, 0, cardWidth, cardHeight ) );

                    Carousel.Init( view );

                    view.SetOnTouchListener( this );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.DisplayShareButton( false, null );

                    // request the prayers each time this appears
                    CCVApp.Shared.Network.RockApi.Instance.GetPrayers( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.PrayerRequest> prayerRequests) 
                        {
                            //ActivityIndicator.Hidden = true;

                            if( prayerRequests.Count > 0 )
                            {
                                //CreatePrayerButton.Enabled = true;

                                PrayerRequests = prayerRequests;

                                Carousel.NumItems = PrayerRequests.Count;

                                UpdatePrayerCards( 0 );
                            }
                        });
                }

                public override bool OnTouch( View v, MotionEvent e )
                {
                    if( ((DroidCardCarousel)Carousel).GestureDetector.OnTouchEvent( e ) )
                    {
                        return true;
                    }

                    switch( e.Action )
                    {
                        case MotionEventActions.Up:
                        {
                            ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.00f );

                            Carousel.TouchesEnded( );
                            break;
                        }
                    }

                    return false;
                }

                void UpdatePrayerCards( int prayerIndex )
                {
                    if( prayerIndex < PrayerRequests.Count )
                    {
                        CenterPrayer.SetPrayer( PrayerRequests[ prayerIndex ] );

                        // set the left and right in case they want to swipe thru the prayers
                        if( prayerIndex - 2 >= 0 )
                        {
                            SubLeftPrayer.SetPrayer( PrayerRequests[ prayerIndex - 2 ] );
                        }

                        if( prayerIndex - 1 >= 0 )
                        {
                            LeftPrayer.SetPrayer( PrayerRequests[ prayerIndex - 1 ] );
                        }

                        if( prayerIndex + 1 < PrayerRequests.Count )
                        {
                            RightPrayer.SetPrayer( PrayerRequests[ prayerIndex + 1 ] );
                        }

                        if( prayerIndex + 2 < PrayerRequests.Count )
                        {
                            PostRightPrayer.SetPrayer( PrayerRequests[ prayerIndex + 2 ] );
                        }
                    }
                }
            }
        }
    }
}
