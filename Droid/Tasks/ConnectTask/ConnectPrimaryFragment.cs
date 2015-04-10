
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
using Android.Media;
using Android.Graphics;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI.DroidNative;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using CCVApp.Shared;

namespace Droid
{
    namespace Tasks
    {
        namespace Connect
        {
            public class ArrayAdapter : BaseAdapter
            {
                ConnectPrimaryFragment ParentFragment { get; set; }

                public ArrayAdapter( ConnectPrimaryFragment parentFragment )
                {
                    ParentFragment = parentFragment;
                }

                public override int Count 
                {
                    get { return ParentFragment.LinkEntries.Count + 1; }
                }

                public override Java.Lang.Object GetItem (int position) 
                {
                    // could wrap a Contact in a Java.Lang.Object
                    // to return it here if needed
                    return null;
                }

                public override long GetItemId (int position) 
                {
                    return 0;
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    if ( position == 0 )
                    {
                        return GetPrimaryView( convertView, parent );
                    }
                    else
                    {
                        return GetStandardView( position - 1, convertView, parent );
                    }
                }

                View GetPrimaryView( View convertView, ViewGroup parent )
                {
                    PrimaryListItem primaryItem = convertView as PrimaryListItem;
                    if ( primaryItem == null )
                    {
                        primaryItem = new PrimaryListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    }

                    primaryItem.ParentAdapter = this;

                    primaryItem.Billboard.SetImageBitmap( ParentFragment.Billboard );
                    primaryItem.Billboard.SetScaleType( ImageView.ScaleType.CenterCrop );

                    return primaryItem;
                }

                View GetStandardView( int position, View convertView, ViewGroup parent )
                {
                    ListItem seriesItem = convertView as ListItem;
                    if ( seriesItem == null )
                    {
                        seriesItem = new ListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    }

                    seriesItem.Thumbnail.SetImageBitmap( ParentFragment.LinkBillboards[ position ] );
                    seriesItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );

                    seriesItem.Title.Text = ParentFragment.LinkEntries[ position ].Title;
                    return seriesItem;
                }
            }

            public class PrimaryListItem : LinearLayout
            {
                public ArrayAdapter ParentAdapter { get; set; }
                LinearLayout DetailsLayout { get; set; }

                // stuff that will be set by data
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Billboard { get; set; }
                public TextView Title { get; set; }
                //

                LinearLayout ButtonLayout { get; set; }

                //TextView Footer { get; set; }

                public PrimaryListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    Orientation = Android.Widget.Orientation.Vertical;

                    Billboard = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Billboard.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Billboard.SetScaleType( ImageView.ScaleType.CenterCrop );
                    AddView( Billboard );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Title.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    Title.Text = ConnectStrings.Main_Connect_Header;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = 25;
                    AddView( Title );

                    DetailsLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    DetailsLayout.Orientation = Android.Widget.Orientation.Horizontal;
                    DetailsLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).RightMargin = 25;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).BottomMargin = 50;
                    AddView( DetailsLayout );

                    // fill the remaining space with a dummy view, and that will align our speaker to the right
                    View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                    ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                    DetailsLayout.AddView( dummyView );

                    //Footer = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    //Footer.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    //( (LinearLayout.LayoutParams)Footer.LayoutParameters ).TopMargin = -5;
                    //Footer.Text = ConnectStrings.Main_Connect_OtherWays;
                    //Footer.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    //Footer.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    //Footer.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    //Footer.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Table_Footer_Color ) );
                    //Footer.Gravity = GravityFlags.Center;
                    //AddView( Footer );
                }
            }

            public class ListItem : LinearLayout
            {
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Thumbnail { get; set; }

                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView Chevron { get; set; }
                public View Seperator { get; set; }

                public ListItem( Context context ) : base( context )
                {
                    Orientation = Android.Widget.Orientation.Vertical;

                    SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    LinearLayout contentLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    contentLayout.Orientation = Android.Widget.Orientation.Horizontal;
                    contentLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    AddView( contentLayout );

                    Thumbnail = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Thumbnail.LayoutParameters = new LinearLayout.LayoutParams( (int)Rock.Mobile.Graphics.Util.UnitToPx( ConnectConfig.MainPage_ThumbnailDimension ), (int)Rock.Mobile.Graphics.Util.UnitToPx( ConnectConfig.MainPage_ThumbnailDimension ) );
                    ( (LinearLayout.LayoutParams)Thumbnail.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)Thumbnail.LayoutParameters ).BottomMargin = 25;
                    ( (LinearLayout.LayoutParams)Thumbnail.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    contentLayout.AddView( Thumbnail );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TitleLayout.Orientation = Android.Widget.Orientation.Vertical;
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = 25;
                    contentLayout.AddView( TitleLayout );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Medium_Font_Regular ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Title.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    Title.SetSingleLine( );
                    Title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    TitleLayout.AddView( Title );

                    // fill the remaining space with a dummy view, and that will align our chevron to the right
                    View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                    ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                    contentLayout.AddView( dummyView );

                    Chevron = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Chevron.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Chevron.LayoutParameters ).Gravity = GravityFlags.CenterVertical | GravityFlags.Right;
                    Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Secondary );
                    Chevron.SetTypeface(  fontFace, TypefaceStyle.Normal );
                    Chevron.SetTextSize( Android.Util.ComplexUnitType.Dip, ConnectConfig.MainPage_Table_IconSize );
                    Chevron.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Chevron.Text = ConnectConfig.MainPage_Table_Navigate_Icon;
                    contentLayout.AddView( Chevron );

                    // add our own custom seperator at the bottom
                    Seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    Seperator.LayoutParameters.Height = 2;
                    Seperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    AddView( Seperator );
                }
            }

            public class ConnectPrimaryFragment : TaskFragment
            {
                bool FragmentActive { get; set; }
                ListView ListView { get; set; }

                public Bitmap Billboard { get; set; }

                public List<ConnectLink> LinkEntries { get; set; }
                public List<Bitmap> LinkBillboards { get; set; }

                public ConnectPrimaryFragment( )
                {
                    LinkBillboards = new List<Bitmap>( );
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

                    LinkEntries = ConnectLink.BuildList( );

                    // insert group finder into the beginning of the list so it's always the first entry
                    ConnectLink groupFinderLink = new ConnectLink( );
                    groupFinderLink.Title = ConnectStrings.Main_Connect_GroupFinder;
                    groupFinderLink.ImageName = ConnectConfig.GroupFinder_IconImage;
                    LinkEntries.Insert( 0, groupFinderLink );

                    foreach ( ConnectLink link in LinkEntries )
                    {
                        // load each entry's thumbnail image
                        System.IO.Stream thumbnailStream = Activity.BaseContext.Assets.Open( link.ImageName );
                        LinkBillboards.Add( BitmapFactory.DecodeStream( thumbnailStream ) );
                    }

                    // setup the main image billboard
                    System.IO.Stream assetStream = Activity.BaseContext.Assets.Open( ConnectConfig.MainPageHeaderImage );
                    Billboard = BitmapFactory.DecodeStream( assetStream );

                    View view = inflater.Inflate(Resource.Layout.Connect_Primary, container, false);
                    view.SetOnTouchListener( this );

                    ListView = view.FindViewById<ListView>( Resource.Id.connect_primary_list );

                    ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e ) =>
                        {
                            // ignore clicks to the top banner
                            if( e.Position > 0 )
                            {
                                if( e.Position == 1 )
                                {
                                    ParentTask.OnClick( this, 0 );
                                }
                                else
                                {
                                    // if they clicked a non-groupfinder row, get the link they want to visit
                                    ParentTask.OnClick( this, e.Position - 1, LinkEntries[ e.Position - 1 ].Url );
                                }
                            }
                        };
                    ListView.SetOnTouchListener( this );

                    ListView.Adapter = new ArrayAdapter( this );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    FragmentActive = true;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( true );
                }

                public override void OnPause( )
                {
                    base.OnPause( );

                    FragmentActive = false;
                }

                public override void OnDestroyView()
                {
                    base.OnDestroyView();

                    // free bmp resources
                    Billboard.Dispose( );
                    foreach ( Bitmap image in LinkBillboards )
                    {
                        image.Dispose( );
                    }

                    LinkBillboards.Clear( );
                }
            }
        }
    }
}

