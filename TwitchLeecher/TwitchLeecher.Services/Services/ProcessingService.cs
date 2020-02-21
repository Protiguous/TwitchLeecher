namespace TwitchLeecher.Services.Services {

    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using TwitchLeecher.Core.Models;
    using TwitchLeecher.Services.Interfaces;
    using TwitchLeecher.Shared.Helpers;
    using TwitchLeecher.Shared.IO;

    internal class ProcessingService : IProcessingService {

        private const String FFMPEG_EXE = "ffmpeg.exe";

        public String FFMPEGExe { get; }

        public ProcessingService() {
            var appDir = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            this.FFMPEGExe = Path.Combine( appDir, FFMPEG_EXE );
        }

        private void CheckOutputDirectory( Action<String> log, String outputDir ) {
            if ( !Directory.Exists( outputDir ) ) {
                log( Environment.NewLine + Environment.NewLine + "Creating output directory '" + outputDir + "'..." );
                FileSystem.CreateDirectory( outputDir );
                log( " done!" );
            }
        }

        public void ConcatParts( Action<String> log, Action<String> setStatus, Action<Double> setProgress, VodPlaylist vodPlaylist, String concatFile ) {
            setStatus( "Merging files" );
            setProgress( 0 );

            log( Environment.NewLine + Environment.NewLine + "Merging all VOD parts into '" + concatFile + "'..." );

            using ( FileStream outputStream = new FileStream( concatFile, FileMode.OpenOrCreate, FileAccess.Write ) ) {
                var partsCount = vodPlaylist.Count;

                for ( var i = 0; i < partsCount; i++ ) {
                    VodPlaylistPart part = vodPlaylist[ i ];

                    using ( FileStream partStream = new FileStream( part.LocalFile, FileMode.Open, FileAccess.Read ) ) {
                        Int32 maxBytes;
                        Byte[] buffer = new Byte[ 4096 ];

                        while ( ( maxBytes = partStream.Read( buffer, 0, buffer.Length ) ) > 0 ) {
                            outputStream.Write( buffer, 0, maxBytes );
                        }
                    }

                    FileSystem.DeleteFile( part.LocalFile );

                    setProgress( i * 100 / partsCount );
                }
            }

            setProgress( 100 );
        }

        public void ConvertVideo( Action<String> log, Action<String> setStatus, Action<Double> setProgress,
            Action<Boolean> setIsIndeterminate, String sourceFile, String outputFile, CropInfo cropInfo ) {
            setStatus( "Converting Video" );
            setIsIndeterminate( true );

            this.CheckOutputDirectory( log, Path.GetDirectoryName( outputFile ) );

            log( Environment.NewLine + Environment.NewLine + "Executing '" + this.FFMPEGExe + "' on '" + sourceFile + "'..." );

            ProcessStartInfo psi = new ProcessStartInfo( this.FFMPEGExe ) {
                Arguments = "-y" + ( cropInfo.CropStart ? " -ss " + cropInfo.Start.ToString( CultureInfo.InvariantCulture ) : null ) + " -i \"" + sourceFile + "\" -analyzeduration " + Int32.MaxValue + " -probesize " + Int32.MaxValue + " -c:v copy" + ( cropInfo.CropEnd ? " -t " + cropInfo.Length.ToString( CultureInfo.InvariantCulture ) : null ) + " \"" + outputFile + "\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            log( Environment.NewLine + "Command line arguments: " + psi.Arguments + Environment.NewLine );

            using ( Process p = new Process() ) {
                FixedSizeQueue<String> logQueue = new FixedSizeQueue<String>( 200 );

                TimeSpan duration = TimeSpan.FromSeconds( cropInfo.Length );

                DataReceivedEventHandler outputDataReceived = new DataReceivedEventHandler( ( s, e ) => {
                    try {
                        if ( !String.IsNullOrWhiteSpace( e.Data ) ) {
                            var dataTrimmed = e.Data.Trim();

                            logQueue.Enqueue( dataTrimmed );

                            if ( dataTrimmed.StartsWith( "frame", StringComparison.OrdinalIgnoreCase ) && duration != TimeSpan.Zero ) {
                                var timeStr = dataTrimmed.Substring( dataTrimmed.IndexOf( "time" ) + 4 ).Trim();
                                timeStr = timeStr.Substring( timeStr.IndexOf( "=" ) + 1 ).Trim();
                                timeStr = timeStr.Substring( 0, timeStr.IndexOf( " " ) ).Trim();

                                if ( TimeSpan.TryParse( timeStr, out TimeSpan current ) ) {
                                    setIsIndeterminate( false );
                                    setProgress( current.TotalMilliseconds / duration.TotalMilliseconds * 100 );
                                }
                                else {
                                    setIsIndeterminate( true );
                                }
                            }
                        }
                    }
                    catch ( Exception ex ) {
                        log( Environment.NewLine + "An error occured while reading '" + this.FFMPEGExe + "' output stream!" + Environment.NewLine + Environment.NewLine + ex.ToString() );
                    }
                } );

                p.OutputDataReceived += outputDataReceived;
                p.ErrorDataReceived += outputDataReceived;
                p.StartInfo = psi;
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();

                if ( p.ExitCode == 0 ) {
                    log( Environment.NewLine + "Video conversion complete!" );
                }
                else {
                    if ( !logQueue.IsEmpty ) {
                        foreach ( var line in logQueue ) {
                            log( Environment.NewLine + line );
                        }
                    }

                    throw new ApplicationException( "An error occured while converting the video!" );
                }
            }
        }
    }
}