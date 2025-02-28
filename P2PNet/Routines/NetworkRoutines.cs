using P2PNet.Routines.Implementations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.Routines
{

    /// <summary>
    /// Handles network routines for peer-to-peer networking.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value which must implement IRoutine.</typeparam>
    public static class NetworkRoutines<TKey, TValue> where TValue : class, IRoutine
    {
        private static Dictionary<TKey, TValue> _routines { get; set; } = new Dictionary<TKey, TValue>();

        /// <summary>
        /// Initializes the routines dictionary and adds default routines.
        /// </summary>
        public static void InitializeRoutines()
        {
            _routines = new Dictionary<TKey, TValue>();

            // add default routines
            AddDefaultRoutine("RotateBroadcastPort", new RotateBroadcastPort { RoutineName = "RotateBroadcastPort", RoutineInterval = 30000 });
            AddDefaultRoutine("DiscernPeerChannels", new DiscernPeerChannels { RoutineName = "DiscernPeerChannels", RoutineInterval = 60000 });
        }

        /// <summary>
        /// Adds a default routine to the routines dictionary.
        /// </summary>
        /// <param name="key">The key of the routine to add.</param>
        /// <param name="routine">The routine to add.</param>
        /// <exception cref="InvalidCastException">Thrown when the key cannot be cast to the specified type.</exception>
        private static void AddDefaultRoutine(string key, IRoutine routine)
        {
            if (key is TKey typedKey)
            {
                _routines[typedKey] = (TValue)routine;
            }
            else
            {
                throw new InvalidCastException($"Cannot cast key '{key}' to type '{typeof(TKey)}'.");
            }
        }

        /// <summary>
        /// Gets the routine associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the routine to get or set.</param>
        /// <returns>The routine associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not found in the cache.</exception>
        public static TValue GetRoutine(TKey key)
        {
            if (_routines.TryGetValue(key, out TValue foundValue))
            {
                return foundValue;
            }
            throw new KeyNotFoundException($"Key '{key}' not found in the cache.");
        }

        /// <summary>
        /// Sets the routine associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the routine to set.</param>
        /// <param name="value">The routine to set.</param>
        public static void SetRoutine(TKey key, TValue value)
        {
                _routines[key] = value;
        }

        /// <summary>
        /// Adds a routine to the routines dictionary.
        /// </summary>
        /// <param name="item">The key-value pair of the routine to add.</param>
        public static void AddRoutine(TValue item)
        {
            if (item.RoutineName is TKey typedKey)
            {
                _routines[typedKey] = item;
            }
            else
            {
                throw new InvalidCastException($"Cannot cast routine name '{item.RoutineName}' to type '{typeof(TKey)}'.");
            }
        }

        /// <summary>
        /// Tries to start the routine associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the routine to start.</param>
        public static void TryStartRoutine(TKey key)
        {
            if (_routines.TryGetValue(key, out TValue foundValue))
            {
                foundValue.StartRoutine();
            }
        }

        /// <summary>
        /// Tries to stop the routine associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the routine to stop.</param>
        public static void TryStopRoutine(TKey key)
        {
            if (_routines.TryGetValue(key, out TValue foundValue))
            {
                foundValue.StopRoutine();
            }
        }

        /// <summary>
        /// Tries to set the interval of the routine associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the routine to set the interval for.</param>
        /// <param name="interval">The interval to set.</param>
        public static void TrySetRoutineInterval(TKey key, int interval)
        {
            if (_routines.TryGetValue(key, out TValue foundValue))
            {
                foundValue.SetRoutineInterval(interval);
            }
        }

        /// <summary>
        /// Gets the count of routines.
        /// </summary>
        public static int Count
        {
            get
            {
                return _routines.Count;
            }
        }

        public static IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _routines.ToList().GetEnumerator();
        }
    }
}
