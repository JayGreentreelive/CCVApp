
#if __ANDROID__
using System;
using Android.Widget;
using Android.Graphics;
using Android.Animation;

namespace Notes
{
    namespace PlatformUI
    {
        namespace DroidNative
        {
            /// <summary>
            /// A subclassed TextView that allows fading in the text
            /// </summary>
            public class FadeTextView : TextView, Android.Animation.ValueAnimator.IAnimatorUpdateListener
            {
                public FadeTextView( Android.Content.Context context ) : base( context )
                {
                    MaskScale = 1.0f;

                    RenderBuffersValid = false;

                    TextPaint = new Paint();
                    TextTransform = new Matrix();
                }

                Bitmap RGBMask { get; set; }
                Bitmap AlphaMask { get; set; }

                bool RenderBuffersValid { get; set; }

                Bitmap TextBmp { get; set; }
                Paint TextPaint { get; set; }
                Matrix TextTransform { get; set; }

                Bitmap ResultBmp { get; set; }
                Android.Graphics.Canvas ResultCanvas { get; set; }

                int CurrHeight { get; set; }
                int CurrWidth { get; set; }

                float _MaskScale;
                public float MaskScale 
                { 
                    get 
                    {
                        return _MaskScale;
                    }

                    set 
                    {
                        _MaskScale = Math.Max(value, .01f);
                    }
                }

                public void AnimateMaskScale( float targetScale, long duration )
                {
                    float clampedValue = Math.Max(targetScale, .01f);

                    // setup an animation from our current mask scale to the new one.
                    ValueAnimator animator = ValueAnimator.OfFloat( _MaskScale, clampedValue);

                    animator.AddUpdateListener( this );
                    animator.SetDuration( duration );

                    animator.Start();
                }

                public void OnAnimationUpdate(ValueAnimator animation)
                {
                    _MaskScale = ((Java.Lang.Float)animation.GetAnimatedValue("")).FloatValue();
                    Invalidate();
                }

                public void CreateAlphaMask( Android.Content.Context context, int sourceMaskId )
                {
                    // grab the RGB mask
                    RGBMask = BitmapFactory.DecodeResource( context.Resources, sourceMaskId );

                    // convert it to an alpha mask
                    AlphaMask = Bitmap.CreateBitmap( RGBMask.Width, RGBMask.Height, Bitmap.Config.Alpha8 );

                    // put the 8bit mask into a canvas
                    Android.Graphics.Canvas maskCanvas = new Android.Graphics.Canvas( AlphaMask );

                    // render the rgb mask into the canvas, which writes the result into the AlphaMask bitmap
                    maskCanvas.DrawBitmap( RGBMask, 0.0f, 0.0f, null );
                }

                protected void CreateTextBitmaps( int width, int height )
                {
                    // create a 32bit text bmp that will store the rendered text.
                    TextBmp = Bitmap.CreateBitmap( width, height, Bitmap.Config.Argb8888 );

                    // create a canvas and place the TextBmp as its target
                    Android.Graphics.Canvas canvas = new Android.Graphics.Canvas( TextBmp );

                    // render our text (which will put it into the TextBmp buffer)
                    base.OnDraw( canvas );

                    // set the TextPaint's shader to render with the Text we just rendered
                    TextPaint.SetShader( new BitmapShader( TextBmp, Shader.TileMode.Clamp, Shader.TileMode.Clamp ) );
                }

                protected void CreateResultBitmaps( int width, int height )
                {
                    // create the 32bit result bmp that we will render the text and alpha mask to
                    ResultBmp = Bitmap.CreateBitmap( width, height, Bitmap.Config.Argb8888 );

                    // store it in the canvas we'll use for rendering
                    ResultCanvas = new Android.Graphics.Canvas( ResultBmp );
                }

                protected override void OnDraw(Android.Graphics.Canvas canvas)
                {
                    // if our render buffers are not valid, generate them
                    if( CurrWidth != this.LayoutParameters.Width || CurrHeight != this.LayoutParameters.Height )
                    {
                        CreateTextBitmaps( this.LayoutParameters.Width, this.LayoutParameters.Height );
                        CreateResultBitmaps( this.LayoutParameters.Width, this.LayoutParameters.Height );

                        CurrWidth = this.LayoutParameters.Width;
                        CurrHeight = this.LayoutParameters.Height;
                    }


                    // keep the spot light centered on the image
                    float xPos = (LayoutParameters.Width / 2) - ((AlphaMask.Width / 2) * MaskScale);
                    float yPos = (LayoutParameters.Height / 2) - ((AlphaMask.Height / 2) * MaskScale);

                    // update the text's transform for the mask scale.
                    // The text's transform should be the inverse of the Canvas' values
                    TextTransform.SetScale( 1.0f / MaskScale, 1.0f / MaskScale );
                    TextTransform.PreTranslate( -xPos, -yPos );

                    TextPaint.Shader.SetLocalMatrix( TextTransform );

                    // clear the bitmap before re-rendering. (NOT EFFICIENT _AT_ _ALL_)
                    //ResultBmp.SetPixels( new int[ResultBmp.RowBytes * ResultBmp.Height], 0, ResultBmp.RowBytes, 0, 0, ResultBmp.Width, ResultBmp.Height );

                    // save / restore the canvas settings so we don't accumulate the scale
                    ResultCanvas.Save();
                    ResultCanvas.Translate( xPos, yPos );
                    ResultCanvas.Scale( MaskScale, MaskScale );

                    // render the alpha mask'd text to our result bmp
                    ResultCanvas.DrawBitmap( AlphaMask, 0, 0, TextPaint );

                    // restore the canvas values for next time.
                    ResultCanvas.Restore();

                    // and to the actual android buffer, render our result
                    canvas.DrawBitmap( ResultBmp, 0, 0, null);
                }
            }
        }
    }
}
#endif
