using System;
using System.Drawing;
using System.Collections.Generic;

namespace CCVApp
{
    namespace Shared
    {
        namespace Notes
        {
            /// <summary>
            /// Interface with methods allowing abstracted management of UI Controls.
            /// </summary>
            public interface IUIControl
            {
                void AddOffset( float xOffset, float yOffset );

                System.Drawing.RectangleF GetFrame( );

                void GetControlOfType<TControlType>( List<IUIControl> controlList ) where TControlType : class;

                void AddToView( object obj );

                void RemoveFromView( object obj );

                bool TouchesBegan( PointF touch );

                void TouchesMoved( PointF touch );

                bool TouchesEnded( PointF touch );

                Styles.Alignment GetHorzAlignment( );

                bool ShouldShowBulletPoint();
            }
        }
    }
}
