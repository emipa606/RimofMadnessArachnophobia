﻿using Verse;
using Verse.AI;
using RimWorld;

namespace Arachnophobia
{
    public class JobGiver_GetFoodSpider : ThinkNode_JobGiver
    {
        private HungerCategory minCategory;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_GetFoodSpider JobGiver_GetFoodSpider = (JobGiver_GetFoodSpider)base.DeepCopy(resolve);
            JobGiver_GetFoodSpider.minCategory = this.minCategory;
            return JobGiver_GetFoodSpider;
        }

        public override float GetPriority(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if (food == null)
            {
                return 0f;
            }
            if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn))
            {
                return 0f;
            }
            if (food.CurCategory < this.minCategory)
            {
                return 0f;
            }
            if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
            {
                return 9.5f;
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Need_Food food = pawn.needs.food;
            if (food == null || food.CurCategory < this.minCategory)
            {
                return null;
            }
            if (!pawn.RaceProps.Animal)
            {
                return null;
            }
            var localCocoons = Utility.CocoonsFor(pawn.Map, pawn);
            if (localCocoons != null && localCocoons.Count > 0)
            {
                Building_Cocoon closestCocoon = null;
                var shortestDistance = 9999f;
                foreach (Building_Cocoon cocoon in localCocoons)
                {
                    //Log.Message("1");
                    if (cocoon?.isConsumableBy(pawn) == true &&
                        cocoon.CurrentDrinker == null)
                    {
                        //Log.Message("2");
                        if (closestCocoon == null)
                        {
                            closestCocoon = cocoon; 
                            continue;
                        }
                        var thisDistance = (float)(cocoon.Position - pawn.Position).LengthHorizontalSquared;
                        if (thisDistance < shortestDistance)
                        {
                            shortestDistance = thisDistance;
                            closestCocoon = cocoon;
                        }
                    }
                }
                if (closestCocoon != null)
                {
                    //Log.Message("3");
                    closestCocoon.CurrentDrinker = pawn as PawnWebSpinner;
                    var newJob = new Job(ROMADefOf.ROMA_ConsumeCocoon, closestCocoon);
                    //pawn.Reserve(closestCocoon, newJob);
                    return newJob;
                }
            }


            bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out Thing thing, out ThingDef def, true, true, false, true, false))
            {
                return null;
            }

            if (thing is Pawn pawn2)
            {
                var jobToDo = new Job(ROMADefOf.ROMA_SpinPrey, pawn2)
                {
                    count = 1
                };
                return jobToDo;
            }
            if (thing is Building_NutrientPasteDispenser building_NutrientPasteDispenser && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())
            {
                Building building = building_NutrientPasteDispenser.AdjacentReachableHopper(pawn);
                if (building != null)
                {
                    ISlotGroupParent hopperSgp = building as ISlotGroupParent;
                    Job job = WorkGiver_CookFillHopper.HopperFillFoodJob(pawn, hopperSgp);
                    if (job != null)
                    {
                        return job;
                    }
                }
                thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn, desperate, out ThingDef thingDef, FoodPreferability.MealLavish, false, !pawn.IsTeetotaler(), false, false, false, false, false);
                if (thing == null)
                {
                    return null;
                }
                def = thingDef;
            }
            float nutrition = FoodUtility.GetNutrition(thing, def);
            return new Job(JobDefOf.Ingest, thing)
            {
                count = FoodUtility.WillIngestStackCountOf(pawn, def, nutrition)
            };
        }
    }
}
