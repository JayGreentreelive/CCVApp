
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
using Rock.Mobile.PlatformSpecific.Android.UI;

namespace Droid
{
    namespace Tasks
    {
        namespace Connect
        {
            public class ArrayAdapter : ListAdapter
            {
                ConnectPrimaryFragment ParentFragment { get; set; }

                public ArrayAdapter( ConnectPrimaryFragment parentFragment ) : base ( )
                {
                    ParentFragment = parentFragment;
                }

                public override int Count 
                {
                    get { return ParentFragment.LinkEntries.Count + 1; }
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    ListItemView returnedView = null;
                    if ( position == 0 )
                    {
                        returnedView = GetPrimaryView( convertView, parent );
                    }
                    else
                    {
                        returnedView = GetStandardView( position - 1, convertView, parent );
                    }

                    return base.AddView( returnedView );
                }

                ListItemView GetPrimaryView( View convertView, ViewGroup parent )
                {
                    PrimaryListItem primaryItem = convertView as PrimaryListItem;
                    if ( primaryItem == null )
                    {
                        primaryItem = new PrimaryListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    }

                    primaryItem.Billboard.SetImageBitmap( ParentFragment.Billboard );
                    primaryItem.Billboard.SetScaleType( ImageView.ScaleType.CenterCrop );

                    return primaryItem;
                }

                ListItemView GetStandardView( int position, View convertView, ViewGroup parent )
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

            public class PrimaryListItem : Rock.Mobile.PlatformSpecific.Android.UI.ListAdapter.ListItemView
            {
                // stuff that will be set by data
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Billboard { get; set; }
                public TextView Title { get; set; }
                //

                LinearLayout ButtonLayout { get; set; }

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
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = 5;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).BottomMargin = 5;
                    AddView( Title );
                }

                public override void Destroy( )
                {
                    Billboard.SetImageBitmap( null );
                }
            }

            public class ListItem : Rock.Mobile.PlatformSpecific.Android.UI.ListAdapter.ListItemView
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

                public override void Destroy( )
                {
                    Thumbnail.SetImageBitmap( null );
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
                        thumbnailStream.Dispose( );
                    }

                    // setup the main image billboard
                    System.IO.Stream assetStream = Activity.BaseContext.Assets.Open( ConnectConfig.MainPageHeaderImage );
                    Billboard = BitmapFactory.DecodeStream( assetStream );
                    assetStream.Dispose( );

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
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );
                }

                public override void OnPause( )
                {
                    base.OnPause( );

                    FragmentActive = false;
                }

                public override void OnDestroyView()
                {
                    base.OnDestroyView();

                    ( (ArrayAdapter)ListView.Adapter ).Destroy( );

                    // free bmp resources
                    Billboard.Dispose( );
                    Billboard = null;

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

