using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.AI;

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
                    //thing.SetColor(new Color(1, 0, 0));

                    TerrainDef sandType = GridsUtility.GetTerrain(base.TargetLocA, base.Map);
                    var texture = sandType.graphic?.MatSingle?.mainTexture;
                    Color castleColor = new Color(0.578f, 0.516f, 0.445f);
                    //Color castleColor = new Color(1f, 0, 0);
                    if (texture != null)
                    {
                        //var renderTex = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
                        //var readableTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                        //Graphics.Blit(texture, renderTex);
                        //var rawColor = readableTex.GetPixel(0, 0);


                        if (texture is Texture2D texture2D)
                        {
                            var renderTex = RenderTexture.GetTemporary(10, 10, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
                            var readableTex = new Texture2D(10, 10, TextureFormat.RGBA32, false);
                            Graphics.Blit(texture2D, renderTex);

                            readableTex.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
                            readableTex.Apply(false);

                            var rawColor = readableTex.GetPixel(0, 0);
                            Log.Message("Terrain: " + sandType.label);
                            Log.Message("color = " + rawColor.r + ", " + rawColor.g + ", " + rawColor.b);


                            castleColor = rawColor * sandType.graphic.color;
                        }
                    }
                    castleColor.a = 1f;


                    //castleColor = new Color(0.578f, 0.516f, 0.445f, 1f);


                    thing.SetColor(castleColor);

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
