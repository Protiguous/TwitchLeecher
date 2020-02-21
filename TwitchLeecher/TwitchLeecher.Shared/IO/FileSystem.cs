namespace TwitchLeecher.Shared.IO {

    using System.IO;
    using System.Linq;

    public static class FileSystem {

        public static void CleanDirectory( System.String directory ) {
            DirectoryInfo dirInfo = new DirectoryInfo( directory );

            if ( dirInfo.Exists ) {
                foreach ( FileInfo file in dirInfo.GetFiles() ) {
                    DeleteFile( file.FullName );
                }

                foreach ( DirectoryInfo dir in dirInfo.GetDirectories() ) {
                    DeleteDirectory( dir.FullName );
                }
            }
        }

        public static void CopyFile( System.String sourceFile, System.String targetDir, System.String newFileName = null ) {
            CreateDirectory( targetDir );

            FileInfo fileInfo = new FileInfo( sourceFile );

            var targetFile = Path.Combine( targetDir, newFileName ?? fileInfo.Name );

            ResetFileAttributes( targetFile );

            File.Copy( fileInfo.FullName, targetFile, true );
        }

        public static void CreateDirectory( System.String directory ) {
            DirectoryInfo dirInfo = new DirectoryInfo( directory );

            if ( !dirInfo.Exists ) {
                dirInfo.Create();
            }
        }

        public static void DeleteDirectory( System.String directory ) {
            DirectoryInfo dirInfo = new DirectoryInfo( directory );

            if ( dirInfo.Exists ) {
                CleanDirectory( directory );
                dirInfo.Delete( true );
            }
        }

        public static void DeleteFile( System.String file ) {
            FileInfo fileInfo = new FileInfo( file );

            if ( fileInfo.Exists ) {
                ResetFileAttributes( fileInfo.FullName );
                fileInfo.Delete();
            }
        }

        public static System.Boolean FilenameContainsInvalidChars( System.String filename ) {
            if ( System.String.IsNullOrWhiteSpace( filename ) ) {
                return false;
            }

            foreach ( var c in Path.GetInvalidFileNameChars() ) {
                if ( filename.Contains( c ) ) {
                    return true;
                }
            }

            return false;
        }

        public static void ResetFileAttributes( System.String file ) {
            if ( File.Exists( file ) ) {
                File.SetAttributes( file, FileAttributes.Normal );
            }
        }
    }
}