using System;
using System.Windows;
using System.Windows.Controls;

namespace TwitchLeecher.Gui.Controls {
    public class TlSpacedUniformGrid : Panel {

        private double newItemWidth = 0;
        private double newItemHeight = 0;

        private int columnCount = 1;
        private int rowCount = 1;

        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
                "Spacing",
                typeof( double ),
                typeof( TlSpacedUniformGrid ), new FrameworkPropertyMetadata(
                        defaultValue: 10.0,
                        flags: FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange ) );

        public double Spacing {
            get { return ( double ) this.GetValue( SpacingProperty ); }
            set { this.SetValue( SpacingProperty, value ); }
        }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
                "ItemWidth",
                typeof( double ),
                typeof( TlSpacedUniformGrid ), new FrameworkPropertyMetadata(
                        defaultValue: 320.0,
                        flags: FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange ) );

        public double ItemWidth {
            get { return ( double ) this.GetValue( ItemWidthProperty ); }
            set { this.SetValue( ItemWidthProperty, value ); }
        }

        protected override Size MeasureOverride( Size availableSize ) {
            int elementCount = this.InternalChildren.Count;

            if ( elementCount == 0 ) {
                return new Size( 0, 0 );
            }

            this.newItemWidth = 0;
            this.newItemHeight = 0;
            this.columnCount = 0;
            this.rowCount = 0;

            UIElementCollection elements = this.InternalChildren;

            double availableWidth = availableSize.Width;
            double availableHeight = availableSize.Height;

            double spacing = this.Spacing;
            double itemWidth = this.ItemWidth;

            double widthSum = itemWidth;

            while ( widthSum < availableWidth ) {
                this.columnCount++;

                if ( widthSum == itemWidth ) {
                    widthSum += spacing;
                }

                widthSum += itemWidth;
            }

            this.columnCount = Math.Max( 1, this.columnCount );

            this.rowCount = Math.Max( 1, ( int )Math.Ceiling( elementCount / ( double ) this.columnCount ) );

            double maxElementHeight = 0;

            if ( elementCount < this.columnCount ) {
                this.newItemWidth = itemWidth;
            }
            else {
                this.newItemWidth = ( availableWidth - ( this.columnCount > 1 ? ( this.columnCount - 1 ) * spacing : 0 ) ) / this.columnCount;
            }

            foreach ( UIElement element in elements ) {
                element.Measure( new Size( this.newItemWidth, double.PositiveInfinity ) );
                maxElementHeight = Math.Max( maxElementHeight, element.DesiredSize.Height );
            }

            this.newItemHeight = this.newItemWidth / ( itemWidth / maxElementHeight );

            double newWidth = ( this.newItemWidth * this.columnCount ) + ( this.columnCount > 1 ? ( this.columnCount - 1 ) * spacing : 0 );
            double newHeight = ( this.newItemHeight * this.rowCount ) + ( this.rowCount > 1 ? ( this.rowCount - 1 ) * spacing : 0 );

            newWidth = double.IsPositiveInfinity( newWidth ) ? int.MaxValue : newWidth;
            newHeight = double.IsPositiveInfinity( newHeight ) ? int.MaxValue : newHeight;

            return new Size( newWidth, newHeight );
        }

        protected override Size ArrangeOverride( Size arrangeSize ) {
            int elementCount = this.InternalChildren.Count;

            if ( elementCount == 0 ) {
                return arrangeSize;
            }

            UIElementCollection elements = this.InternalChildren;

            double spacing = this.Spacing;

            double curX = 0;
            double curY = 0;

            for ( int i = 0; i < elementCount; i++ ) {
                UIElement element = elements[ i ];

                if ( i % this.columnCount == 0 ) {
                    curX = 0;

                    if ( i > this.columnCount - 1 ) {
                        curY += spacing + this.newItemHeight;
                    }
                }

                if ( i % this.columnCount > 0 ) {
                    curX += spacing;
                }

                Rect rect = new Rect( curX, curY, this.newItemWidth, this.newItemHeight );

                elements[ i ].Arrange( rect );

                curX += this.newItemWidth;
            }

            return arrangeSize;
        }
    }
}