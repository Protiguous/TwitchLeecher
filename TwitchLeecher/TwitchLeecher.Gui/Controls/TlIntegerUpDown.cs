using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace TwitchLeecher.Gui.Controls {
    public class TlIntegerUpDown : IntegerUpDown {

        private int maxLength;

        public static readonly DependencyProperty PadZerosProperty = DependencyProperty.Register(
                "PadZeros",
                typeof( bool ),
                typeof( TlIntegerUpDown ), new FrameworkPropertyMetadata( defaultValue: false ) );

        public bool PadZeros {
            get { return ( bool ) this.GetValue( PadZerosProperty ); }
            set { this.SetValue( PadZerosProperty, value ); }
        }

        public static readonly DependencyProperty LoopProperty = DependencyProperty.Register(
                "Loop",
                typeof( bool ),
                typeof( TlIntegerUpDown ), new FrameworkPropertyMetadata( defaultValue: false ) );

        public bool Loop {
            get { return ( bool ) this.GetValue( LoopProperty ); }
            set { this.SetValue( LoopProperty, value ); }
        }

        protected void FocusAndSelectAll() {
            if ( !this.TextBox.IsFocused ) {
                this.TextBox.Focus();
            }

            this.TextBox.SelectAll();
        }

        protected override void OnInitialized( EventArgs e ) {
            this.maxLength = this.Maximum.HasValue ? this.Maximum.ToString().Length : int.MaxValue;

            base.OnInitialized( e );

            DataObject.AddPastingHandler( this, ( s, args ) => { args.CancelCommand(); } );
        }

        protected override void OnPreviewTextInput( TextCompositionEventArgs e ) {
            string input = e.Text;

            if ( string.IsNullOrWhiteSpace( input ) || input.Length < 1 || !Regex.Match( input, "[0-9]" ).Success ) {
                e.Handled = true;
            }
            else {
                char inputChar = input[ 0 ];

                StringBuilder sb = new StringBuilder( this.Text );

                int caretIndex = this.TextBox.CaretIndex;
                int textLength = sb.Length;

                if ( this.TextBox.SelectionLength == 0 && textLength == this.maxLength ) {
                    e.Handled = true;

                    if ( caretIndex < textLength ) {
                        sb[ caretIndex ] = inputChar;
                        this.Text = sb.ToString();
                        this.TextBox.CaretIndex = caretIndex + 1;
                    }
                }
            }

            base.OnPreviewTextInput( e );
        }

        protected override void OnLostKeyboardFocus( KeyboardFocusChangedEventArgs e ) {
            this.SetValue( this.Value.GetValueOrDefault() );
            base.OnLostKeyboardFocus( e );
        }

        protected override void OnPreviewMouseWheel( MouseWheelEventArgs e ) {
            if ( this.TextBox.IsFocused ) {
                this.SetValue( this.Value.GetValueOrDefault() + ( e.Delta > 0 ? 1 : -1 ) );
                e.Handled = true;
            }

            base.OnMouseWheel( e );
        }

        protected override void SetValidSpinDirection() {
            if ( this.Loop ) {
                if ( this.Spinner != null ) {
                    ValidSpinDirections validSpinDirections = ValidSpinDirections.None;

                    if ( this.Increment.HasValue && !this.IsReadOnly ) {
                        validSpinDirections |= ValidSpinDirections.Increase;
                        validSpinDirections |= ValidSpinDirections.Decrease;
                    }

                    this.Spinner.ValidSpinDirection = validSpinDirections;
                }
            }
            else {
                base.SetValidSpinDirection();
            }
        }

        protected override void OnIncrement() {
            if ( this.Loop && this.Increment.HasValue ) {
                this.SetValue( this.Value.GetValueOrDefault() + this.Increment.Value );
            }
            else {
                base.OnIncrement();
            }

            this.FocusAndSelectAll();
        }

        protected override void OnDecrement() {
            if ( this.Loop && this.Increment.HasValue ) {
                this.SetValue( this.Value.GetValueOrDefault() - this.Increment.Value );
            }
            else {
                base.OnDecrement();
            }

            this.FocusAndSelectAll();
        }

        protected void SetValue( int value ) {
            if ( this.Loop ) {
                value = this.Maximum.HasValue && this.Minimum.HasValue && value > this.Maximum.Value ? this.Minimum.Value : value;
                value = this.Minimum.HasValue && this.Maximum.HasValue && value < this.Minimum.Value ? this.Maximum.Value : value;
            }

            value = this.Maximum.HasValue && value > this.Maximum.Value ? this.Maximum.Value : value;
            value = this.Minimum.HasValue && value < this.Minimum.Value ? this.Minimum.Value : value;

            this.Value = value;

            this.FocusAndSelectAll();
        }

        protected override int? ConvertTextToValue( string text ) {
            if ( !string.IsNullOrWhiteSpace( text ) ) {
                return int.TryParse( text.TrimStart( '0' ), out int result ) ? result : 0; ;
            }
            else {
                return 0;
            }
        }

        protected override string ConvertValueToText() {
            int value = this.Value.GetValueOrDefault();

            if ( this.Maximum.HasValue && this.PadZeros ) {
                return Math.Abs( value ).ToString().PadLeft( this.maxLength, '0' );
            }
            else {
                return value.ToString();
            }
        }
    }
}