using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TutorialProject.Framework.Utility {
    /// <summary>
    /// A custom attribute that allows for tagging a field with a simple ID that allows
    /// for simple reference by way of serialization dynamically.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SerializeIDAttribute : System.Attribute {
        #region Declarations
        public int ID;
        public string Label;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializeIDAttribute"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public SerializeIDAttribute(int id, string label) {
            ID = id;
            Label = label;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Tries to get the field info of the serialize id attribute with the specified ID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="property">The field.</param>
        /// <returns></returns>
        public static bool TryGetPropertyByID<T>(int id, out PropertyInfo property) {
            property = null;

            foreach (PropertyInfo tmpProp in typeof(T).GetProperties()) {
                SerializeIDAttribute attribute = (SerializeIDAttribute)tmpProp.GetCustomAttribute(typeof(SerializeIDAttribute), false);
                if (attribute != null && attribute.ID == id) {
                    property = tmpProp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tries the get the identifier for a field based on its name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static bool TryGetIdByProperty<T>(string name, out int id) {
            var property = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null) {
                id = 0;
                return false;
            }

            SerializeIDAttribute attribute = (SerializeIDAttribute)property.GetCustomAttribute(typeof(SerializeIDAttribute), false);
            id = attribute.ID;
            return true;
        }

        /// <summary>
        /// Builds out a dictionary using labels as the key and their respective id as the key to be used
        /// in an editor field picker.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, int> GetSerializeIdPickerDictionary<T>() {
            Dictionary<string, int> tmpDict = new Dictionary<string, int>();

            tmpDict.Add("None", 0);
            foreach (PropertyInfo tmpProp in typeof(T).GetProperties()) {
                SerializeIDAttribute attribute = (SerializeIDAttribute)tmpProp.GetCustomAttribute(typeof(SerializeIDAttribute), false);
                if (attribute != null) {
                    tmpDict.Add(attribute.Label, attribute.ID);
                }
            }

            return tmpDict;
        }
        #endregion
    }
}