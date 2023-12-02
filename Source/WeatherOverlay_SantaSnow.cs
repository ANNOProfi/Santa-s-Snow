using Verse;
using RimWorld;
using UnityEngine;

namespace SantaSnow
{
    [StaticConstructorOnStartup]
    public class WeatherOverlay_SantaSnow : SkyOverlay
    {
        public WeatherOverlay_SantaSnow()
        {
            this.worldOverlayMat = WeatherOverlay_SantaSnow.SmokeleafSnowOverlayWorld;
            this.worldOverlayPanSpeed1 = 0.15f;
            this.worldPanDir1 = new Vector2(-0.25f, -1f);
            this.worldPanDir1.Normalize();
            this.worldOverlayPanSpeed2 = 0.022f;
            this.worldPanDir2 = new Vector2(0.24f, -1f);
            this.worldPanDir2.Normalize();
        }

        private static readonly Material SmokeleafSnowOverlayWorld = MatLoader.LoadMat("Weather/SnowOverlayWorld", -1);
    }
}