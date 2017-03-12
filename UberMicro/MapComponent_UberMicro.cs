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
        private Dictionary<Pawn, string> lastPrioritizedJobs;
        private Dictionary<Pawn, bool> lastJobWasDrafted;

        public MapComponent_UberMicro(Map map) : base(map)
        {
            this.enabled = true;
            this.lastPrioritizedJobs = new Dictionary<Pawn, string>();
            this.lastJobWasDrafted = new Dictionary<Pawn, bool>();
        }

        public override void MapComponentTick()
        {
            if (enabled)
            {
                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                {
                    if (p.Downed)
                    {
                        string lastPrioritizedJob;
                        if ( !( lastPrioritizedJobs.TryGetValue(p, out lastPrioritizedJob) && lastPrioritizedJob.Equals("downed.") ) )
                        {
                            Find.LetterStack.ReceiveLetter(
                                        "Downed!",
                                        p.NameStringShort + " has been downed!",
                                        LetterType.Good,
                                        new GlobalTargetInfo(p)
                                    );
                            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                            lastPrioritizedJobs[p] = "downed.";
                        }
                    }
                    else if (p.Drafted)
                    {
                        lastJobWasDrafted[p] = true;
                        //Log.Message(p.NameStringShort + "," + p.CurJob.ToString());
                        string curJob = getJobString(p.CurJob);
                        if (curJob.Equals("moving."))// || curJob.Equals("standing.") || curJob.Equals("watching for targets."))
                        {
                            lastPrioritizedJobs[p] = "moving.";
                        }
                        else
                        {
                            string lastPrioritizedJob = null;
                            if (lastPrioritizedJobs.TryGetValue(p, out lastPrioritizedJob) && lastPrioritizedJob != null)
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
                                    Find.LetterStack.ReceiveLetter(
                                            "Job Done",
                                            p.NameStringShort + " has stopped or completed their last job to \"" + lastPrioritizedJobs[p] + "\" and is now \"" + getJobString(p.CurJob) + "\"",
                                            LetterType.Good,
                                            new GlobalTargetInfo(p)
                                        );
                                    Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
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
                        bool wasDrafted = false;
                        if (lastJobWasDrafted.TryGetValue(p, out wasDrafted) && wasDrafted == true)
                        {
                            lastPrioritizedJobs[p] = null;
                        }
                        if (p.CurJob.playerForced)
                        {
                            lastPrioritizedJobs[p] = getJobString(p.CurJob);
                        }
                        else
                        {
                            string lastPrioritizedJob = null;
                            if (lastPrioritizedJobs.TryGetValue(p, out lastPrioritizedJob) && lastPrioritizedJob != null)
                            {
                                Find.LetterStack.ReceiveLetter(
                                        "Job Done",
                                        p.NameStringShort + " has stopped or completed their last job to \"" + lastPrioritizedJobs[p] + "\" and is now \"" + getJobString(p.CurJob) + "\"",
                                        LetterType.Good,
                                        new GlobalTargetInfo(p)
                                    );
                                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                                lastPrioritizedJobs[p] = null;
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
    }
}
