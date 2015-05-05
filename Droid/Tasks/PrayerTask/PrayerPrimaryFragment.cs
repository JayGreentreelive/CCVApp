
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
using App.Shared.Network;
using Android.Graphics;
using Rock.Mobile.UI;
using System.Drawing;
using App.Shared.Config;
using App.Shared.Strings;
using App.Shared;
using App.Shared.Analytics;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using Rock.Mobile.Animation;
using App.Shared.PrivateConfig;

namespace Droid
{
    namespace Tasks
    {
        namespace Prayer
        {
            public class PrayerPrimaryFragment : TaskFragment
            {
                class PrayerCard
                {
                    class PrayerLayoutRender
                    {
                        public LinearLayout LinearLayout { get; set; }
                        public TextView Name { get; set; }
                        public TextView Date { get; set; }
                        public TextView Category { get; set; }
                        public TextView Prayer { get; set; }

                        float MaxPrayerLayoutHeight { get; set; }

                        public PrayerLayoutRender( RectangleF bounds, float prayerActionHeight, Rock.Client.PrayerRequest prayer )
                        {
                            // Create the core layout that stores the prayer
                            LinearLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            //LinearLayout.SetBackgroundColor( Android.Graphics.Color.Green );
                            LinearLayout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            LinearLayout.Orientation = Orientation.Vertical;


                            // add the name
                            Name = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            Name.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            ((LinearLayout.LayoutParams)Name.LayoutParameters).TopMargin = 20;
                            ((LinearLayout.LayoutParams)Name.LayoutParameters).LeftMargin = 20;
                            Name.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                            Name.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Bold ), TypefaceStyle.Normal );
                            Name.SetTextSize( ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                            Name.Text = prayer.FirstName.ToUpper( );
                            LinearLayout.AddView( Name );

                            MaxPrayerLayoutHeight = bounds.Height;

                            // create the layout for managing the category / date
                            LinearLayout detailsLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            detailsLayout.Orientation = Orientation.Horizontal;
                            detailsLayout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                            LinearLayout.AddView( detailsLayout );

                            // category
                            Category = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            Category.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            ((LinearLayout.LayoutParams)Category.LayoutParameters).TopMargin = 10;
                            ((LinearLayout.LayoutParams)Category.LayoutParameters).LeftMargin = 20;
                            Category.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                            Category.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Light ), TypefaceStyle.Normal );
                            Category.SetTextSize( ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                            Category.Text = prayer.CategoryId.HasValue ? RockGeneralData.Instance.Data.PrayerIdToCategory( prayer.CategoryId.Value ) : RockGeneralData.Instance.Data.PrayerCategories[ 0 ].Name;
                            detailsLayout.AddView( Category );

                            // add a dummy view that will force the date over to the right
                            View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                            ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                            detailsLayout.AddView( dummyView );

                            // date
                            Date = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            Date.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            ((LinearLayout.LayoutParams)Date.LayoutParameters).TopMargin = 10;
                            ((LinearLayout.LayoutParams)Date.LayoutParameters).RightMargin = 20;
                            Date.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                            Date.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Light ), TypefaceStyle.Normal );
                            Date.SetTextSize( ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                            Date.Text = string.Format( "{0:MM/dd/yy}", prayer.EnteredDateTime );
                            detailsLayout.AddView( Date );

                            // actual prayer
                            Prayer = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            Prayer.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            ((LinearLayout.LayoutParams)Prayer.LayoutParameters).TopMargin = 30;
                            ((LinearLayout.LayoutParams)Prayer.LayoutParameters).LeftMargin = 20;
                            ((LinearLayout.LayoutParams)Prayer.LayoutParameters).RightMargin = 20;
                            ((LinearLayout.LayoutParams)Prayer.LayoutParameters).BottomMargin = (int)prayerActionHeight;
                            Prayer.SetMinWidth( (int)bounds.Width - 40 );
                            Prayer.SetMaxWidth( (int)bounds.Width - 40 );
                            Prayer.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                            Prayer.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Font_Regular ), TypefaceStyle.Normal );
                            Prayer.SetTextSize( ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                            Prayer.Text = prayer.Text;
                            LinearLayout.AddView( Prayer );
                        }

                        public void Scroll( float distanceY )
                        {
                            if ( Prayer != null )
                            {
                                // create the specs we want for measurement
                                int widthMeasureSpec = Android.Views.View.MeasureSpec.MakeMeasureSpec( LinearLayout.Width, MeasureSpecMode.AtMost );
                                int heightMeasureSpec = Android.Views.View.MeasureSpec.MakeMeasureSpec( 0, MeasureSpecMode.Unspecified );

                                // measure the label given the current width/height/text
                                Prayer.Measure( widthMeasureSpec, heightMeasureSpec );

                                Prayer.ScrollY += (int)distanceY;

                                // allow scrolling enough to show all the content of the prayer, but no more than that.
                                float availableHeight = MaxPrayerLayoutHeight - Prayer.GetY( );
                                Prayer.ScrollY = Math.Max( 0, Math.Min( Prayer.ScrollY, Prayer.MeasuredHeight - (int)availableHeight ) );
                            }
                        }

                        public void LayoutChanged( RectangleF bounds )
                        {
                            Prayer.SetMinWidth( (int)bounds.Width - 40 );
                            Prayer.SetMaxWidth( (int)bounds.Width - 40 );

                            MaxPrayerLayoutHeight = bounds.Height;
                        }
                    }

                    PrayerLayoutRender PrayerLayout { get; set; }
                    Rock.Client.PrayerRequest PrayerRequest { get; set; }
                    public PlatformView View { get; set; }

                    // properties for the actionable pray section
                    Button PrayerActionButton { get; set; }
                    CircleView PrayerActionCircle { get; set; }
                    TextView PrayerActionLabel { get; set; }
                    System.Drawing.SizeF PrayerActionSize { get; set; }
                    RelativeLayout PrayerActionLayout { get; set; }
                    bool Prayed { get; set; }

                    static float PrayerActionDimension = 100;

                    public PrayerCard( Rock.Client.PrayerRequest prayer, RectangleF bounds )
                    {
                        PrayerRequest = prayer;

                        View = PlatformView.Create( );
                        View.Bounds = bounds;
                        View.BackgroundColor = ControlStylingConfig.BG_Layer_Color;
                        View.BorderColor = ControlStylingConfig.BG_Layer_BorderColor;
                        View.CornerRadius = ControlStylingConfig.Button_CornerRadius;
                        View.BorderWidth = ControlStylingConfig.BG_Layer_BorderWidth;

                        // create a vertically oriented linearLayout that will act as our root
                        RelativeLayout root = new RelativeLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        //root.SetBackgroundColor( Android.Graphics.Color.GreenYellow );
                        root.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );


                        // create the bottom prayer layout
                        PrayerActionSize = new System.Drawing.SizeF( Rock.Mobile.Graphics.Util.UnitToPx( PrayerActionDimension ), 
                                                                     Rock.Mobile.Graphics.Util.UnitToPx( PrayerActionDimension ) );


                        // create the layout that will contain the circle, button and label
                        PrayerActionLayout = new RelativeLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        //frameLayout.SetBackgroundColor( Android.Graphics.Color.Aqua );
                        PrayerActionLayout.LayoutParameters = new LinearLayout.LayoutParams( (int)PrayerActionSize.Width, (int)PrayerActionSize.Height );
                        PrayerActionLayout.SetX( bounds.Width - PrayerActionSize.Width / 1.5f );
                        PrayerActionLayout.SetY( bounds.Height - PrayerActionSize.Height / 1.5f );
                        ((LinearLayout.LayoutParams)PrayerActionLayout.LayoutParameters).Weight = 1;
                        root.AddView( PrayerActionLayout );


                        // Pray Button
                        PrayerActionButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        PrayerActionButton.Enabled = false;
                        PrayerActionButton.Background = null;
                        PrayerActionButton.LayoutParameters = new RelativeLayout.LayoutParams( (int)PrayerActionSize.Width, (int)PrayerActionSize.Height );
                        //Pray.SetBackgroundColor( Android.Graphics.Color.Green );
                        PrayerActionLayout.AddView( PrayerActionButton );


                        // Layout for the text and circle
                        PrayerActionCircle = new Rock.Mobile.PlatformSpecific.Android.Graphics.CircleView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        PrayerActionCircle.Color = Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor );
                        PrayerActionCircle.StrokeWidth = 1;
                        //PrayerActionCircle.SetBackgroundColor( Android.Graphics.Color.Blue );
                        PrayerActionCircle.LayoutParameters = new RelativeLayout.LayoutParams( (int)PrayerActionSize.Width, (int)PrayerActionSize.Height );
                        PrayerActionLayout.AddView( PrayerActionCircle );

                        // Setup the "I Prayed" label
                        PrayerActionLabel = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        PrayerActionLabel.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                        PrayerActionLabel.Text = PrayerStrings.Prayer_Before;
                        PrayerActionLabel.Gravity = GravityFlags.Center;
                        //PrayerActionLabel.SetBackgroundColor( Android.Graphics.Color.Orange );
                        PrayerActionLabel.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                        PrayerActionLayout.AddView( PrayerActionLabel );

                        PositionPrayedLabel( );

                        PrayerActionButton.Click += (object sender, EventArgs e) => 
                            {
                                TogglePrayed( true );
                            };
                        //

                        // add the controls
                        ViewGroup nativeView = View.PlatformNativeObject as ViewGroup;
                        nativeView.AddView( root );


                        // add it to this view
                        PrayerLayout = new PrayerLayoutRender( new RectangleF( bounds.Left, bounds.Top, bounds.Width, bounds.Height - PrayerActionSize.Height ), PrayerActionSize.Height * .75f, prayer );
                        nativeView.AddView( PrayerLayout.LinearLayout );

                        PrayerActionButton.Enabled = true;
                    }

                    public void TogglePrayed( bool prayed )
                    {
                        // ignore if the state remains the same
                        if ( prayed != Prayed )
                        {
                            Prayed = prayed;

                            uint currColor = 0;
                            uint targetColor = 0;

                            // if we are ACTIVATING prayed, 
                            if ( prayed == true )
                            {
                                // send an analytic 
                                App.Shared.Network.RockApi.Instance.IncrementPrayerCount( PrayerRequest.Id, null );

                                // and fill in the circle
                                PrayerActionLabel.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                                PrayerActionLabel.Text = PrayerStrings.Prayer_After;

                                currColor = ControlStylingConfig.BG_Layer_BorderColor;
                                targetColor = PrayerConfig.PrayedForColor;
                                PrayerActionCircle.Style = Android.Graphics.Paint.Style.FillAndStroke;
                            }
                            else
                            {
                                // on deactivation, clear the circle
                                PrayerActionLabel.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                                PrayerActionLabel.Text = PrayerStrings.Prayer_Before;

                                currColor = PrayerConfig.PrayedForColor;
                                targetColor = ControlStylingConfig.BG_Layer_BorderColor;
                                PrayerActionCircle.Style = Android.Graphics.Paint.Style.Stroke;
                            }

                            PositionPrayedLabel( );

                            // animate the circle color to its new target
                            SimpleAnimator_Color colorAnim = new SimpleAnimator_Color( currColor, targetColor, .35f, 
                                delegate(float percent, object value )
                                {
                                    PrayerActionCircle.Color = Rock.Mobile.UI.Util.GetUIColor( (uint)value );
                                    PrayerActionCircle.Invalidate( );
                                }, null );
                            colorAnim.Start( );
                        }
                    }

                    void PositionPrayedLabel( )
                    {
                        PrayerActionLabel.Measure( 0, 0 );
                        PrayerActionLabel.SetX( (PrayerActionSize.Width / 2) - (PrayerActionLabel.MeasuredWidth / 1.25f) );
                        PrayerActionLabel.SetY( (PrayerActionSize.Height / 2) - PrayerActionLabel.MeasuredHeight );
                    }

                    public void Scroll( float distanceY )
                    {
                        // only process scrolling if we have an active prayer. (We won't if the prayers are still downloading)
                        if ( PrayerLayout != null )
                        {
                            PrayerLayout.Scroll( distanceY );
                        }
                    }

                    public void LayoutChanged( RectangleF bounds )
                    {
                        View.Bounds = bounds;

                        PrayerLayout.LayoutChanged( new RectangleF( bounds.Left, bounds.Top, bounds.Width, bounds.Height - PrayerActionSize.Height ) );

                        // set the prayer action area correctly
                        PrayerActionLayout.SetX( bounds.Width - PrayerActionSize.Width / 1.5f );
                        PrayerActionLayout.SetY( bounds.Height - PrayerActionSize.Height / 1.5f );
                    }
                }

                List<PrayerCard> PrayerRequestCards { get; set; }

                PlatformCardCarousel Carousel { get; set; }

                DateTime LastDownloadTime { get; set; }

                RectangleF PrayerCardSize { get; set; }
                ProgressBar ActivityIndicator { get; set; }

                bool IsActive { get; set; }
                bool IsRequesting { get; set; }


                public View StatusLayer { get; set; }
                public TextView StatusText { get; set; }

                public View ResultLayer { get; set; }
                public TextView ResultSymbol { get; set; }
                public TextView ResultText { get; set; }

                public Button RetryButton { get; set; }

                public PrayerPrimaryFragment( )
                {
                    LastDownloadTime = DateTime.MinValue;
                    PrayerRequestCards = new List<PrayerCard>();
                }

                static float PrayerCardSizePerc = .80f;

                float GetPrayerRegionHeight( )
                {
                    // take the valid height of the area a prayer can display (the space between the top header and the bottom navToolbar)
                    return Resources.DisplayMetrics.HeightPixels - ParentTask.NavbarFragment.NavToolbar.ButtonLayout.Height - ParentTask.NavbarFragment.ActiveTaskFrame.GetY( );
                }

                float GetCardWidth( )
                {
                    return NavbarFragment.GetContainerDisplayWidth( ) * PrayerCardSizePerc;
                }

                float GetCardHeight( )
                {
                    return GetPrayerRegionHeight( ) * PrayerCardSizePerc;
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

                    View view = inflater.Inflate(Resource.Layout.Prayer_Primary, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    ActivityIndicator = (ProgressBar)view.FindViewById<ProgressBar>( Resource.Id.prayer_primary_activityIndicator );
                    ActivityIndicator.Visibility = ViewStates.Invisible;

                    // create the carousel
                    float prayerRegionHeight = GetPrayerRegionHeight( );

                    float cardWidth = GetCardWidth( );
                    float cardHeight = GetCardHeight( );

                    // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
                    float cardYOffset = (prayerRegionHeight - cardHeight) / 2;

                    PrayerCardSize = new RectangleF( 0, 0, cardWidth, cardHeight );

                    // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
                    Carousel = PlatformCardCarousel.Create( view, cardWidth, cardHeight, new RectangleF( 0, cardYOffset, NavbarFragment.GetContainerDisplayWidth( ), prayerRegionHeight ), PrivatePrayerConfig.Card_AnimationDuration );


                    // setup our error UI
                    StatusLayer = view.FindViewById<View>( Resource.Id.status_background );
                    ControlStyling.StyleBGLayer( StatusLayer );

                    StatusText = StatusLayer.FindViewById<TextView>( Resource.Id.text );
                    ControlStyling.StyleUILabel( StatusText, ControlStylingConfig.Font_Regular, ControlStylingConfig.Medium_FontSize );

                    ResultLayer = view.FindViewById<View>( Resource.Id.result_background );
                    ControlStyling.StyleBGLayer( ResultLayer );

                    ResultSymbol = ResultLayer.FindViewById<TextView>( Resource.Id.resultSymbol );
                    ResultSymbol.SetTypeface( FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Secondary ), TypefaceStyle.Normal );
                    ResultSymbol.SetTextSize( ComplexUnitType.Dip, PrivatePrayerConfig.PostPrayer_ResultSymbolSize_Droid );
                    ResultSymbol.SetTextColor( Rock.Mobile.UI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    ResultSymbol.Text = ControlStylingConfig.Result_Symbol_Failed;

                    ResultText = ResultLayer.FindViewById<TextView>( Resource.Id.text );
                    ControlStyling.StyleUILabel( ResultText, ControlStylingConfig.Font_Regular, ControlStylingConfig.Large_FontSize );

                    RetryButton = view.FindViewById<Button>( Resource.Id.retryButton );
                    ControlStyling.StyleButton( RetryButton, GeneralStrings.Retry, ControlStylingConfig.Font_Regular, ControlStylingConfig.Large_FontSize );

                    RetryButton.Click += (object sender, EventArgs e ) =>
                        {
                            if( IsRequesting == false )
                            {
                                DownloadPrayers( );
                            }
                        };

                    return view;
                }

                public void ResetPrayerStatus( )
                {
                    // now update the layout for each prayer card
                    foreach ( PrayerCard prayerCard in PrayerRequestCards )
                    {
                        prayerCard.TogglePrayed( false );
                    }
                }

                void LayoutChanged( )
                {
                    float prayerRegionHeight = GetPrayerRegionHeight( );

                    float cardWidth = GetCardWidth( );
                    float cardHeight = GetCardHeight( );
                    
                    // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
                    float cardYOffset = (prayerRegionHeight - cardHeight) / 2;

                    PrayerCardSize = new RectangleF( 0, 0, cardWidth, cardHeight );

                    Carousel.LayoutChanged( cardWidth, cardHeight, new RectangleF( 0, cardYOffset, NavbarFragment.GetContainerDisplayWidth( ), prayerRegionHeight ) );

                    // now update the layout for each prayer card
                    foreach ( PrayerCard prayerCard in PrayerRequestCards )
                    {
                        prayerCard.LayoutChanged( PrayerCardSize );
                    }
                }

                public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
                {
                    base.OnConfigurationChanged(newConfig);

                    // let the carousel and cards update
                    LayoutChanged( );
                }

                public override void OnPause()
                {
                    base.OnPause();

                    IsActive = false;

                    Carousel.Clear( );
                }

                public override void OnResume()
                {
                    base.OnResume();

                    IsActive = true;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( true, delegate
                        {
                            ParentTask.OnClick( this, 0 );
                        } );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( true );

                    if ( IsRequesting == false )
                    {
                        TimeSpan deltaTime = DateTime.Now - LastDownloadTime;
                        if ( deltaTime.TotalHours > PrivatePrayerConfig.PrayerDownloadFrequency.TotalHours )
                        {
                            Console.WriteLine( "Downloading Prayers" );
                            DownloadPrayers( );
                        }
                        else
                        {
                            Console.WriteLine( "Not downloading prayers" );

                            // We already have cached prayers, so simply restore them.
                            StatusLayer.Visibility = ViewStates.Invisible;
                            ResultLayer.Visibility = ViewStates.Invisible;
                            RetryButton.Visibility = ViewStates.Invisible;

                            // setup the carousel again
                            Carousel.Clear( );
                            foreach ( PrayerCard prayerCard in PrayerRequestCards )
                            {
                                Carousel.AddCard( prayerCard.View );
                            }

                            // prayers received and are being viewed
                            PrayerAnalytic.Instance.Trigger( PrayerAnalytic.Read );
                        }

                        LayoutChanged( );
                    }
                }

                void DownloadPrayers( )
                {
                    // protect against double requests
                    IsRequesting = true;

                    ActivityIndicator.Visibility = ViewStates.Visible;

                    // let them know we're attempting to download the prayers
                    StatusLayer.Visibility = ViewStates.Visible;
                    StatusText.Text = PrayerStrings.ViewPrayer_StatusText_Retrieving;

                    // when downloading prayers, make sure the result and retry are invisible
                    // until we have a result.
                    ResultLayer.Visibility = ViewStates.Invisible;
                    RetryButton.Visibility = ViewStates.Invisible;

                    // request the prayers each time this appears
                    App.Shared.Network.RockApi.Instance.GetPrayers( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.PrayerRequest> prayerRequests )
                        {
                            IsRequesting = false;

                            PrayerRequestCards.Clear( );

                            // only process this if the view is still active. It's possible this request came in after we left the view.
                            if( IsActive == true )
                            {
                                ActivityIndicator.Visibility = ViewStates.Invisible;

                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true && prayerRequests.Count > 0 )
                                {
                                    // sort the prayers based on prayer count (least prayed for first)
                                    prayerRequests.Sort( delegate(Rock.Client.PrayerRequest x, Rock.Client.PrayerRequest y) 
                                        {
                                            return x.PrayerCount < y.PrayerCount ? -1 : 1;
                                        });
                                    
                                    LastDownloadTime = DateTime.Now;

                                    // success, so hide the status layer, we don't need it
                                    StatusLayer.Visibility = ViewStates.Invisible;

                                    // create our prayer request layouts
                                    foreach ( Rock.Client.PrayerRequest request in prayerRequests )
                                    {
                                        PrayerCard prayerCard = new PrayerCard( request, PrayerCardSize );
                                        PrayerRequestCards.Add( prayerCard );

                                        Carousel.AddCard( prayerCard.View );
                                    }

                                    // prayers received and are being viewed
                                    PrayerAnalytic.Instance.Trigger( PrayerAnalytic.Read );
                                }
                                else
                                {
                                    StatusLayer.Visibility = ViewStates.Visible;

                                    ResultLayer.Visibility = ViewStates.Visible;
                                    RetryButton.Visibility = ViewStates.Visible;

                                    StatusText.Text = PrayerStrings.ViewPrayer_StatusText_Failed;
                                    ResultText.Text = PrayerStrings.Error_Retrieve_Message;
                                }
                            }
                        } );
                }

                // forward these to the carousel
                public override bool OnFlingGesture( MotionEvent e1, MotionEvent e2, float velocityX, float velocityY )
                {
                    //Console.WriteLine( "OnFlingGesture" );
                    return ( (DroidCardCarousel)Carousel ).OnFling( e1, e2, velocityX, velocityY );
                }

                public override bool OnScrollGesture(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
                {
                    // let the center prayer scroll
                    PrayerRequestCards[ Carousel.CenterCardIndex ].Scroll( distanceY );

                    //Console.WriteLine( "OnScrollGesture" );
                    return ( (DroidCardCarousel)Carousel ).OnScroll( e1, e2, distanceX, distanceY );
                }

                public override bool OnDownGesture( MotionEvent e )
                {
                    //Console.WriteLine( "OnDownGesture" );
                    if ( IsRequesting == false )
                    {
                        Carousel.TouchesBegan( );
                    }
                    return false;
                }

                public override bool OnTouch( View v, MotionEvent e )
                {
                    //Console.WriteLine( "OnTouch" );

                    // if we're downloading prayers, don't process touch, because it causes a crash in android's gesture detector.
                    if ( IsRequesting == false )
                    {
                        if ( base.OnTouch( v, e ) == true )
                        {
                            return true;
                        }
                        else
                        {
                            switch ( e.Action )
                            {
                                case MotionEventActions.Up:
                                {
                                    Carousel.TouchesEnded( );
                                    break;
                                }
                            }

                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
