using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace UberMicro
{
    class MapComponent_UberMicro : MapComponent
    {
        public bool enabled;
        private Dictionary<Pawn, string> lastPrioritizedJobs;

        public MapComponent_UberMicro(Map map) : base(map)
        {
            this.enabled = true;
            this.lastPrioritizedJobs = new Dictionary<Pawn, string>();
        }

        public override void MapComponentTick()
        {
            if (enabled)
            {
                foreach (Pawn p in map.mapPawns.FreeColonistsSpawned)
                {
                    if (p.CurJob.playerForced)
                    {
                        lastPrioritizedJobs[p] = p.CurJob.ToString();
                    }
                    else
                    {
                        string lastPrioritizedJob = null;
                        if (lastPrioritizedJobs.TryGetValue(p, out lastPrioritizedJob) && lastPrioritizedJob != null)
                        {
                            Find.LetterStack.ReceiveLetter(
                                    "Job Done",
                                    p.NameStringShort + " has completed their prioritized job to: " + lastPrioritizedJobs[p],
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
}
