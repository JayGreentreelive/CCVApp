using System;
using Android.Views;
using CCVApp.Shared.Network;
using CCVApp.Shared.Config;
using CCVApp.Shared.PrivateConfig;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesTask : Task
            {
                NotesFragment NotesPage { get; set; }
                NotesPrimaryFragment MainPage { get; set; }
                NotesDetailsFragment DetailsPage { get; set; }
                NotesWatchFragment WatchPage { get; set; }
                NotesListenFragment ListenPage { get; set; }
                TaskWebFragment WebViewPage { get; set; }

                public NotesTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = new NotesPrimaryFragment( );
                    MainPage.ParentTask = this;

                    DetailsPage = new NotesDetailsFragment( );
                    DetailsPage.ParentTask = this;

                    NotesPage = new NotesFragment( );
                    NotesPage.ParentTask = this;

                    WatchPage = new NotesWatchFragment( );
                    WatchPage.ParentTask = this;

                    ListenPage = new NotesListenFragment( );
                    ListenPage.ParentTask = this;

                    WebViewPage = new TaskWebFragment( );
                    WebViewPage.ParentTask = this;
                }

                public override void SpringboardDidAnimate( bool springboardRevealed )
                {
                    // did the springboard just close?
                    if( springboardRevealed == false )
                    {
                        // if we weren't ready, let the notes know we now are.
                        if( TaskReadyForFragmentDisplay == false )
                        {
                            TaskReadyForFragmentDisplay = true;

                            NotesPage.TaskReadyForFragmentDisplay( );
                        }
                    }
                }

                public override void PerformTaskAction( string action )
                {
                    base.PerformTaskAction( action );

                    switch ( action )
                    {
                        case PrivateGeneralConfig.TaskAction_NotesRead:
                        {
                            if ( RockLaunchData.Instance.Data.NoteDB.SeriesList.Count > 0 )
                            {
                                NotesPage.NoteName = RockLaunchData.Instance.Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].Name;
                                NotesPage.NoteUrl = RockLaunchData.Instance.Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].NoteUrl;
                                NotesPage.StyleSheetDefaultHostDomain = RockLaunchData.Instance.Data.NoteDB.HostDomain;

                                PresentFragment( NotesPage, true );
                            }
                            break;
                        }

                        case PrivateGeneralConfig.TaskAction_NotesDownloadImages:
                        {
                            MainPage.DownloadImages( );
                            break;
                        }
                    }
                }

                public override void Activate( bool forResume )
                {
                    base.Activate( forResume );

                    // if the springboard is already closed, set ourselves as ready.
                    // This is always called before any fragment methods, so the fragment
                    // will be able to know if it can display or not.

                    // alternatively, if we're simply resuming from a pause, it's ok to allow the note to show.
                    if( NavbarFragment.ShouldTaskAllowInput( ) || forResume == true)
                    {
                        TaskReadyForFragmentDisplay = true;
                    }
                    else
                    {
                        TaskReadyForFragmentDisplay = false;
                    }
                }

                public override void Deactivate( bool forPause )
                {
                    base.Deactivate( forPause );

                    TaskReadyForFragmentDisplay = false;
                }

                public override bool CanContainerPan()
                {
                    if ( NotesPage.IsVisible == true )
                    {
                        return NotesPage.MovingUserNote ? false : true;
                    }
                    else
                    {
                        return true;
                    }
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnClick(Android.App.Fragment source, int buttonId, object context = null)
                {
                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // decide what to do.
                        if ( source == MainPage )
                        {
                            // on the main page, if the buttonId was -1, the user tapped the header,
                            // so we need to either go to the Watch or Take Notes page
                            if ( buttonId == -1 )
                            {
                                // the context is the button they clicked (watch or take notes)
                                int buttonChoice = (int)context;

                                // 0 is listen
                                if ( buttonChoice == 0 )
                                {
                                    ListenPage.MediaUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].AudioUrl;
                                    ListenPage.ShareUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].ShareUrl;
                                    ListenPage.Name = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                                    PresentFragment( ListenPage, true );
                                }
                                // 1 is watch
                                else if ( buttonChoice == 1 )
                                {
                                    WatchPage.MediaUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].WatchUrl;
                                    WatchPage.ShareUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].ShareUrl;
                                    WatchPage.Name = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                                    PresentFragment( WatchPage, true );
                                }
                                // 2 is read
                                else if ( buttonChoice == 2 )
                                {
                                    NotesPage.NoteUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].NoteUrl;
                                    NotesPage.NoteName = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                                    NotesPage.StyleSheetDefaultHostDomain = RockLaunchData.Instance.Data.NoteDB.HostDomain;

                                    PresentFragment( NotesPage, true );
                                }
                            }
                            else
                            {
                                DetailsPage.Series = MainPage.SeriesEntries[ buttonId ].Series;
                                DetailsPage.SeriesBillboard = MainPage.GetSeriesBillboard( buttonId );
                                PresentFragment( DetailsPage, true );
                            }
                        }
                        else if ( source == DetailsPage )
                        {
                            // the context is the button they clicked (watch or take notes)
                            int buttonChoice = (int)context;

                            // 0 is listen
                            if ( buttonChoice == 0 )
                            {
                                ListenPage.MediaUrl = DetailsPage.Messages[ buttonId ].Message.AudioUrl;
                                ListenPage.ShareUrl = DetailsPage.Messages[ buttonId ].Message.ShareUrl;
                                ListenPage.Name = DetailsPage.Messages[ buttonId ].Message.Name;
                                PresentFragment( ListenPage, true );
                            }
                            // 1 is watch
                            else if ( buttonChoice == 1 )
                            {
                                WatchPage.MediaUrl = DetailsPage.Messages[ buttonId ].Message.WatchUrl;
                                WatchPage.ShareUrl = DetailsPage.Messages[ buttonId ].Message.ShareUrl;
                                WatchPage.Name = DetailsPage.Messages[ buttonId ].Message.Name;
                                PresentFragment( WatchPage, true );
                            }
                            // 2 is read
                            else if ( buttonChoice == 2 )
                            {
                                NotesPage.NoteUrl = DetailsPage.Messages[ buttonId ].Message.NoteUrl;
                                NotesPage.NoteName = DetailsPage.Messages[ buttonId ].Message.Name;
                                NotesPage.StyleSheetDefaultHostDomain = RockLaunchData.Instance.Data.NoteDB.HostDomain;

                                PresentFragment( NotesPage, true );
                            }
                        }
                        else if ( source == NotesPage )
                        {
                            // the context is the activeURL to visit.
                            WebViewPage.DisplayUrl( (string)context );

                            PresentFragment( WebViewPage, true );
                        }
                    }
                }

                public override void OnUp( MotionEvent e )
                {
                    base.OnUp( e );
                }
            }
        }
    }
}

