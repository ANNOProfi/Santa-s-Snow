using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

/*
 * 
 */

namespace SantaSnow
{

    [StaticConstructorOnStartup]
    public static class SantaSnow
    {
        static SantaSnow()
        {
            Log.Message("Orbital Sweatshop ready.");
        }
    }

    [DefOf]
    public class GameConitionDef_SantaSnow
    {
        public static GameConditionDef SantaSnow;
    }

    public class GameCondition_SantaSnow : GameCondition
    {
        public void GameCondition()
        {
            RuntimeHelpers.RunClassConstructor(typeof(GameCondition).TypeHandle);
        }

        public GameCondition_SantaSnow()
        {
            ColorInt colorInt = new ColorInt(9, 191, 255);
            Color toColor = colorInt.ToColor;
            ColorInt colorInt2 = new ColorInt(248, 248, 255);
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

        public static void DoDrugEffectCheck()
        {

        }

        public override void GameConditionTick()
        {
            if(Find.TickManager.TicksGame % 3451 == 0)
            {
                List<Pawn> allPawnsSpawned = base.SingleMap.mapPawns.AllPawnsSpawned;
                for(int i = 0; i < allPawnsSpawned.Count; i++)
                {
                    Pawn pawn = allPawnsSpawned[i];
                    if(!pawn.Position.Roofed(base.SingleMap) && pawn.def.race.IsFlesh && !pawn.health.hediffSet.HasHediff(HediffDefOf_DrugHighs.ChemicalBuildup, false))
                    {
                        float num = 0.01f;
                        if (num != 0f)
                        {
                            float num2 = Mathf.Lerp(0.05f, 0.15f, Rand.ValueSeeded(pawn.thingIDNumber ^ 74374237));
                            num *= num2;
                            HealthUtility.AdjustSeverity(pawn, HediffDefOf_DrugHighs.ChemicalBuildup, num2);
                            if (HediffDefOf_DrugHighs.ChemicalBuildup ==null)
                            {
                                HealthUtility.AdjustSeverity(pawn, HediffDefOf_DrugHighs.ChemicalBuildup, num);
                                return;
                            }
                        }
                    }

                    Hediff item = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def.label == "chemical buildup");
                    if (pawn.health.hediffSet.HasHediff(HediffDefOf_DrugHighs.YayoHigh, false) && pawn.health.hediffSet.HasHediff(HediffDefOf_DrugHighs.ChemicalBuildup, false))
                    {
                        pawn.health.hediffSet.hediffs.Remove(item);
                    }
                }
            }

            for (int j = 0; j < this.overlays.Count; j++)
            {
                this.overlays[j].TickOverlay(base.SingleMap);
            }
        }

        public override void DoCellSteadyEffects(IntVec3 c, Map map)
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
        }

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
            return GameConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, 5000f, 0.5f);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget?(new SkyTarget(0.85f, this.SantaSnowColors, 1f, 1f));
        }

        public override bool AllowEnjoyableOutsideNow(Map map)
        {
            return true;
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

    public class HediffDefOf_DrugHighs
    {
        public static HediffDef YayoHigh;

        public static HediffDef ChemicalBuildup;
    }

    public class IncidentDefOf_SantaSnow
    {
        public static IncidentDef SantaSnow;
    }

    public abstract class WeatherOutcomeDoer
    {
        public void DoWeathernOutcome(Pawn pawn, GameCondition gameCondition)
        {
            if (Rand.Value < this.chance)
            {
                this.DoWeatherOutcomeSpecial(pawn, gameCondition);
            }
        }

        protected abstract void DoWeatherOutcomeSpecial(Pawn pawn, GameCondition gameCondition);

        public float chance = 1f;

        public bool doToGeneratedPawnIfAddicted;
    }

    public class WeatherOutComeDoer_GiveHediff : WeatherOutcomeDoer
    {
        protected override void DoWeatherOutcomeSpecial(Pawn pawn, GameCondition gameCondition)
        {
            Hediff hediff = HediffMaker.MakeHediff(this.hediffDef, pawn, null);
            float num;
            if (this.severity > 0f)
            {
                num = this.severity;
            }
            else
            {
                num = this.hediffDef.initialSeverity;
            }
            if (this.divideByBodySize)
            {
                num /= pawn.BodySize;
            }
            AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, this.toleranceChemical, ref num);
            hediff.Severity = num;
            pawn.health.AddHediff(hediff, null, null, null);
        }

        public HediffDef hediffDef;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        private bool divideByBodySize;
    }

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
