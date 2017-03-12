using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace UberMicro
{
    public class MainTabWindow_UberMicro : MainTabWindow
    {
        public MainTabWindow_UberMicro()
        {
            //base.forcePause = true;
        }

        public override Vector2 InitialSize
        {
            get
            {
                //return base.InitialSize;
                return new Vector2(200f, 200f);
            }
        }

        public override MainTabWindowAnchor Anchor =>
            MainTabWindowAnchor.Right;

        public override void DoWindowContents(Rect canvas)
        {
            base.DoWindowContents(canvas);

            Rect topRow = new Rect(canvas);
            topRow.height = 50f;

            //Rect topRowLeft = new Rect(topRow);
            //topRowLeft.width = 200f;

            bool curMode = Find.VisibleMap.GetComponent<MapComponent_UberMicro>().enabled;
            string buttonLabel = "Current Mode is: ";
            if (curMode)
            {
                buttonLabel += "ON";
            }
            else
            {
                buttonLabel += "OFF";
            }
            if ( Widgets.ButtonText(topRow, buttonLabel) )
            {
                Find.VisibleMap.GetComponent<MapComponent_UberMicro>().enabled = !curMode;
            }
        }

    }
}
