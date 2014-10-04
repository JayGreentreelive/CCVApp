
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
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Java.IO;

namespace Droid
{
    public class ImageCropFragment : Fragment, View.IOnTouchListener
    {
        /// <summary>
        /// The last touch position received. Used for calculating the delta
        /// movement when moving the CropView
        /// </summary>
        /// <value>The last tap position.</value>
        PointF LastTapPos { get; set; }

        /// <summary>
        /// The view representing the region of picture that will be kept when cropping.
        /// </summary>
        /// <value>The crop view.</value>
        View CropView { get; set; }

        PointF CropViewMinPos { get; set; }
        PointF CropViewMaxPos { get; set; }


        /// <summary>
        /// Scalar to convert from screen points to image pixels and back
        /// </summary>
        float ScreenToImageScalar { get; set; }

        /// <summary>
        /// The image we're cropping
        /// </summary>
        /// <value>The source image.</value>
        Android.Graphics.Bitmap SourceImage { get; set; }

        /// <summary>
        /// The source image scaled to fit the screen
        /// </summary>
        /// <value>The scaled source image.</value>
        Android.Graphics.Bitmap ScaledSourceImage { get; set; }

        /// <summary>
        /// The resulting cropped image
        /// </summary>
        /// <value>The cropped image.</value>
        Android.Graphics.Bitmap CroppedImage { get; set; }

        /// <summary>
        /// The resulting cropped image scaled to fit the screen
        /// </summary>
        /// <value>The scaled cropped image.</value>
        Android.Graphics.Bitmap ScaledCroppedImage { get; set; }

        /// <summary>
        /// The view that displays the source/cropped image
        /// </summary>
        /// <value>The image view.</value>
        ImageView ImageView { get; set; }

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
        /// Callback when cropping is complete
        /// </summary>
        public delegate void ImageCropped( Android.Graphics.Bitmap croppedImage );
        ImageCropped ImageCroppedCallback;

        public override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );
        }

        public void Begin( Bitmap sourceImage, ImageCropped callback )
        {
            SourceImage = sourceImage;
            ImageCroppedCallback = callback;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            // scale the image to match the view's width
            ScreenToImageScalar = (float) SourceImage.Width / (float) container.Width;

            // get the scaled dimensions, maintaining aspect ratio
            float scaledWidth = (float)SourceImage.Width * (1.0f / ScreenToImageScalar);
            float scaledHeight = (float)SourceImage.Height * (1.0f / ScreenToImageScalar);

            // now, if the scaled height is too large, re-calc with Height is the dominant, 
            // so we guarantee a fit within the view.
            if( scaledHeight > container.Height )
            {
                ScreenToImageScalar = (float) SourceImage.Height / (float) container.Height;

                scaledWidth = (float)SourceImage.Width * (1.0f / ScreenToImageScalar);
                scaledHeight = (float)SourceImage.Height * (1.0f / ScreenToImageScalar);
            }

            // most important step, create our scalar since the image is going to be scaled down to fit the screen
            // our scalar should be based on the largest dimension
            /*if( container.Height < container.Width )
            {
                ScreenToImageScalar = (float)SourceImage.Height / (float)container.Height;
            }
            else
            {
                ScreenToImageScalar = (float)SourceImage.Width / (float)container.Width;
            }

            // now create a scaled version of the source image so it fits our screen.
            float scaledWidth = (float)SourceImage.Width * (1.0f / ScreenToImageScalar);
            float scaledHeight = (float)SourceImage.Height * (1.0f / ScreenToImageScalar);*/
            ScaledSourceImage = Bitmap.CreateScaledBitmap( SourceImage, (int)scaledWidth, (int)scaledHeight, false );


            // setup our layout for touch input
            RelativeLayout view = inflater.Inflate( Resource.Layout.ImageCrop, container, false ) as RelativeLayout;
            view.SetOnTouchListener( this );


            // create the view that will display the image to crop
            ImageView = new ImageView( Rock.Mobile.PlatformCommon.Droid.Context );
            ImageView.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent );
            ((RelativeLayout.LayoutParams)ImageView.LayoutParameters).AddRule( LayoutRules.CenterInParent );

            view.AddView( ImageView );

            // create the draggable crop view that will let the user pic which part of the image to use.
            CropView = new View( Rock.Mobile.PlatformCommon.Droid.Context );
            CropView.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );

            CropView.LayoutParameters.Width = (int) (scaledWidth < scaledHeight ? scaledWidth : scaledHeight);
            CropView.LayoutParameters.Height = CropView.LayoutParameters.Width; //yes WIDTH, we want to enforce a square

            // the crop view should be a nice outlined rounded rect
            float _Radius = 3.0f;
            RoundRectShape rectShape = new RoundRectShape( new float[] { _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius, 
                                                                         _Radius }, null, null );
            // configure its paint
            ShapeDrawable border = new ShapeDrawable( rectShape );
            border.Paint.SetStyle( Paint.Style.Stroke );
            border.Paint.StrokeWidth = 8;
            border.Paint.Color = Color.WhiteSmoke;
            CropView.SetBackgroundDrawable( border );


            // set our clamp values
            CropViewMinPos = new PointF( (container.Width - scaledWidth) / 2,
                                         (container.Height - scaledHeight) / 2 );

            CropViewMaxPos = new PointF( CropViewMinPos.X + (scaledWidth - CropView.LayoutParameters.Width),
                                         CropViewMinPos.Y + (scaledHeight - CropView.LayoutParameters.Height) );

            view.AddView( CropView );


            // Now setup our bottom area with cancel, crop, and text to explain
            LinearLayout LinearLayout = new LinearLayout( Rock.Mobile.PlatformCommon.Droid.Context );
            LinearLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
            ((RelativeLayout.LayoutParams)LinearLayout.LayoutParameters).AddRule( LayoutRules.AlignParentBottom );

            // set the nav subBar color (including opacity)
            Color navColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.SubNavToolbar.BackgroundColor );
            navColor.A = (Byte) ( (float) navColor.A * CCVApp.Shared.Config.SubNavToolbar.Opacity );
            LinearLayout.SetBackgroundColor( navColor );

            LinearLayout.LayoutParameters.Height = 150;
            view.AddView( LinearLayout );



            // setup the cancel button (which will undo cropping or take you back to the picture taker)
            Button cancelButton = new Button( Rock.Mobile.PlatformCommon.Droid.Context );
            cancelButton.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            cancelButton.Gravity = GravityFlags.Left;
            ((RelativeLayout.LayoutParams)cancelButton.LayoutParameters).AddRule( LayoutRules.AlignParentLeft );

            // set the crop button's font
            Android.Graphics.Typeface fontFace = Android.Graphics.Typeface.CreateFromAsset( Rock.Mobile.PlatformCommon.Droid.Context.Assets, "Fonts/" + CCVApp.Shared.Config.ImageCrop.CropCancelButton_Font + ".ttf" );
            cancelButton.SetTypeface( fontFace, Android.Graphics.TypefaceStyle.Normal );
            cancelButton.SetTextSize( Android.Util.ComplexUnitType.Dip, CCVApp.Shared.Config.ImageCrop.CropCancelButton_Size );
            cancelButton.Text = CCVApp.Shared.Config.ImageCrop.CropCancelButton_Text;

            cancelButton.Click += (object sender, EventArgs e) => 
                {
                    // if they hit cancel while previewing, go back to editing
                    if( Mode == CropMode.Previewing )
                    {
                        SetMode( CropMode.Editing );
                    }
                    else
                    {
                        // they pressed it while they're in crop mode, so go back to camera mode
                    }
                };

            LinearLayout.AddView( cancelButton );



            // setup the Confirm button, which will use a font to display its graphic
            Button confirmButton = new Button( Rock.Mobile.PlatformCommon.Droid.Context );
            confirmButton.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
            confirmButton.Gravity = GravityFlags.Right;
            ((RelativeLayout.LayoutParams)confirmButton.LayoutParameters).AddRule( LayoutRules.AlignParentRight );

            // set the crop button's font
            fontFace = Android.Graphics.Typeface.CreateFromAsset( Rock.Mobile.PlatformCommon.Droid.Context.Assets, "Fonts/" + CCVApp.Shared.Config.ImageCrop.CropOkButton_Font + ".ttf" );
            confirmButton.SetTypeface( fontFace, Android.Graphics.TypefaceStyle.Normal );
            confirmButton.SetTextSize( Android.Util.ComplexUnitType.Dip, CCVApp.Shared.Config.ImageCrop.CropOkButton_Size );
            confirmButton.Text = CCVApp.Shared.Config.ImageCrop.CropOkButton_Text;

            // when clicked, we should crop the image.
            confirmButton.Click += (object sender, EventArgs e) => 
                {
                    // if they pressed confirm while editing, go to preview
                    if( Mode == CropMode.Editing )
                    {
                        SetMode( CropMode.Previewing );
                    }
                    else
                    {
                        // they pressed it WHILE previewing, so we're done.
                        ImageCroppedCallback( CroppedImage );

                        // free our resources.
                        CroppedImage.Dispose( );
                        CroppedImage = null;

                        ScaledCroppedImage.Dispose( );
                        ScaledCroppedImage = null;

                        SourceImage = null;

                        ScaledSourceImage.Dispose( );
                        ScaledSourceImage = null;

                        ImageCroppedCallback = null;

                        // now free our resources, cause we're done.
                        Activity.OnBackPressed( );
                    }
                };

            LinearLayout.AddView( confirmButton );

            // start in editing mode (obviously)
            SetMode( CropMode.Editing );

            // force the crop view into correct position
            MoveCropView( new PointF( 0, 0 ) );

            return view;
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
                    // release any cropped image we had.
                    if ( CroppedImage != null )
                    {
                        CroppedImage.Dispose( );
                        CroppedImage = null;
                    }

                    // release the scaled version if we had it
                    if( ScaledCroppedImage != null )
                    {
                        ScaledCroppedImage.Dispose( );
                        ScaledCroppedImage = null;
                    }

                    // turn on our cropper
                    CropView.Visibility = ViewStates.Visible;

                    // and set the source image to the scaled source.
                    ImageView.SetImageBitmap( ScaledSourceImage );
                    break;
                }

                case CropMode.Previewing:
                {
                    // create the cropped image
                    CroppedImage = CropImage( SourceImage, new System.Drawing.RectangleF( CropView.GetX( ) - CropViewMinPos.X, 
                                                                                          CropView.GetY( ) - CropViewMinPos.Y, 
                                                                                          CropView.LayoutParameters.Width, 
                                                                                          CropView.LayoutParameters.Height ) );

                    // create a scaled version of the cropped image
                    float scaledWidth = (float)CroppedImage.Width * (1.0f / ScreenToImageScalar);
                    float scaledHeight = (float)CroppedImage.Height * (1.0f / ScreenToImageScalar);
                    ScaledCroppedImage = Bitmap.CreateScaledBitmap( CroppedImage, (int)scaledWidth, (int)scaledHeight, false );

                    // set the scaled cropped image
                    ImageView.SetImageBitmap( ScaledCroppedImage );
                    ImageView.SetBackgroundColor( Color.Black );

                    // hide the cropper
                    CropView.Visibility = ViewStates.Gone;
                    break;
                }
            }

            Mode = mode;
        }

        public bool OnTouch( View v, MotionEvent e )
        {
            switch( e.Action )
            {
                case MotionEventActions.Down:
                {
                    // stamp our position so we can update the crop view
                    LastTapPos = new PointF( e.GetX( ), e.GetY( ) );
                    break;
                }

                case MotionEventActions.Move:
                {
                    // adjust by the amount moved
                    PointF delta = new PointF( e.GetX( ) - LastTapPos.X, e.GetY( ) - LastTapPos.Y );

                    MoveCropView( delta );

                    LastTapPos = new PointF( e.GetX( ), e.GetY( ) );
                    break;
                }

                case MotionEventActions.Up:
                {

                    break;
                }
            }
            return true;
        }

        void MoveCropView( PointF delta )
        {
            // update the crop view by how much it should be moved
            float xPos = CropView.GetX( ) + delta.X;
            float yPos = CropView.GetY( ) + delta.Y;

            // clamp to valid bounds
            xPos = Math.Max( CropViewMinPos.X, Math.Min( xPos, CropViewMaxPos.X ) );
            yPos = Math.Max( CropViewMinPos.Y, Math.Min( yPos, CropViewMaxPos.Y ) );

            CropView.SetX( xPos );
            CropView.SetY( yPos );
        }

        Bitmap CropImage( Bitmap image, System.Drawing.RectangleF cropDimension )
        {
            // convert our position on screen to where it should be in the image
            float pixelX = cropDimension.X * ScreenToImageScalar;
            float pixelY = cropDimension.Y * ScreenToImageScalar;

            // same for height, since the image was scaled down to fit the screen.
            float width = (float) cropDimension.Width * ScreenToImageScalar;
            float height = (float) cropDimension.Height * ScreenToImageScalar;

            return Bitmap.CreateBitmap( image, (int) pixelX, (int) pixelY, (int)width, (int)height);
        }
    }
}
