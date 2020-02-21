using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TwitchLeecher.Gui.Controls {
    [TemplatePart( Name = "Tpl_Part_PaintArea", Type = typeof( Shape ) ),
     TemplatePart( Name = "Tpl_Part_MainContent", Type = typeof( ContentControl ) )]
    public class TlFadeContentControl : ContentControl {

        private Shape _paintArea;
        private ContentControl _mainContent;

        private readonly DoubleAnimation _fadeInAnim;
        private readonly DoubleAnimation _fadeOutAnim;

        public TlFadeContentControl() {
            this._fadeInAnim = new DoubleAnimation( 0, 1, TimeSpan.FromMilliseconds( 200 ) );
            this._fadeOutAnim = new DoubleAnimation( 1, 0, TimeSpan.FromMilliseconds( 200 ) );

            this._fadeOutAnim.Completed += this.FadeOutAnim_Completed;
        }

        public override void OnApplyTemplate() {
            this._paintArea = this.Template.FindName( "Tpl_Part_PaintArea", this ) as Shape;
            this._mainContent = this.Template.FindName( "Tpl_Part_MainContent", this ) as ContentControl;

            base.OnApplyTemplate();
        }

        protected override void OnContentChanged( object oldContent, object newContent ) {
            if ( this._paintArea != null && this._mainContent != null ) {
                this._paintArea.Fill = this.CreateBrushFromVisual( this._mainContent );
                this.BeginAnimateContentReplacement();
            }

            base.OnContentChanged( oldContent, newContent );
        }

        private void BeginAnimateContentReplacement() {
            this._paintArea.Visibility = Visibility.Visible;
            this._mainContent.Visibility = Visibility.Hidden;
            this._paintArea.BeginAnimation( OpacityProperty, this._fadeOutAnim );
        }

        private Brush CreateBrushFromVisual( Visual visual ) {
            if ( visual == null ) {
                throw new ArgumentNullException( nameof( visual ) );
            }

            RenderTargetBitmap target = new RenderTargetBitmap( ( int ) this.ActualWidth, ( int ) this.ActualHeight, 96, 96, PixelFormats.Pbgra32 );
            target.Render( visual );

            ImageBrush brush = new ImageBrush( target );
            brush.Freeze();

            return brush;
        }

        private void FadeOutAnim_Completed( object sender, EventArgs e ) {
            this._paintArea.Visibility = Visibility.Hidden;
            this._mainContent.Visibility = Visibility.Visible;
            this._mainContent.BeginAnimation( OpacityProperty, this._fadeInAnim );
        }
    }
}