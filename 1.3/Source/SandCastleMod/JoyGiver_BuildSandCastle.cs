using System.Collections.Generic;
using Verse;
using RimWorld;
using Verse.AI;

namespace SandCastleMod
{
    public class JoyGiver_BuildSandCastle : JoyGiver
    {
        private const float MinDistBetweenSandCastles = 12f;

        public override Job TryGiveJob(Pawn pawn)
        {
            if(pawn.Map.mapTemperature.OutdoorTemp < 0f)
            {
                return null;
            }
            if(pawn.WorkTypeIsDisabled(WorkTypeDefOf.Construction))
            {
                return null;
            }
            if (!JoyUtility.EnjoyableOutsideNow(pawn, null))
            {
                return null;
            }
            IntVec3 c = JoyGiver_BuildSandCastle.TryFindSandCastleBuildCell(pawn);
            if (!c.IsValid)
            {
                return null;
            }
            return new Job(this.def.jobDef, c);
        }

        private static IntVec3 TryFindSandCastleBuildCell(Pawn pawn)
        {
            Region rootReg;
            if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(RegionType.Set_Passable), TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), (Region r) => r.Room.PsychologicallyOutdoors, 100, out rootReg, RegionType.Set_Passable))
            {
                return IntVec3.Invalid;
            }
            IntVec3 result = IntVec3.Invalid;
            RegionTraverser.BreadthFirstTraverse(rootReg, (Region from, Region r) => r.Room == rootReg.Room, delegate (Region r)
            {
                for (int i = 0; i < 5; i++)
                {
                    IntVec3 randomCell = r.RandomCell;
                    if (JoyGiver_BuildSandCastle.IsGoodSandCastleCell(randomCell, pawn))
                    {
                        result = randomCell;
                        return true;
                    }
                }
                return false;
            }, 30, RegionType.Set_Passable);
            return result;
        }

        private static bool IsGoodSandCastleCell(IntVec3 c, Pawn pawn)
        {
            if (pawn.Map.terrainGrid.TerrainAt(c) != TerrainDefOf.Sand)
            {
                return false;
            }
            if (c.IsForbidden(pawn))
            {
                return false;
            }
            if (c.GetEdifice(pawn.Map) != null)
            {
                return false;
            }
            for (int i = 0; i < 9; i++)
            {
                IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
                if (!c2.InBounds(pawn.Map))
                {
                    return false;
                }
                if (!c2.Standable(pawn.Map))
                {
                    return false;
                }
                if (pawn.Map.reservationManager.IsReservedAndRespected(c2, pawn))
                {
                    return false;
                }
            }
            List<Thing> list = pawn.Map.listerThings.ThingsOfDef(SandCastleDefOf.SCM_SandCastle);
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j].Position.InHorDistOf(c, MinDistBetweenSandCastles))
                {
                    return false;
                }
            }
            return true;
        }
    }
}