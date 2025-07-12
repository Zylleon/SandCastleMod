using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Diagnostics;


namespace SandCastleMod
{
    public class JobDriver_BuildSandCastle : JobDriver
    {
        private float workLeft = -1000f;

        protected const int BaseWorkAmount = 2300;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = ToilMaker.MakeToil("MakeNewToils");
            doWork.initAction = delegate
            {
                workLeft = 2300f;
            };
            doWork.tickIntervalAction = delegate (int delta)
            {
                workLeft -= doWork.actor.GetStatValue(StatDefOf.ConstructionSpeed) * 2f * (float)delta;
                if (workLeft <= 0f)
                {
                    Thing thing = ThingMaker.MakeThing(SandCastleDefOf.SCM_SandCastle, null);
                    if (pawn.Faction.IsPlayer)
                    {
                        thing.SetFaction(Faction.OfPlayer, null);
                    }
                    GenSpawn.Spawn(thing, base.TargetLocA, base.Map);
                    ReadyForNextToil();
                }
                else
                {
                    JoyUtility.JoyTickCheckEnd(pawn, delta);
                }
            };
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.FailOn(() => !JoyUtility.EnjoyableOutsideNow(pawn));
            doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            doWork.activeSkill = () => SkillDefOf.Construction;
            yield return doWork;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
        }
    }
}
