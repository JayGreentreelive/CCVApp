using System;
using CCVApp.Shared;
using Rock.Mobile.PlatformSpecific.Util;
using CCVApp.Shared.Config;
using UIKit;
using Foundation;
using CCVApp.Shared.UI;

namespace iOS
{
    public class GroupFinderJoinViewController : TaskUIViewController
    {
        public string GroupTitle { get; set; }
        public string Distance { get; set; }
        public string MeetingTime { get; set; }
        public int GroupID { get; set; }

        UIJoinGroup JoinGroupView { get; set; }

        UIScrollViewWrapper ScrollView { get; set; }
        UITextField CellPhoneTextField { get; set; }
        
        public GroupFinderJoinViewController( )
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            ScrollView = new UIScrollViewWrapper();
            ScrollView.Parent = this;
            ScrollView.Frame = View.Frame;
            View.AddSubview( ScrollView );

            JoinGroupView = new UIJoinGroup();
            JoinGroupView.Create( ScrollView, View.Frame.ToRectF( ) );


            // since we're using the platform UI, we need to manually hook up the phone formatting delegate,
            // because that isn't implemented in platform abstracted code.
            CellPhoneTextField = (UITextField)JoinGroupView.CellPhone.PlatformNativeObject;
            CellPhoneTextField.Delegate = new Rock.Mobile.PlatformSpecific.iOS.UI.PhoneNumberFormatterDelegate();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            JoinGroupView.DisplayView( GroupTitle, Distance, MeetingTime, GroupID );

            // force the cell phone field to update itself so it contains proper formatting
            CellPhoneTextField.Delegate.ShouldChangeCharacters( CellPhoneTextField, new NSRange( CellPhoneTextField.Text.Length, 0 ), "" );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            ScrollView.ContentSize = new CoreGraphics.CGSize( View.Frame.Width, View.Frame.Height * 1.25f );
        }
    }
}

