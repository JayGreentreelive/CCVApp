using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.CoreImage;
using MonoTouch.AssetsLibrary;
using CCVApp.Shared.Config;

namespace iOS
{
	partial class ImageCropViewController : UIViewController
	{
        public SpringboardViewController Springboard { get; set; }

        /// <summary>
        /// The source image to crop
        /// </summary>
        /// <value>The source image.</value>
        UIImage SourceImage { get; set; }

        /// <summary>
        /// The image view used to display the source and cropped images.
        /// </summary>
        /// <value>The image view.</value>
        UIImageView ImageView { get; set; }

        /// <summary>
        /// The view representing the region of the image to crop to.
        /// </summary>
        /// <value>The crop view.</value>
        UIView CropView { get; set; }

        /// <summary>
        /// Gets or sets the crop view minimum position.
        /// </summary>
        /// <value>The crop view minimum position.</value>
        PointF CropViewMinPos { get; set; }

        /// <summary>
        /// Gets or sets the crop view max position.
        /// </summary>
        /// <value>The crop view max position.</value>
        PointF CropViewMaxPos { get; set; }

        /// <summary>
        /// Scalar to convert from screen points to image pixels and back
        /// </summary>
        float ScreenToImageScalar { get; set; }

        /// <summary>
        /// The last touch position received. Used for calculating the delta
        /// movement when moving the CropView
        /// </summary>
        /// <value>The last tap position.</value>
        PointF LastTapPos { get; set; }

        /// <summary>
        /// The resulting cropped image
        /// </summary>
        /// <value>The cropped image.</value>
        UIImage CroppedImage { get; set; }

        /// <summary>
        /// Crop mode.
        /// </summary>
        enum CropMode
        {
            None,
            Editing,
            Previewing
        }

        /// <summary>
        /// Determines whether we're editing or previewing the crop
        /// </summary>
        /// <value>The mode.</value>
        CropMode Mode { get; set; }

        /// <summary>
        /// The aspect ratio we should be cropping the picture to.
        /// Example: 1.0f would mean 1:1 width/height, or a square.
        /// 9 / 16 would mean 9:16 (or 16:9), which is "wide screen" like a movie.
        /// </summary>
        /// <value>The crop aspect ratio.</value>
        float CropAspectRatio { get; set; }

        UIButton CancelButton { get; set; }
        UIButton EditButton { get; set; }

        UIView FullscreenBlocker { get; set; }
        CAShapeLayer FullscreenBlockerMask { get; set; }

		public ImageCropViewController (IntPtr handle) : base (handle)
		{
		}

        public void Begin( UIImage image, float cropAspectRatio )
        {
            CropAspectRatio = cropAspectRatio;

            SourceImage = image;
        }

        public override bool PrefersStatusBarHidden()
        {
            return Springboard.PrefersStatusBarHidden();
        }

        public override bool ShouldAutorotate()
        {
            return Springboard.ShouldAutorotate();
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations( )
        {
            return Springboard.GetSupportedInterfaceOrientations( );
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation( )
        {
            return Springboard.PreferredInterfaceOrientationForPresentation( );
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.Black;

            // set the image
            ImageView = new UIImageView( );
            ImageView.BackgroundColor = UIColor.Black;
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            ImageView.Frame = new RectangleF( View.Frame.X, View.Frame.Y, View.Frame.Width, View.Frame.Height );

            View.AddSubview( ImageView );


            // create our cropper
            CropView = new UIView( );
            CropView.BackgroundColor = UIColor.Clear;
            CropView.Layer.BorderColor = UIColor.White.CGColor;
            CropView.Layer.BorderWidth = 1;
            CropView.Layer.CornerRadius = 4;

            View.AddSubview( CropView );


            // create our fullscreen blocker. It needs to be HUGE so we can center the
            // masked part
            FullscreenBlocker = new UIView();
            FullscreenBlocker.BackgroundColor = UIColor.Black;
            FullscreenBlocker.Layer.Opacity = 0.00f;
            FullscreenBlocker.Bounds = new RectangleF( 0, 0, 10000, 10000 );
            FullscreenBlocker.AutoresizingMask = UIViewAutoresizing.None;
            View.AddSubview( FullscreenBlocker );

            FullscreenBlockerMask = new CAShapeLayer();
            FullscreenBlockerMask.FillRule = CAShapeLayer.FillRuleEvenOdd;
            FullscreenBlocker.Layer.Mask = FullscreenBlockerMask;


            // create our bottom toolbar
            UIToolbar toolbar = new UIToolbar( new RectangleF( 0, View.Bounds.Height - 40, View.Bounds.Width, 40 ) );

            // create the cancel button
            NSString cancelLabel = new NSString( ImageCropConfig.CropCancelButton_Text );

            CancelButton = new UIButton(UIButtonType.System);
            CancelButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Icon_Font_Primary, ImageCropConfig.CropCancelButton_Size );
            CancelButton.SetTitle( cancelLabel.ToString( ), UIControlState.Normal );

            SizeF buttonSize = cancelLabel.StringSize( CancelButton.Font );
            CancelButton.Bounds = new RectangleF( 0, 0, buttonSize.Width, buttonSize.Height );
            CancelButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    // if cancel was pressed while editing, cancel this entire operation
                    if( CropMode.Editing == Mode )
                    {
                        Springboard.ResignModelViewController( this, null );
                        Mode = CropMode.None;
                    }
                    else
                    {
                        SetMode( CropMode.Editing );
                    }
                };

            // create the edit button
            NSString editLabel = new NSString( ImageCropConfig.CropOkButton_Text );

            EditButton = new UIButton(UIButtonType.System);
            EditButton.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Icon_Font_Primary, ImageCropConfig.CropOkButton_Size );
            EditButton.SetTitle( editLabel.ToString( ), UIControlState.Normal );
            EditButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;

            // determine its dimensions
            buttonSize = editLabel.StringSize( EditButton.Font );
            EditButton.Bounds = new RectangleF( 0, 0, buttonSize.Width, buttonSize.Height );
            EditButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    if( Mode == CropMode.Previewing )
                    {
                        // confirm we're done
                        Springboard.ResignModelViewController( this, CroppedImage );
                        Mode = CropMode.None;
                    }
                    else
                    {
                        SetMode( CropMode.Previewing );
                    }
                };

            // create a container that will allow us to align the buttons
            UIView buttonContainer = new UIView( new RectangleF( 0, View.Bounds.Height - 40, View.Bounds.Width, 40 ) );
            buttonContainer.AddSubview( EditButton );
            buttonContainer.AddSubview( CancelButton );

            CancelButton.Frame = new RectangleF( (CancelButton.Frame.Width), 0, CancelButton.Frame.Width, CancelButton.Frame.Height );
            EditButton.Frame = new RectangleF( buttonContainer.Frame.Width - ((EditButton.Frame.Width / 2) + (EditButton.Frame.Width * 2)), 0, EditButton.Frame.Width, EditButton.Frame.Height );

            toolbar.SetItems( new UIBarButtonItem[] { new UIBarButtonItem( buttonContainer ) }, false );
            View.AddSubview( toolbar );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            AnimateBlocker( false );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // scale the image to match the view's width
            ScreenToImageScalar = (float)SourceImage.Size.Width / (float)View.Bounds.Width;

            // get the scaled dimensions, maintaining aspect ratio
            float scaledImageWidth = (float)SourceImage.Size.Width * (1.0f / ScreenToImageScalar);
            float scaledImageHeight = (float)SourceImage.Size.Height * (1.0f / ScreenToImageScalar);

            // calculate the image's starting X / Y location
            float imageStartX = ( ImageView.Frame.Width - scaledImageWidth ) / 2;
            float imageStartY = ( ImageView.Frame.Height - scaledImageHeight ) / 2;


            // now calculate the size of the cropper
            float cropperWidth = scaledImageWidth;
            float cropperHeight = scaledImageHeight;


            // get the image's aspect ratio so we can shrink down the cropper correctly
            float aspectRatio = SourceImage.Size.Width / SourceImage.Size.Height;

            // if the cropper should be wider than it is tall (or square)
            if ( CropAspectRatio <= 1.0f )
            {
                // then if the image is wider than it is tall, scale down the cropper's width
                if ( aspectRatio > 1.0f )
                {
                    cropperWidth *= 1 / aspectRatio;
                }

                // and the height should be scaled down from the width
                cropperHeight = cropperWidth * CropAspectRatio;
            }
            else
            {
                // the cropper should be taller than it is wide

                // so if the image is taller than it is wide, scale down the cropper's height
                if ( aspectRatio < 1.0f )
                {
                    cropperWidth *= 1 / aspectRatio;
                }

                // and the width should be scaled down from the height. (Invert CropAspectRatio since it was Width based)
                cropperWidth = cropperHeight * (1 / CropAspectRatio);
            }

            // set the crop bounds
            CropView.Frame = new RectangleF( View.Frame.X, View.Frame.Y, cropperWidth, cropperHeight );


            // Now set the min / max movement bounds for the cropper
            CropViewMinPos = new PointF( imageStartX, imageStartY );

            CropViewMaxPos = new PointF( ( imageStartX + scaledImageWidth ) - cropperWidth,
                                         ( imageStartY + scaledImageHeight ) - cropperHeight );

            // center the cropview
            CropView.Layer.Position = new PointF( 0, 0 );
            MoveCropView( new PointF( ImageView.Bounds.Width / 2, ImageView.Bounds.Height / 2 ) );



            // setup the mask that will reveal only the part of the image that will be cropped
            FullscreenBlocker.Layer.Opacity = 0.00f;
            UIBezierPath viewFill = UIBezierPath.FromRect( FullscreenBlocker.Bounds );
            UIBezierPath cropMask = UIBezierPath.FromRoundedRect( new RectangleF( ( FullscreenBlocker.Bounds.Width - CropView.Bounds.Width ) / 2, 
                                                                                  ( FullscreenBlocker.Bounds.Height - CropView.Bounds.Height ) / 2, 
                                                                                    CropView.Bounds.Width, 
                                                                                    CropView.Bounds.Height ), 4 );
            viewFill.AppendPath( cropMask );
            FullscreenBlockerMask.Path = viewFill.CGPath;

            // and set our source image
            ImageView.Image = SourceImage;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // start in editing mode (obviously)
            SetMode( CropMode.Editing );
        }

        void AnimateBlocker( bool visible )
        {
            // animate in the blocker
            UIView.Animate( .5f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                new NSAction( delegate
                    { 
                        FullscreenBlocker.Layer.Opacity = visible == true ? .60f : 0.00f;
                        CropView.Layer.Opacity = visible == true ? 1.00f : 0.00f;
                    } )
                , new NSAction( delegate
                    { 
                    } ) );
        }

        void MoveCropView( PointF delta )
        {
            // update the crop view by how much it should be moved
            float xPos = CropView.Frame.X + delta.X;
            float yPos = CropView.Frame.Y + delta.Y;

            // clamp to valid bounds
            xPos = Math.Max( CropViewMinPos.X, Math.Min( xPos, CropViewMaxPos.X ) );
            yPos = Math.Max( CropViewMinPos.Y, Math.Min( yPos, CropViewMaxPos.Y ) );

            CropView.Frame = new RectangleF( xPos, yPos, CropView.Frame.Width, CropView.Frame.Height );

            // update the position of the blocking view
            FullscreenBlocker.Center = CropView.Layer.Position;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                LastTapPos = touch.LocationInView( View );
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            // adjust by the amount moved
            UITouch touch = touches.AnyObject as UITouch;
            if( touch != null )
            {
                PointF touchPoint = touch.LocationInView( View );

                PointF delta = new PointF( touchPoint.X - LastTapPos.X, touchPoint.Y - LastTapPos.Y );

                MoveCropView( delta );

                LastTapPos = touchPoint;
            }
        }

        void SetMode( CropMode mode )
        {
            if( mode == Mode )
            {
                throw new Exception( string.Format( "Crop Mode {0} requested, but already in that mode.", mode ) );
            }

            switch( mode )
            {
                case CropMode.Editing:
                {
                    // if we're entering Editing for the first time, setup is simple.
                    if ( Mode == CropMode.None )
                    {
                        // fade in the blocker, which will help the user
                        // see what they're supposed to be doing
                        AnimateBlocker( true );
                    }
                    // If we're coming BACK from previewing
                    if ( Mode == CropMode.Previewing )
                    {
                        // Then we need to reverse the animation we played to crop the image.
                        UIView.Animate( .5f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                            // ANIMATING
                            new NSAction( delegate
                                { 
                                    // animate the cropped image down to its original size.
                                    ImageView.Frame = CropView.Frame;
                                } )
                            // DONE ANIMATING
                            , new NSAction( delegate
                                { 
                                    // done, so now set the original image (which will
                                    // seamlessly replace the cropped image)
                                    ImageView.Frame = View.Frame;
                                    ImageView.Image = SourceImage;

                                    // and turn on the blocker fully, so we still only see
                                    // the cropped part of the image
                                    FullscreenBlocker.Layer.Opacity = 1.00f;

                                    // and finally animate down the blocker, so it restores
                                    // the original editing appearance
                                    AnimateBlocker( true );
                                } ) 
                        );
                    }
                    break;
                }

                case CropMode.Previewing:
                {
                    // create the cropped image
                    CroppedImage = CropImage( SourceImage, new System.Drawing.RectangleF( CropView.Frame.X - CropViewMinPos.X, 
                                                                                          CropView.Frame.Y - CropViewMinPos.Y, 
                                                                                          CropView.Frame.Width , 
                                                                                          CropView.Frame.Height ) );


                    // Kick off an animation that will simulate cropping and scaling up the image.
                    UIView.Animate( .5f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                        // ANIMATING
                        new NSAction( delegate
                            { 
                                // animate in the blocker fully, which will black out the non-cropped parts of the image.
                                FullscreenBlocker.Layer.Opacity = 1.00f;

                                // fade out the cropper border
                                CropView.Layer.Opacity = 0.00f;
                            } )
                        // DONE ANIMATING
                        , new NSAction( delegate
                            { 
                                // set the scaled cropped image, seamlessly replacing full image
                                ImageView.Frame = CropView.Frame;
                                ImageView.Image = CroppedImage;

                                // and turn off the blocker completely (nothing to block now, since we're literally using
                                // the cropped image
                                FullscreenBlocker.Layer.Opacity = 0.00f;

                                // and kick off a final (chained) animation that scales UP the cropped image to fill the viewport. 
                                UIView.Animate( .5f, 0, UIViewAnimationOptions.CurveEaseInOut, 
                                    new NSAction( delegate
                                        { 
                                            ImageView.Frame = View.Frame;
                                        } ), null );
                            } ) );
                    break;
                }
            }

            Mode = mode;
        }

        UIImage CropImage( UIImage sourceImage, RectangleF cropDimension )
        {
            // step one, transform the crop region into image space.
            // (So pixelX is a pixel in the actual image, not the scaled screen)

            // convert our position on screen to where it should be in the image
            float pixelX = cropDimension.X * ScreenToImageScalar;
            float pixelY = cropDimension.Y * ScreenToImageScalar;

            // same for height, since the image was scaled down to fit the screen.
            float width = (float) cropDimension.Width * ScreenToImageScalar;
            float height = (float) cropDimension.Height * ScreenToImageScalar;


            // Now we're going to rotate the image to actually be "up" as the user
            // sees it. To do that, we simply rotate it according to the apple documentation.
            float rotationDegrees = 0.0f;

            switch ( sourceImage.Orientation )
            {
                case UIImageOrientation.Up:
                {
                    // don't do anything. The image space and the user space are 1:1
                    break;
                }
                case UIImageOrientation.Left:
                {
                    // the image space is rotated 90 degrees from user space,
                    // so do a CCW 90 degree rotation
                    rotationDegrees = 90.0f;
                    break;
                }
                case UIImageOrientation.Right:
                {
                    // the image space is rotated -90 degrees from user space,
                    // so do a CW 90 degree rotation
                    rotationDegrees = -90.0f;
                    break;
                }
                case UIImageOrientation.Down:
                {
                    rotationDegrees = 180;
                    break;
                }
            }
            
            // Now get a transform so we can rotate the image to be oriented the same as when the user previewed it
            CGAffineTransform fullImageTransform = GetImageTransformAboutCenter( rotationDegrees, sourceImage.Size );

            // apply to the image
            CIImage ciCorrectedImage = new CIImage( sourceImage.CGImage );
            CIImage ciCorrectedRotatedImage = ciCorrectedImage.ImageByApplyingTransform( fullImageTransform );

            // create a context and render it back out to a CGImage.
            CIContext ciContext = CIContext.FromOptions( null );
            CGImage rotatedCGImage = ciContext.CreateCGImage( ciCorrectedRotatedImage, ciCorrectedRotatedImage.Extent );

            // now the image is properly orientated, so we can crop it.
            RectangleF cropRegion = new RectangleF( pixelX, pixelY, width, height );
            CGImage croppedImage = rotatedCGImage.WithImageInRect( cropRegion );
            return new UIImage( croppedImage );
        }

        CGAffineTransform GetImageTransformAboutCenter( float angleDegrees, SizeF imageSize )
        {
            // Create a tranform that will rotate our image about its center
            CGAffineTransform transform = CGAffineTransform.MakeIdentity( );

            // setup our transform. Translate it by the image's half width/height so it rotates about its center.
            transform.Translate( -imageSize.Width / 2, -imageSize.Height / 2 );
            transform.Rotate( angleDegrees * Rock.Mobile.Math.Util.DegToRad );

            // now we need to concat on a post-transform that will put the image's pivot back at the top left.
            // get the image's dimensions transformed
            RectangleF transformedImageRect = transform.TransformRect( new RectangleF( 0, 0, imageSize.Width, imageSize.Height ) );

            // our post transform simply translates the image back
            CGAffineTransform postTransform = CGAffineTransform.MakeIdentity( );
            postTransform.Translate( transformedImageRect.Width / 2, transformedImageRect.Height / 2 );

            // now multiply the transform and postTranform and we have our final transform to use
            return CGAffineTransform.Multiply( transform, postTransform );
        }
	}
}
