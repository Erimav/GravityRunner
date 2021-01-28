using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    public class KillingObstacle : Script
    {
        public Collider Trigger;

        public override void OnStart()
        {
            Trigger = Trigger ?? Actor.GetChild<Collider>();

            Trigger.TriggerEnter += actor =>
            {
                if (actor.LayerName == "Player")
                    Checkpoint.Current.Load();
            };
        }
    }
}
