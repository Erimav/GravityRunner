using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    /// <summary>
    /// Loads checkpoint when player leaves area
    /// </summary>
    public class GameplayArea : Script
    {
        public Collider Trigger;
        /// <summary>
        /// Areas player is located in
        /// </summary>
        public static HashSet<GameplayArea> CurrentAreas { get; private set; }

        public override void OnStart()
        {
            Trigger = Trigger ?? Actor.As<Collider>();

            Trigger.TriggerEnter += actor =>
            {
                if (actor.LayerName == "Player")
                {
                    CurrentAreas.Add(this);
                    Debug.Log("Edge set");
                }
            };

            Trigger.TriggerExit += actor =>
            {
                if (actor.LayerName == "Player")
                {
                    CurrentAreas.Remove(this);
                    if (CurrentAreas.Count == 0)
                    {
                        Checkpoint.Current.Load();
                        Debug.Log("Edge triggered");
                    }
                }
            };
        }
    }
}
