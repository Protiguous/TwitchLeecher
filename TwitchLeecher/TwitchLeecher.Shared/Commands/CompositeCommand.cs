namespace TwitchLeecher.Shared.Commands {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;

    public class CompositeCommand : ICommand {
        private readonly EventHandler onRegisteredCommandCanExecuteChangedHandler;
        private readonly List<ICommand> registeredCommands = new List<ICommand>();

        public IList<ICommand> RegisteredCommands {
            get {
                IList<ICommand> commandList;
                lock ( this.registeredCommands ) {
                    commandList = this.registeredCommands.ToList();
                }

                return commandList;
            }
        }

        public CompositeCommand() {
            this.onRegisteredCommandCanExecuteChangedHandler = new EventHandler( this.OnRegisteredCommandCanExecuteChanged );
        }

        public virtual event EventHandler CanExecuteChanged;

        private void Command_IsActiveChanged( Object sender, EventArgs e ) {
            this.OnCanExecuteChanged();
        }

        private void OnRegisteredCommandCanExecuteChanged( Object sender, EventArgs e ) {
            this.OnCanExecuteChanged();
        }

        protected virtual void OnCanExecuteChanged() {
            CanExecuteChanged?.Invoke( this, EventArgs.Empty );
        }

        protected virtual Boolean ShouldExecute( ICommand command ) {
            return true;
        }

        public virtual Boolean CanExecute( Object parameter ) {
            var hasEnabledCommandsThatShouldBeExecuted = false;

            ICommand[] commandList;
            lock ( this.registeredCommands ) {
                commandList = this.registeredCommands.ToArray();
            }
            foreach ( ICommand command in commandList ) {
                if ( this.ShouldExecute( command ) ) {
                    if ( !command.CanExecute( parameter ) ) {
                        return false;
                    }

                    hasEnabledCommandsThatShouldBeExecuted = true;
                }
            }

            return hasEnabledCommandsThatShouldBeExecuted;
        }

        public virtual void Execute( Object parameter ) {
            Queue<ICommand> commands;
            lock ( this.registeredCommands ) {
                commands = new Queue<ICommand>( this.registeredCommands.Where( this.ShouldExecute ).ToList() );
            }

            while ( commands.Count > 0 ) {
                ICommand command = commands.Dequeue();
                command.Execute( parameter );
            }
        }

        public virtual void RegisterCommand( ICommand command ) {
            if ( command == null )
                throw new ArgumentNullException( nameof( command ) );
            if ( command == this ) {
                throw new ArgumentException( "Cannot register a CompositeCommand in itself" );
            }

            lock ( this.registeredCommands ) {
                if ( this.registeredCommands.Contains( command ) ) {
                    throw new InvalidOperationException( "Cannot register the same command twice in the same CompositeCommand" );
                }
                this.registeredCommands.Add( command );
            }

            command.CanExecuteChanged += this.onRegisteredCommandCanExecuteChangedHandler;
            this.OnCanExecuteChanged();
        }

        public virtual void UnregisterCommand( ICommand command ) {
            if ( command == null )
                throw new ArgumentNullException( nameof( command ) );
            Boolean removed;
            lock ( this.registeredCommands ) {
                removed = this.registeredCommands.Remove( command );
            }

            if ( removed ) {
                command.CanExecuteChanged -= this.onRegisteredCommandCanExecuteChangedHandler;
                this.OnCanExecuteChanged();
            }
        }
    }
}