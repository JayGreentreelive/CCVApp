using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

namespace iOS
{
	partial class PrayerMainUIViewController : TaskUIViewController
	{
		public PrayerMainUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.AddGestureRecognizer( new UISwipeGestureRecognizer( swipe =>
                {
                    Console.WriteLine("Swiped");
                })
            );

            // request the prayers
            ActivityIndicator.Hidden = false;

            //CALL THIS
            //http://rock.ccvonline.com/api/prayerrequests?$filter=IsApproved eq true&$expand=Category
        }
	}
}
