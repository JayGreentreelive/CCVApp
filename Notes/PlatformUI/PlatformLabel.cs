using System;
using System.Drawing;

namespace Notes
{
    namespace PlatformUI
    {
        /// <summary>
        /// The base Platform Label that provides an interface to platform specific text labels.
        /// </summary>
        public abstract class PlatformLabel : PlatformBaseUI
        {
            public static PlatformLabel Create( )
            {
                #if __IOS__
                return new iOSLabel( );
                #endif

                #if __ANDROID__
                return new DroidLabel( );
                #endif
            }
        }
    }
}

