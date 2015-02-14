
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
using System.Drawing;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using CCVApp.Shared;
using CCVApp.Shared.Analytics;
using Rock.Mobile.PlatformSpecific.Android.Graphics;

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

                        int PrayerMaxScroll { get; set; }

                        public PrayerLayoutRender( RectangleF bounds, Rock.Client.PrayerRequest prayer )
                        {
                            // Create the core layout that stores the prayer
                            LinearLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            //LinearLayout.SetBackgroundColor( Android.Graphics.Color.Green );
                            LinearLayout.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            LinearLayout.Orientation = Orientation.Vertical;


                            // add the name
                            Name = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            Name.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            ((LinearLayout.LayoutParams)Name.LayoutParameters).LeftMargin = 10;
                            Name.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                            Name.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                            Name.SetTextSize( ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                            Name.Text = prayer.FirstName;
                            LinearLayout.AddView( Name );



                            // create the layout for managing the category / date
                            LinearLayout detailsLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            detailsLayout.Orientation = Orientation.Horizontal;
                            detailsLayout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                            LinearLayout.AddView( detailsLayout );

                            // category
                            Category = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            Category.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            ((LinearLayout.LayoutParams)Category.LayoutParameters).LeftMargin = 10;
                            Category.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                            Category.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                            Category.SetTextSize( ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                            Category.Text = "Category";
                            detailsLayout.AddView( Category );

                            // add a dummy view that will force the date over to the right
                            View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                            ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                            detailsLayout.AddView( dummyView );

                            // date
                            Date = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            Date.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            ((LinearLayout.LayoutParams)Date.LayoutParameters).RightMargin = 10;
                            Date.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                            Date.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                            Date.SetTextSize( ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                            Date.Text = string.Format( "{0:MM/dd/yy}", prayer.CreatedDateTime );
                            detailsLayout.AddView( Date );

                            // actual prayer
                            Prayer = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                            Prayer.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                            //((LinearLayout.LayoutParams)Prayer.LayoutParameters).LeftMargin = 10;
                            //((LinearLayout.LayoutParams)Prayer.LayoutParameters).RightMargin = 10;
                            //Prayer.SetMinWidth( (int)bounds.Width );
                            //Prayer.SetMaxWidth( (int)bounds.Width );
                            Prayer.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                            Prayer.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Medium_Font_Regular ), TypefaceStyle.Normal );
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
                                int availableHeight = LinearLayout.Height - (int)Prayer.GetY( );
                                Prayer.ScrollY = Math.Max( 0, Math.Min( Prayer.ScrollY, Prayer.MeasuredHeight - availableHeight ) );
                            }
                        }
                    }

                    PrayerLayoutRender PrayerLayout { get; set; }
                    Rock.Client.PrayerRequest PrayerRequest { get; set; }
                    public PlatformView View { get; set; }
                    Button Pray { get; set; }
                    bool Prayed { get; set; }

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
                        LinearLayout root = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        //root.SetBackgroundColor( Android.Graphics.Color.GreenYellow );
                        root.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                        root.Orientation = Orientation.Vertical;

                        // use a dummy view to pad the root and force the "I Prayed" label to the bottom area
                        View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        //dummyView.SetBackgroundColor( Android.Graphics.Color.Orange );
                        dummyView.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                        ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 10;
                        root.AddView( dummyView );


                        // create the bottom prayer layout
                        RelativeLayout frameLayout = new RelativeLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        //frameLayout.SetBackgroundColor( Android.Graphics.Color.Aqua );
                        frameLayout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                        ((LinearLayout.LayoutParams)frameLayout.LayoutParameters).Weight = 1;
                        root.AddView( frameLayout );


                        // Pray Button
                        Pray = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        Pray.Enabled = false;
                        Pray.SetBackgroundDrawable( null );
                        Pray.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                        frameLayout.AddView( Pray );


                        // Layout for the text and circle
                        LinearLayout prayedLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        //prayedLayout.SetBackgroundColor( Android.Graphics.Color.Bisque );
                        prayedLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                        ( (RelativeLayout.LayoutParams)prayedLayout.LayoutParameters ).AddRule( LayoutRules.AlignParentRight );
                        prayedLayout.Orientation = Orientation.Horizontal;
                        frameLayout.AddView( prayedLayout );



                        // Setup the "I Prayed" label
                        TextView prayedLabel = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        //prayedLabel.SetBackgroundColor( Android.Graphics.Color.Green );
                        prayedLabel.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent );
                        prayedLabel.Text = PrayerStrings.Prayer_Confirm;
                        prayedLabel.Gravity = GravityFlags.Center;
                        prayedLabel.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                        prayedLayout.AddView( prayedLabel );

                        Rock.Mobile.PlatformSpecific.Android.Graphics.CircleView tappedCircle = new Rock.Mobile.PlatformSpecific.Android.Graphics.CircleView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        tappedCircle.LayoutParameters = new LinearLayout.LayoutParams( 75, ViewGroup.LayoutParams.MatchParent );
                        //tappedCircle.SetBackgroundColor( Android.Graphics.Color.Purple );
                        tappedCircle.Color = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor );
                        tappedCircle.Radius = 8;
                        tappedCircle.StrokeWidth = 1;
                        //tappedCircle.Style = Android.Graphics.Paint.Style.FillAndStroke;
                        prayedLayout.AddView( tappedCircle );

                        Pray.Click += (object sender, EventArgs e) => 
                            {
                                Prayed = true;

                                CCVApp.Shared.Network.RockApi.Instance.IncrementPrayerCount( PrayerRequest.Id, null );

                                tappedCircle.Style = Android.Graphics.Paint.Style.FillAndStroke;
                                tappedCircle.Invalidate( );
                            };
                        //


                        // add the controls
                        ViewGroup nativeView = View.PlatformNativeObject as ViewGroup;
                        nativeView.AddView( root );


                        // add it to this view
                        PrayerLayout = new PrayerLayoutRender( bounds, prayer );
                        nativeView.AddView( PrayerLayout.LinearLayout );

                        Pray.Enabled = true;
                    }

                    public void Scroll( float distanceY )
                    {
                        // only process scrolling if we have an active prayer. (We won't if the prayers are still downloading)
                        if ( PrayerLayout != null )
                        {
                            PrayerLayout.Scroll( distanceY );
                        }
                    }
                }

                List<PrayerCard> PrayerRequests { get; set; }

                PlatformCardCarousel Carousel { get; set; }

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

                    view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    ActivityIndicator = (ProgressBar)view.FindViewById<ProgressBar>( Resource.Id.prayer_primary_activityIndicator );
                    ActivityIndicator.Visibility = ViewStates.Invisible;

                    float viewRealHeight = this.Resources.DisplayMetrics.HeightPixels;

                    float cardSizePerc = .80f;
                    float cardWidth = this.Resources.DisplayMetrics.WidthPixels * cardSizePerc;
                    float cardHeight = viewRealHeight * cardSizePerc;

                    // setup the card positions to be to the offscreen to the left, centered on screen, and offscreen to the right
                    float cardYOffset = viewRealHeight * .03f;

                    PrayerCardSize = new RectangleF( 0, 0, cardWidth, cardHeight );

                    PrayerRequests = new List<PrayerCard>();

                    Carousel = PlatformCardCarousel.Create( view, cardWidth, cardHeight, new RectangleF( 0, cardYOffset, this.Resources.DisplayMetrics.WidthPixels, viewRealHeight ), PrayerConfig.Card_AnimationDuration );



                    // setup our error UI
                    StatusLayer = view.FindViewById<View>( Resource.Id.status_background );
                    ControlStyling.StyleBGLayer( StatusLayer );

                    StatusText = StatusLayer.FindViewById<TextView>( Resource.Id.text );
                    ControlStyling.StyleUILabel( StatusText, ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );

                    ResultLayer = view.FindViewById<View>( Resource.Id.result_background );
                    ControlStyling.StyleBGLayer( ResultLayer );

                    ResultSymbol = ResultLayer.FindViewById<TextView>( Resource.Id.resultSymbol );
                    ResultSymbol.SetTypeface( FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Primary ), TypefaceStyle.Normal );
                    ResultSymbol.SetTextSize( ComplexUnitType.Dip, PrayerConfig.PostPrayer_ResultSymbolSize );
                    ResultSymbol.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );

                    ResultText = ResultLayer.FindViewById<TextView>( Resource.Id.text );
                    ControlStyling.StyleUILabel( ResultText, ControlStylingConfig.Large_Font_Regular, ControlStylingConfig.Large_FontSize );

                    RetryButton = view.FindViewById<Button>( Resource.Id.retryButton );
                    ControlStyling.StyleButton( RetryButton, GeneralStrings.Retry, ControlStylingConfig.Large_Font_Regular, ControlStylingConfig.Large_FontSize );

                    RetryButton.Click += (object sender, EventArgs e ) =>
                        {
                            DownloadPrayers( );
                        };

                    return view;
                }

                public override void OnPause()
                {
                    base.OnPause();

                    IsActive = false;
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

                    DownloadPrayers( );
                }

                void DownloadPrayers( )
                {
                    // protect against double requests
                    if ( IsRequesting == false )
                    {
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
                        CCVApp.Shared.Network.RockApi.Instance.GetPrayers( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.PrayerRequest> prayerRequests )
                            {
                                IsRequesting = false;

                                PrayerRequests.Clear( );
                                Carousel.Clear( );

                                // only process this if the view is still active. It's possible this request came in after we left the view.
                                if( IsActive == true )
                                {
                                    ActivityIndicator.Visibility = ViewStates.Invisible;

                                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true && prayerRequests.Count > 0 )
                                    {
                                        // success, so hide the status layer, we don't need it
                                        StatusLayer.Visibility = ViewStates.Invisible;

                                        // create our prayer request layouts
                                        foreach ( Rock.Client.PrayerRequest request in prayerRequests )
                                        {
                                            PrayerCard prayerCard = new PrayerCard( request, PrayerCardSize );
                                            PrayerRequests.Add( prayerCard );

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
                    PrayerRequests[ Carousel.CenterCardIndex ].Scroll( distanceY );

                    //Console.WriteLine( "OnScrollGesture" );
                    return ( (DroidCardCarousel)Carousel ).OnScroll( e1, e2, distanceX, distanceY );
                }

                public override bool OnDownGesture( MotionEvent e )
                {
                    //Console.WriteLine( "OnDownGesture" );
                    Carousel.TouchesBegan( );
                    return false;
                }

                public override bool OnTouch( View v, MotionEvent e )
                {
                    //Console.WriteLine( "OnTouch" );
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
            }
        }
    }
}
