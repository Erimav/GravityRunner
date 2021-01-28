using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class Edge : Script
    {
        public Collider Trigger;
        public static Edge Current { get; set; }

        public override void OnStart()
        {
            Trigger = Trigger ?? Actor.As<Collider>();

            Trigger.TriggerEnter += actor =>
            {
                if (actor.LayerName == "Player")
                {
                    Current = this;
                    Debug.Log("Edge set");
                }
            };

            Trigger.TriggerExit += actor =>
            {
                if (this == Current && actor.LayerName == "Player")
                {
                    Checkpoint.Current.Load();
                    Debug.Log("Edge triggered");
                }
            };
        }
    }
}
