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

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = new Toil();
            doWork.initAction = delegate
            {
                this.workLeft = 2300f;
            };
            doWork.tickAction = delegate
            {
                this.workLeft -= 2f * doWork.actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
                if (this.workLeft <= 0f)
				{
                    Thing thing = ThingMaker.MakeThing(SandCastleDefOf.SCM_SandCastle, null);
                    if(pawn.Faction.IsPlayer)
                    {
                        thing.SetFaction(Faction.OfPlayer, null);
                    }
                    GenSpawn.Spawn(thing, this.TargetLocA, this.Map, WipeMode.Vanish);
                    this.ReadyForNextToil();
                    return;
                }
                JoyUtility.JoyTickCheckEnd(this.pawn);
            };
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.pawn, null));
            doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return doWork;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0f, false);
        }
    }
}
