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

        private const float BUTTON_HEIGHT = 50f;
        private const float BUTTON_SPACE = 10f;


        public MainTabWindow_UberMicro()
        {
            //base.forcePause = true;
        }

        public override Vector2 InitialSize
        {
            get
            {
                //return base.InitialSize;
                return new Vector2(250f, 400f);
            }
        }

        public override MainTabWindowAnchor Anchor =>
            MainTabWindowAnchor.Right;

        public override void DoWindowContents(Rect canvas)
        {
            base.DoWindowContents(canvas);

            MapComponent_UberMicro component = Find.VisibleMap.GetComponent<MapComponent_UberMicro>();
            bool curEnabled = component.enabled;
            bool curDraftedWalking = component.draftedWalking;
            bool curColonistShotFinished = component.colonistShotFinished;
            bool curEnemyShotBegun = component.enemyShotBegun;

            for (int i = 0; i < 4; i++)
            {
                Rect nextButton = new Rect(canvas);
                nextButton.y = i * (BUTTON_HEIGHT + BUTTON_SPACE);
                nextButton.height = BUTTON_HEIGHT;

                string buttonLabel;
                switch (i)
                {
                    case 0:
                        buttonLabel = "Current Enabled is: ";
                        if (curEnabled)
                        {
                            buttonLabel += "ON";
                        }
                        else
                        {
                            buttonLabel += "OFF";
                        }
                        if (Widgets.ButtonText(nextButton, buttonLabel))
                        {
                            component.enabled = !curEnabled;
                        }
                        break;
                    case 1:
                        buttonLabel = "Current Drafted Walking is: ";
                        if (curDraftedWalking)
                        {
                            buttonLabel += "ON";
                        }
                        else
                        {
                            buttonLabel += "OFF";
                        }
                        if (Widgets.ButtonText(nextButton, buttonLabel))
                        {
                            component.draftedWalking = !curDraftedWalking;
                        }
                        break;
                    case 2:
                        buttonLabel = "Current Colonist Shot Finished is: ";
                        if (curColonistShotFinished)
                        {
                            buttonLabel += "ON";
                        }
                        else
                        {
                            buttonLabel += "OFF";
                        }
                        if (Widgets.ButtonText(nextButton, buttonLabel))
                        {
                            component.colonistShotFinished = !curColonistShotFinished;
                        }
                        break;
                    case 3:
                        buttonLabel = "Current Enemy Shot Begun is: ";
                        if (curEnemyShotBegun)
                        {
                            buttonLabel += "ON";
                        }
                        else
                        {
                            buttonLabel += "OFF";
                        }
                        if (Widgets.ButtonText(nextButton, buttonLabel))
                        {
                            component.enemyShotBegun = !curEnemyShotBegun;
                        }
                        break;
                }
            }
        }

    }
}
