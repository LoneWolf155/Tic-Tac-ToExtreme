using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using TutorialProject.Framework.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
namespace TutorialProject.Framework.Utility.Editor {
    /// <summary>
    /// Utility for working with runtime assets in the project.
    /// </summary>
    public static class EditorUtility {
        #region Declarations
        private static GameEvent[] _gameEventCache;
        #endregion

        #region GameEvent Methods
        /// <summary>
        /// Automatically fills events based on the name of the events in the project.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        public static void AutoFillEvents<T>(ref T target) where T : class, new() {
            CollectGameEvents();

            // If the object is null then create a new one
            if (target == null) {
                target = new T();
            }

            // Go through the fields and fill them with a game event that matches by name
            foreach (FieldInfo tmpField in typeof(T).GetFields()) {
                if (tmpField.FieldType != typeof(GameEvent)) { continue; }

                for (int x = 0; x < _gameEventCache.Length; x++) {
                    var gameEvent = _gameEventCache[x];
                    if (string.Compare(gameEvent.name, tmpField.Name, true) == 0 || string.Compare(gameEvent.name, "on" + tmpField.Name, true) == 0) {
                        tmpField.SetValue(target, gameEvent);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of the game event by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static GameEvent GetEventByName(string name) {
            CollectGameEvents();

            for (int x = 0; x < _gameEventCache.Length; x++) {
                var gameEvent = _gameEventCache[x];
                if (string.Compare(gameEvent.name, name, true) == 0 || string.Compare(gameEvent.name, "on" + name, true) == 0) {
                    return gameEvent;
                }
            }
            return null;
        }

        /// <summary>
        /// Collects the game events in the project.
        /// </summary>
        private static void CollectGameEvents() {
            var guids = AssetDatabase.FindAssets("t:" + typeof(GameEvent).Name);
            if (_gameEventCache == null || guids.Length != _gameEventCache.Length) {
                List<GameEvent> events = new List<GameEvent>();
                for (int x = 0; x < guids.Length; x++) {
                    events.Add(AssetDatabase.LoadAssetAtPath<GameEvent>(AssetDatabase.GUIDToAssetPath(guids[x])));
                }
                _gameEventCache = events.ToArray();
            }
        }
        #endregion

        #region Asset Helper Methods
        /// <summary>
        /// Gets the first found assets of the specified type from the project.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetScriptableObjectAsset<T>() where T : ScriptableObject {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            if (guids.Length > 0) {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            return null;
        }

        /// <summary>
        /// Gets the first found assets of the specified type and name from the project.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        public static T GetScriptableObjectAsset<T>(string assetName) where T : ScriptableObject {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            if (guids.Length > 0) {
                if (guids.Length > 0) {
                    for (int x = 0; x < guids.Length; x++) {
                        var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[x]));
                        if (string.Compare(asset.name, assetName, true) == 0) {
                            return asset;
                        }
                    }
                }
            }

            Debug.LogWarning($"Failed to find runtime asset named '{assetName}', did its name change?");
            return null;
        }

        /// <summary>
        /// Gets assets from the project.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetScriptableObjectAssets<T>() where T : ScriptableObject {
            List<T> assets = new List<T>();
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            if (guids.Length > 0) {
                for (int x = 0; x < guids.Length; x++) {
                    assets.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[x])));
                }
            }
            return assets.ToArray();
        }

        /// <summary>
        /// Gets the assets within the project of the specified type.
        /// </summary>
        /// <param name="assetType">Type of the asset.</param>
        /// <returns></returns>
        public static ScriptableObject[] GetScriptableObjectAssets(System.Type assetType) {
            List<ScriptableObject> assets = new List<ScriptableObject>();
            if (!assetType.IsSubclassOf(typeof(ScriptableObject))) {
                return assets.ToArray();
            }

            var guids = AssetDatabase.FindAssets("t:" + assetType.Name);
            if (guids.Length > 0) {
                for (int x = 0; x < guids.Length; x++) {
                    assets.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[x]), assetType) as ScriptableObject);
                }
            }
            return assets.ToArray();
        }

        /// <summary>
        /// Gets the objects with interface.
        /// </summary>
        /// <typeparam name="T1">The type of object we are looking for (aka MonoBehavior or ScriptableObject).</typeparam>
        /// <typeparam name="T2">The interface to be implemented.</typeparam>
        /// <param name="ignorePrefabChildren">If true, the object is ignored if it is part of a prefab (prefabs themselves will still be returned).</param>
        /// <returns></returns>
        public static T1[] GetObjectsWithInterface<T1, T2>(bool ignorePrefabChildren = true) where T1 : Object {
            // Note: Strip down to type 2 then convert back to type 2
            T1[] assets = Resources.FindObjectsOfTypeAll(typeof(T1)).OrderBy(i => (i as T1).name).OfType<T2>().OfType<T1>().ToArray();
            if (ignorePrefabChildren) {
                assets = assets.Where(i => !PrefabUtility.IsPartOfPrefabAsset(i)).ToArray();
            }
            return assets;
        }

        /// <summary>
        /// Gets the main asset for the asset specified. If the asset is not sub asset, than NULL is returned.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public static Object GetMainAsset(Object asset) {
            if (!AssetDatabase.IsSubAsset(asset)) {
                return null;
            }
            return AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(asset));
        }

        /// <summary>
        /// Gets all of the sub assets of the specified asset that are of the type specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public static List<Object> GetSubObjectOfType<T>(Object asset) {
            List<Object> ofType = new List<Object>();
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));

            foreach (Object o in objs) {
                if (o is T){
                    ofType.Add(o);
                }
            }

            return ofType;
        }
        #endregion
    }
}
#endif
