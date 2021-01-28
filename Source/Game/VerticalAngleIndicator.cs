using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game
{
    public class VerticalAngleIndicator : Script
    {
        private PlayerScript player;
        private UIControl bar;
        private Control control;
        public override void OnStart()
        {
            player = Scene.FindScript(typeof(PlayerScript)) as PlayerScript;
            bar = Actor.Parent.As<UIControl>();
            control = Actor.As<UIControl>().Control;
        }

        public override void OnUpdate()
        {
            var top = 0f;
            var bottom = bar.Control.Bottom - bar.Control.Top;
            var angle = player.TargetRotationAngles.X + 90f;
            control.Y = Mathf.Lerp(bottom, top, 1 - (angle / 180f));
        }
    }
}
