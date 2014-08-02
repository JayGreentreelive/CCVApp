using System;
using System.Drawing;

namespace Notes
{
    /// <summary>
    /// Interface with methods allowing abstracted management of UI Controls.
    /// </summary>
    public interface IUIControl
    {
        void AddOffset( float xOffset, float yOffset );

        System.Drawing.RectangleF GetFrame( );

        void AddToView( object obj );

        void RemoveFromView( object obj );

        void TouchesEnded( PointF touch );

        Styles.Alignment GetHorzAlignment( );

        bool ShouldShowBulletPoint();
    }
}
