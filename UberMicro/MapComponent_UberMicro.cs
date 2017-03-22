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

        private bool firstPass;

        private Dictionary<Pawn, string> lastJobStrings;
        private Dictionary<Pawn, bool> lastJobSignificance;

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
            this.draftedWalking = false;
            this.colonistShotFinished = false;
            this.enemyShotBegun = false;

            this.firstPass = true;

            this.lastJobStrings = new Dictionary<Pawn, string>();
            this.lastJobSignificance = new Dictionary<Pawn, bool>();

        }

        private static string getJobString(Job j)
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
                firstPass = true;
            }
            else
            {

                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                {

                    if (!lastJobStrings.ContainsKey(p))
                    {
                        lastJobStrings.Add(p, NOTHING);
                    }

                    if (!lastJobSignificance.ContainsKey(p))
                    {
                        lastJobSignificance.Add(p, false);
                    }

                    string lastJobString = lastJobStrings[p];
                    bool lastJobSignificant = lastJobSignificance[p];

                    string curJobString = NOTHING;
                    bool curJobSignificant = false;
                    bool interruptSignificant = false;

                    if (p.Downed)
                    {
                        curJobString = DOWNED;
                        curJobSignificant = false;
                        interruptSignificant = true;
                        //processJobString(p, DOWNED, notFirstPass);
                    }
                    else if (FireUtility.IsBurning(p))
                    {
                        curJobString = BURNING;
                        curJobSignificant = true;
                        interruptSignificant = true;
                        //processJobString(p, BURNING, notFirstPass);
                    }
                    else
                    {
                        curJobString = getJobString(p.CurJob);
                        if (p.CurJob.playerForced)
                        {
                            curJobSignificant = true;
                        }
                    }

                    if (!firstPass)
                    {
                        if (lastJobString != curJobString)
                        {
                            if (interruptSignificant)
                            {
                                if (curJobString == DOWNED)
                                {
                                    notifyAndPause(
                                        "Downed!",
                                        p.NameStringShort + " is downed, their last job was \"" + lastJobString + "\"",
                                        p
                                    );
                                }
                                else if (curJobString == BURNING)
                                {
                                    notifyAndPause(
                                        "Burning!",
                                        p.NameStringShort + " is burning, their last job was \"" + lastJobString + "\"",
                                        p
                                    );
                                }
                                else
                                {
                                    notifyAndPause(
                                        "Interrupted!",
                                        p.NameStringShort + " was interrupted, their last job was \"" + lastJobString + "\"",
                                        p
                                    );
                                }
                            }
                            else if (lastJobSignificant)
                            {
                                notifyAndPause(
                                    "Job Done",
                                    p.NameStringShort + " has completed (or stopped?) their last job to \"" + lastJobString + "\" and is now \"" + curJobString + "\"",
                                    p
                                );
                            }
                        }
                    }
                    lastJobStrings[p] = curJobString;
                    lastJobSignificance[p] = curJobSignificant;
                }
                firstPass = false;
            }
        }

    }
}
