using System;
using System.Drawing;

namespace Notes
{
    namespace PlatformUI
    {
        public abstract class PlatformLabel : PlatformCommonUI
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

