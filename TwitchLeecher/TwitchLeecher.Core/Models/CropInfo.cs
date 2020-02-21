namespace TwitchLeecher.Core.Models {

    public class CropInfo {

        public System.Boolean CropEnd { get; private set; }

        public System.Boolean CropStart { get; private set; }

        public System.Double Length { get; private set; }

        public System.Double Start { get; private set; }

        public CropInfo( System.Boolean cropStart, System.Boolean cropEnd, System.Double start, System.Double length ) {
            this.CropStart = cropStart;
            this.CropEnd = cropEnd;
            this.Start = start;
            this.Length = length;
        }
    }
}