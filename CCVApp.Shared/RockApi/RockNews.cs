using System;

namespace CCVApp
{
    namespace Shared
    {
        namespace Network
        {
            /// <summary>
            /// Contains a news item for the news display.
            /// </summary>
            public class RockNews
            {
                public string Title { get; set; }
                public string Description { get; set; }

                public RockNews( string title, string description )
                {
                    Title = title;
                    Description = description;
                }
            }
        }
    }
}

