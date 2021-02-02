using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game
{
    public class BlackScreen : Script
    {
        public float AppearTime = .5f;
        [HideInEditor]
        public Image Image { get; private set; }

        public static BlackScreen Instance { get; private set; }

        public override void OnAwake()
        {
            Instance = this;
            Image = Actor.As<UIControl>().Control as Image;
        }

        public static Task Appear() => Instance.SmoothAlphaAsync(1f);
        public static Task Faint() => Instance.SmoothAlphaAsync(0f);

        private async Task SmoothAlphaAsync(float alpha)
        {
            var t = 0f;
            while(t < AppearTime)
            {
                await Scripting.RunOnUpdate(() =>
                {
                    t += Time.DeltaTime;
                    Image.Color = new Color(0, 0, 0, Mathf.Lerp(Image.Color.A, alpha, t / AppearTime));
                });
            }
            Image.Color = new Color(0, 0, 0, alpha);
        }
    }
}
