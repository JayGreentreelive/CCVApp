using System;
using System.Drawing;

namespace Notes
{
    public interface IUIControl
    {
        void AddOffset( float xOffset, float yOffset );

        System.Drawing.RectangleF GetFrame( );

        void AddToView( object obj );

        void RemoveFromView( object obj );

        void TouchesEnded( PointF touch );

        Styles.Alignment GetHorzAlignment( );
    }
}
