
namespace TutorialProject.Framework.Events {
    /// <summary>
    /// Implements the ability for a class to listen to a <see cref="GameEvent"/>.
    /// </summary>
    public interface IGameEventListener {
        /// <summary>
        /// Handles the raising of the linked event.
        /// </summary>
        /// <param name="gameEvent">The game event being raised.</param>
        /// <param name="sender">The sender that is triggering the event (can be NULL).</param>
        /// <param name="args">Any arguments being passed along with the event.</param>
        void OnEventRaised(GameEvent gameEvent, object sender, params object[] args);
    }
}
