namespace TwitchLeecher.Services.Interfaces {

    using System;
    using TwitchLeecher.Core.Models;

    public interface IProcessingService {

        String FFMPEGExe { get; }

        void ConcatParts( Action<String> log, Action<String> setStatus, Action<Double> setProgress, VodPlaylist vodPlaylist, String concatFile );

        void ConvertVideo( Action<String> log, Action<String> setStatus, Action<Double> setProgress, Action<Boolean> setIsIndeterminate, String sourceFile, String outputFile, CropInfo cropInfo );
    }
}