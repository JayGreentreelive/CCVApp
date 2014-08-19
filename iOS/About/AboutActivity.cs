using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace iOS
{
    public class AboutActivity : Activity
    {
        protected UIViewController MainPageVC { get; set; }
        protected UIViewController DetailsPageVC { get; set; }
        protected UIViewController MoreDetailsPageVC { get; set; }

        protected UIViewController CurrentVC { get; set; }

        public AboutActivity( string storyboardName ) : base( storyboardName )
        {
            PushTransition.AboutActivity = this;
            PopTransition.AboutActivity = this;

            MainPageVC = Storyboard.InstantiateViewController( "MainPageViewController" ) as UIViewController;
            DetailsPageVC = Storyboard.InstantiateViewController( "DetailsPageViewController" ) as UIViewController;
            MoreDetailsPageVC = Storyboard.InstantiateViewController( "MoreDetailsViewController" ) as UIViewController;
        }

        public override void MakeActive( UIViewController parentViewController )
        {
            base.MakeActive( parentViewController );

            // for now always make the main page the starting vc
            CurrentVC = MainPageVC;

            // start with the root controller
            ParentViewController.AddChildViewController( CurrentVC );
            ParentViewController.View.AddSubview( CurrentVC.View );
        }

        public virtual void PushViewController( UIViewController destViewController )
        {
            // add the new controller
            ParentViewController.AddChildViewController( destViewController );
            ParentViewController.View.AddSubview( destViewController.View );

            // position it just off screen
            destViewController.View.Layer.Position = new PointF( CurrentVC.View.Layer.Position.X + CurrentVC.View.Frame.Width, CurrentVC.View.Layer.Position.Y );

            // Animate the new VC in on top of the existing one.
            UIView.Animate( .30f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                new NSAction( 
                    delegate 
                    { 
                        destViewController.View.Layer.Position = new PointF( CurrentVC.View.Layer.Position.X, CurrentVC.View.Layer.Position.Y ); 
                    })

                , new NSAction(
                    delegate
                    {
                        // remove the one we're no longer viewing
                        CurrentVC.View.RemoveFromSuperview( );
                        CurrentVC.RemoveFromParentViewController( );

                        CurrentVC = destViewController;
                    })
            );
        }

        public virtual void PopViewController( UIViewController destViewController )
        {
            // add the new controller
            ParentViewController.AddChildViewController( destViewController );
            ParentViewController.View.AddSubview( destViewController.View );

            // position it on screen and behind the controller we'll pop off
            destViewController.View.Layer.Position = new PointF( CurrentVC.View.Layer.Position.X, CurrentVC.View.Layer.Position.Y );
            destViewController.View.Layer.ZPosition = CurrentVC.View.Layer.ZPosition - 1;

            // Animate the CURRENT vc off screen, which will reveal the view controller to show
            UIView.Animate( .30f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                new NSAction( 
                    delegate 
                    { 
                        CurrentVC.View.Layer.Position = new PointF( CurrentVC.View.Layer.Position.X + CurrentVC.View.Frame.Width, CurrentVC.View.Layer.Position.Y ); 
                    })

                , new NSAction(
                    delegate
                    {
                        // remove the one we're no longer viewing
                        CurrentVC.View.RemoveFromSuperview( );
                        CurrentVC.RemoveFromParentViewController( );

                        CurrentVC = destViewController;
                    })
            );
        }

        public override void OnResignActive( )
        {
            base.OnResignActive( );

            // remove any current one
            if( CurrentVC != null )
            {
                CurrentVC.View.RemoveFromSuperview( );
                CurrentVC.RemoveFromParentViewController( );

                CurrentVC = null;
            }
        }
    }

    [Register("PushTransition")]
    public class PushTransition : UIStoryboardSegue
    {
        public static AboutActivity AboutActivity { get; set; }

        public PushTransition (IntPtr param) : base (param)
        {

        }

        public override void Perform()
        {
            AboutActivity.PushViewController( DestinationViewController );
        }
    }

    [Register("PopTransition")]
    public class PopTransition : UIStoryboardSegue
    {
        public static AboutActivity AboutActivity { get; set; }

        public PopTransition (IntPtr param) : base (param)
        {

        }

        public override void Perform()
        {
            AboutActivity.PopViewController( DestinationViewController );
        }
    }
}

