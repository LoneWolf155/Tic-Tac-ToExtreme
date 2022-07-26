using System.Reflection;
using UnityEngine;
using TutorialProject.Framework.Events;

namespace TutorialProject.Framework.Utility {
    /// <summary>
    /// Provides various methods that perform a variety of tasks.
    /// </summary>
    public static class HelperUtility {
        /// <summary>
        /// Automatically registers game events within a container class for a listener. This uses reflection and is therefore slower than doing it manually,
        /// should only be used for development builds.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventContainer">The event container.</param>
        /// <param name="listener">The listener.</param>
        public static void AutoRegisterGameEvents<T>(T eventContainer, IGameEventListener listener) {
            if (!Debug.isDebugBuild) {
                Debug.LogWarning($"AutoRegisterGameEvents is being called in a non-debug build for '{listener.GetType().Name}', this should be replaced for Release builds");
            }

            // Go through the fields and seek out the game events
            foreach (FieldInfo tmpField in typeof(T).GetFields()) {
                if (tmpField.FieldType != typeof(GameEvent)) { continue; }

                var gameEvent = (tmpField.GetValue(eventContainer) as GameEvent);
                if (gameEvent != null) {
                    gameEvent.RegisterListener(listener);
                }
                else {
                    if (listener is Object) {
                        Debug.LogWarning("Game event is NULL in auto register collection, is this correct?", listener as Object);
                    }
                    else {
                        Debug.LogWarning("Game event is NULL in auto register collection, is this correct?");
                    }
                }
            }
        }

        /// <summary>
        /// Automatically unregisters game events within a container class for a listener. This uses reflection and is therefore slower than doing it manually,
        /// should only be used for development builds.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventContainer">The event container.</param>
        /// <param name="listener">The listener.</param>
        public static void AutoUnregisterGameEvents<T>(T eventContainer, IGameEventListener listener) {
            if (!Debug.isDebugBuild) {
                Debug.LogWarning($"AutoUnregisterGameEvents is being called in a non-debug build for '{listener.GetType().Name}', this should be replaced for Release builds");
            }

            // Go through the fields and seek out the game events
            foreach (FieldInfo tmpField in typeof(T).GetFields()) {
                if (tmpField.FieldType != typeof(GameEvent)) { continue; }

                var gameEvent = (tmpField.GetValue(eventContainer) as GameEvent);
                if (gameEvent != null) {
                    gameEvent.UnregisterListener(listener);
                }
                else {
                    if (listener is Object) {
                        Debug.LogWarning("Game event is NULL in auto unregister collection, is this correct?", listener as Object);
                    }
                    else {
                        Debug.LogWarning("Game event is NULL in auto unregister collection, is this correct?");
                    }
                }
            }
        }

        /// <summary>
        /// Validates an integer argument by properly parsing it and spitting it back out.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static bool ValidateIntegerArgument(object arg, out int result) {
            if (arg is int) {
                result = (int)arg;
                return true;
            }
            else if (arg is string && int.TryParse((string)arg, out result)) {
                return true;
            }
            else {
                result = 0;
                return false;
            }
        }

        /// <summary>
        /// Parses a bool argument and returns its value.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static bool ParseBoolArgument(object arg) {
            if (arg is bool) { return (bool)arg; }
            else if (arg is string) {
                string str = ((string)arg).ToLower();
                return (str == "true" || str == "yes" || str == "1");
            }
            else if (arg is int) {
                return (int)arg > 0;
            }

            Debug.LogWarning($"Failed to parse bool argument of '{arg}'");
            return false;
        }
    }
}
