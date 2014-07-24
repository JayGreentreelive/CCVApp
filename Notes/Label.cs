using System;
using MonoTouch.UIKit;
using System.Xml;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Drawing;

namespace Notes
{
    public class Label : BaseControl
    {
        protected UILabel PlatformLabel { get; set; }

        protected override void Initialize ()
        {
            base.Initialize ();

            // TODO: Update this to be like iBeacon, where it's platform abstracted
            PlatformLabel = new MonoTouch.UIKit.UILabel ();

            PlatformLabel.Layer.AnchorPoint = new PointF (0, 0);
            PlatformLabel.TextAlignment = UITextAlignment.Left;
        }

        protected Label()
        {
        }

        public Label (CreateParams parentParams, string text)
        {
            Initialize ();

            // check for attributes we support
            RectangleF bounds = new RectangleF ();

            // take our parent's style, and for anything not set by them use the default.
            mStyle = parentParams.Style;
            Styles.Style.MergeStyleAttributesWithDefaults(ref mStyle, ref ControlStyles.mText);

            PlatformLabel.Font = UIFont.FromName(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            PlatformLabel.TextColor = Styles.Style.GetUIColor(mStyle.mFont.mColor.Value);

            // set the dimensions and position
            if(bounds.Width == 0)
            {
                // always take the available width, in case this control
                // is specified to be offset relative to its parent
                bounds.Width = parentParams.Width - bounds.X;
            }
            PlatformLabel.Bounds = bounds;

            // get text
            SetText (text);

            // position ourselves in absolute coordinates, and trust our parent to offset us to be relative to them.
            PlatformLabel.Layer.Position = new PointF (bounds.X, bounds.Y);
        }

        protected void SetText (string text)
        {
            PlatformLabel.Text = text;

            // resize the label to fit the text
            PlatformLabel.SizeToFit ();
        }

        public override void AddOffset (float xOffset, float yOffset)
        {
            base.AddOffset (xOffset, yOffset);

            PlatformLabel.Layer.Position = new PointF (PlatformLabel.Layer.Position.X + xOffset, 
                PlatformLabel.Layer.Position.Y + yOffset);
        }

        public override void AddToView (object obj)
        {
            base.AddToView (obj);

            ((UIView)obj).AddSubview (PlatformLabel);
        }

        public override void RemoveFromView ()
        {
            base.RemoveFromView ();

            PlatformLabel.RemoveFromSuperview ();
        }

        public override RectangleF GetFrame ()
        {
            //base.DebugFrameView.Frame = PlatformLabel.Frame;
            return PlatformLabel.Frame;
        }
    }
}
