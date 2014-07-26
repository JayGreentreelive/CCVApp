using System;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using Notes.PlatformUI;

namespace Notes
{
    public class Label : BaseControl
    {
        protected PlatformLabel PlatformLabel { get; set; }

        protected override void Initialize ()
        {
            base.Initialize ();

            PlatformLabel = PlatformLabel.Create();
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

            PlatformLabel.SetFont(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            PlatformLabel.TextColor = mStyle.mFont.mColor.Value;

            if(mStyle.mBackgroundColor.HasValue)
            {
                PlatformLabel.BackgroundColor = mStyle.mBackgroundColor.Value;
            }

            // set the dimensions and position
            if(bounds.Width == 0)
            {
                // always take the available width, in case this control
                // is specified to be offset relative to its parent
                bounds.Width = parentParams.Width - bounds.X;
            }
            PlatformLabel.Bounds = bounds;

            // get text
            SetText(text);

            // position ourselves in absolute coordinates, and trust our parent to offset us to be relative to them.
            PlatformLabel.Position = new PointF (bounds.X, bounds.Y);
        }

        protected void SetText (string text)
        {
            PlatformLabel.Text = text;

            // resize the label to fit the text
            PlatformLabel.SizeToFit();
        }

        public override void AddOffset (float xOffset, float yOffset)
        {
            base.AddOffset (xOffset, yOffset);

            PlatformLabel.Position = new PointF (PlatformLabel.Position.X + xOffset, 
                                                 PlatformLabel.Position.Y + yOffset);
        }

        public override void AddToView (object obj)
        {
            base.AddToView (obj);

            PlatformLabel.AddAsSubview(obj);
        }

        public override void RemoveFromView ()
        {
            base.RemoveFromView ();

            PlatformLabel.RemoveAsSubview();
        }

        public override RectangleF GetFrame ()
        {
            return PlatformLabel.Frame;
        }
    }
}
