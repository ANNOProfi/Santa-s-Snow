using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using UnityEngine.XR;

/*
 * 
 */

namespace SantaSnow
{
    public class GameCondition_SantaSnow : GameCondition
    {
        public void GameCondition()
        {
            RuntimeHelpers.RunClassConstructor(typeof(GameCondition).TypeHandle);
        }

        public GameCondition_SantaSnow()
        {
            ColorInt colorInt = new ColorInt(202, 235, 235);
            Color toColor = colorInt.ToColor;
            ColorInt colorInt2 = new ColorInt(0, 109, 109);
            this.SantaSnowColors = new SkyColorSet(toColor, colorInt2.ToColor, new Color(0.6f, 0.8f, 0.5f), 0.85f);
            this.overlays = new List<SkyOverlay>
            {
                new WeatherOverlay_SantaSnow()
            };
        }

        public override void Init()
        {
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Critical);
        }

        public override void GameConditionTick()
        {
            List<Map> affectedMaps = base.AffectedMaps;
            if(Find.TickManager.TicksGame % 3451 == 0)
            {
                for(int i=0; i < affectedMaps.Count; i++)
                {
                    DoPawnGiveYayo(affectedMaps[i]);
                }
            }

            for (int j = 0; j < this.overlays.Count; j++)
            {
                for(int k=0; k< affectedMaps.Count; k++)
                {
                    this.overlays[j].TickOverlay(affectedMaps[k]);
                }
            }
        }

        private void DoPawnGiveYayo(Map map)
        {
            List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned.ToList<Pawn>();
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                DoPawnGiveYayo(allPawnsSpawned[i]);
            }
        }

        public static void DoPawnGiveYayo(Pawn p, bool protectedByRoof = true)
        {
            if (!(p.Spawned && protectedByRoof) || !p.Position.Roofed(p.Map))
            {
                float num = 0.023006668f;

                num *= Mathf.Max(1f - p.GetStatValue(StatDefOf.ToxicResistance), 0f);

                if (ModsConfig.BiotechActive)
                {
                    num *= Mathf.Max(1f - p.GetStatValue(StatDefOf.ToxicEnvironmentResistance), 0f);
                }

                num *= 5f;

                if (num != 0f)
                {
                    float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(p.thingIDNumber ^ 0x46EDC5D));
                    num *= num2;
                    HealthUtility.AdjustSeverity(p, SantaSnowDefOf.YayoHigh, num);
                }
            }
        }

        /*public override void DoCellSteadyEffects(IntVec3 c, Map map)
        {
            if (!c.Roofed(base.SingleMap))
            {
                List<Thing> thingList = c.GetThingList(base.SingleMap);
                for (int i=0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing is Plant)
                    {
                        if (Rand.Value < 0.0065f)
                        {
                            thing.Kill(null, null);
                        }
                    }
                    else if (thing.def.category == ThingCategory.Item)
                    {
                        CompRottable compRottable = thing.TryGetComp<CompRottable>();
                        if (compRottable != null && compRottable.Stage < RotStage.Dessicated)
                        {
                            compRottable.RotProgress += 3000f;
                        }
                    }
                }
            }
        }*/

        public override void GameConditionDraw(Map map)
        {
            Map singleMap = base.SingleMap;
            for (int i=0; i < this.overlays.Count; i++)
            {
                this.overlays[i].DrawOverlay(singleMap);
            }
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks, 0.5f);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget?(new SkyTarget(0.85f, this.SantaSnowColors, 1f, 1f));
        }

        public override bool AllowEnjoyableOutsideNow(Map map)
        {
            return false;
        }

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return this.overlays;
        }

        private const int LerpTicks = 5000;

        private const float MaxSkyLerpFactor = 0.5f;

        private const float SkyGlow = 0.85f;

        private const int CheckInterval = 3451;

        private const float ToxicPerDay = 0.5f;

        private const float PlantKillChance = 0f;

        private SkyColorSet SantaSnowColors;

        private List<SkyOverlay> overlays;
    }
}
