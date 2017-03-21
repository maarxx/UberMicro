using RimWorld;
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

        private bool notFirstPass;

        private Dictionary<Pawn, string> lastPrioritizedJobs;
        private Dictionary<Pawn, bool> lastJobWasDrafted;

        // Not real, just placeholders.
        private const string NOTHING = "nothing.";
        private const string DOWNED = "downed.";
        private const string BURNING = "burning.";

        // Actually real job strings.
        private const string MOVING = "moving.";
        private const string STANDING = "standing.";
        private const string WATCHING = "watching for targets.";

        public MapComponent_UberMicro(Map map) : base(map)
        {
            this.enabled = false;
            this.notFirstPass = false;
            this.draftedWalking = false;
            this.colonistShotFinished = false;
            this.enemyShotBegun = false;
            this.lastPrioritizedJobs = new Dictionary<Pawn, string>();
            this.lastJobWasDrafted = new Dictionary<Pawn, bool>();
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

        private void processJobString(Pawn p, string curJob, bool notify)
        {
            Log.Message("Processing: " + p.NameStringShort);
            if (!lastPrioritizedJobs.ContainsKey(p))
            {
                Log.Message("Was empty, now NOTHING");
                lastPrioritizedJobs[p] = NOTHING;
            }
            else
            {
                string lastPrioritizedJob = lastPrioritizedJobs[p];
                Log.Message("LastJob: " + lastPrioritizedJob + " CurJob: " + curJob);
                if (lastPrioritizedJob != NOTHING && lastPrioritizedJob != curJob)
                {
                    if (notify)
                    {
                        if (curJob == DOWNED)
                        {
                            notifyAndPause(
                                "Downed!",
                                p.NameStringShort + " is downed, possibly interrupting their last prioritized job to \"" + lastPrioritizedJob + "\"",
                                p
                            );
                        }
                        else if (curJob == BURNING)
                        {
                            notifyAndPause(
                                "Burning!",
                                p.NameStringShort + " is burning, possibly interrupting their last prioritized job to \"" + lastPrioritizedJob + "\"",
                                p
                            );
                        }
                        else if (lastPrioritizedJob == MOVING && ( curJob == STANDING || curJob == WATCHING ) )
                        {
                            if (draftedWalking)
                            {
                                notifyAndPause(
                                    "Job Done",
                                    p.NameStringShort + " has stopped or completed their last job to \"" + lastPrioritizedJob + "\" and is now \"" + curJob + "\"",
                                    p
                                );
                            }
                        }
                        else if (lastPrioritizedJob == STANDING && curJob == WATCHING)
                        {
                            // Deliberately do nothing.
                        }
                        else if (lastPrioritizedJob == STANDING || lastPrioritizedJob == WATCHING)
                        {
                            // Deliberately do nothing.
                        }
                        else
                        {
                            notifyAndPause(
                                "Job Done",
                                p.NameStringShort + " has stopped or completed their last job to \"" + lastPrioritizedJob + "\" and is now \"" + curJob + "\"",
                                p
                            );
                        }
                    }
                }
                lastPrioritizedJobs[p] = curJob;
            }
        }

        private static void notifyAndPause(string title, string text, Pawn target)
        {
            Find.LetterStack.ReceiveLetter(
                    title,
                    text,
                    LetterType.Good,
                    new GlobalTargetInfo(target)
                );
            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
        }

        public override void MapComponentTick()
        {
            if (!enabled)
            {
                notFirstPass = false;
            }
            else
            {
                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                {
                    /*
                    if (!p.Drafted)
                    {
                        bool wasDrafted = false;
                        if (lastJobWasDrafted.TryGetValue(p, out wasDrafted) && wasDrafted == true)
                        {
                            lastPrioritizedJobs[p] = "nothing";
                            lastJobWasDrafted[p] = false;
                        }
                    }
                    */

                    //string curJob = getJobString(p.CurJob);

                    if (p.Downed)
                    {
                        processJobString(p, DOWNED, notFirstPass);
                    }
                    else if (FireUtility.IsBurning(p))
                    {
                        processJobString(p, BURNING, notFirstPass);
                    }
                    else if (p.Drafted)
                    {
                        string curJob = getJobString(p.CurJob);
                        if (curJob == MOVING)
                        {
                            processJobString(p, curJob, false);
                        }
                        else
                        {
                            processJobString(p, curJob, notFirstPass);
                        }

                        /*
                        lastJobWasDrafted[p] = true;
                        string curJob = getJobString(p.CurJob);

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
                                else if (lastPrioritizedJob.Equals("moving.") && curJob.Equals("standing."))
                                {
                                    if (draftedWalking)
                                    {
                                        notifyAndPause(
                                            "Job Done",
                                            p.NameStringShort + " has stopped or completed their last job to \"" + lastPrioritizedJob + "\" and is now \"" + curJob + "\"",
                                            p
                                        );
                                    }
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
                        */
                    }
                    else
                    {
                        string curJob = getJobString(p.CurJob);
                        if (p.CurJob.playerForced)
                        {
                            processJobString(p, curJob, false);
                        }
                        else
                        {
                            processJobString(p, curJob, notFirstPass);
                        }
                    }
                }
                notFirstPass = true;
            }
        }

    }
}
