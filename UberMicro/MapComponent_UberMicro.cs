using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace UberMicro
{
    class MapComponent_UberMicro : MapComponent
    {
        public bool enabled;
        public bool draftedWalking;
        public bool colonistShotFinished;
        public bool enemyShotBegun;

        private Dictionary<Pawn, string> lastPrioritizedJobs;
        private Dictionary<Pawn, bool> lastJobWasDrafted;

        public MapComponent_UberMicro(Map map) : base(map)
        {
            this.enabled = false;
            this.draftedWalking = false;
            this.colonistShotFinished = false;
            this.enemyShotBegun = false;
            this.lastPrioritizedJobs = new Dictionary<Pawn, string>();
            this.lastJobWasDrafted = new Dictionary<Pawn, bool>();
        }

        public override void MapComponentTick()
        {
            if (enabled)
            {
                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                {
                    if (!p.Drafted)
                    {
                        bool wasDrafted = false;
                        if (lastJobWasDrafted.TryGetValue(p, out wasDrafted) && wasDrafted == true)
                        {
                            lastPrioritizedJobs[p] = "nothing";
                            lastJobWasDrafted[p] = false;
                        }
                    }

                    string curJob = getJobString(p.CurJob);
                    string lastPrioritizedJob = "nothing";
                    if (!lastPrioritizedJobs.ContainsKey(p))
                    {
                        lastPrioritizedJobs[p] = "nothing";
                    }

                    lastPrioritizedJob = lastPrioritizedJobs[p];

                    if (p.Downed)
                    {
                        if (!lastPrioritizedJob.Equals("downed."))
                        {
                            notifyAndPause(
                                "Downed!",
                                p.NameStringShort + " has been downed!",
                                p
                            );
                            lastPrioritizedJobs[p] = "downed.";
                        }
                    }
                    else if (p.Drafted)
                    {
                        lastJobWasDrafted[p] = true;
                        if (curJob.Equals("moving."))// || curJob.Equals("standing.") || curJob.Equals("watching for targets."))
                        {
                            lastPrioritizedJobs[p] = "moving.";
                        }
                        else
                        {
                            if (lastPrioritizedJob != "nothing")
                            {
                                if (lastPrioritizedJob.Equals("standing.") && curJob.Equals("watching for targets."))
                                {
                                    lastPrioritizedJobs[p] = curJob;
                                }
                                else if (p.CurJob.playerForced)
                                {
                                    lastPrioritizedJobs[p] = curJob;
                                }
                                else if (!curJob.Equals(lastPrioritizedJob))
                                {
                                    notifyAndPause(
                                        "Job Done",
                                        p.NameStringShort + " has stopped or completed their last job to \"" + lastPrioritizedJob + "\" and is now \"" + curJob + "\"",
                                        p
                                    );
                                    lastPrioritizedJobs[p] = curJob;
                                }
                            }
                            else
                            {
                                lastPrioritizedJobs[p] = curJob;
                            }
                        }
                    }
                    else
                    {
                        if (p.CurJob.playerForced)
                        {
                            lastPrioritizedJobs[p] = curJob;
                        }
                        else
                        {
                            if (lastPrioritizedJob != "nothing")
                            {
                                notifyAndPause(
                                    "Job Done",
                                    p.NameStringShort + " has stopped or completed their last job to \"" + lastPrioritizedJob + "\" and is now \"" + curJob + "\"",
                                    p
                                );
                                lastPrioritizedJobs[p] = "nothing";
                            }
                        }
                    }
                }
            }
        }

        public static string getJobString(Job j)
        {
            if (j.targetA.Thing == null)
            {
                return j.def.reportString;
            }
            else
            {
                if (j.targetB.Thing == null)
                {
                    return j.def.reportString.Replace("TargetA", j.targetA.Thing.LabelNoCount);
                }
                else
                {
                    if (j.targetC.Thing == null)
                    {
                        return j.def.reportString.Replace("TargetA", j.targetA.Thing.LabelNoCount).Replace("TargetB", j.targetB.Thing.LabelNoCount);
                    }
                    else
                    {
                        return j.def.reportString.Replace("TargetA", j.targetA.Thing.LabelNoCount).Replace("TargetB", j.targetB.Thing.LabelNoCount).Replace("TargetC", j.targetC.Thing.LabelNoCount);
                    }
                }
            }
        }

        public static void notifyAndPause(string title, string text, Pawn target)
        {
            Find.LetterStack.ReceiveLetter(
                    title,
                    text,
                    LetterType.Good,
                    new GlobalTargetInfo(target)
                );
            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
        }

    }
}
