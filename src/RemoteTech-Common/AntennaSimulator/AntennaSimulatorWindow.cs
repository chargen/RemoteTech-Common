﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteTech.Common.UI;
using UnityEngine;
using RemoteTech.Common.RemoteTechCommNet;
using UnityEngine.UI;
using RemoteTech.Common.Utils;

namespace RemoteTech.Common.AntennaSimulator
{
    public class AntennaSimulatorWindow : AbstractDialog
    {
        private enum SimContentType { RANGE, POWER, SCIENCE };
        private SimContentType whichContentOnDisplay;
        private DialogGUIVerticalLayout contentRows;

        private ElectricChargeReport ecReport;
        private List<ModuleDataTransmitter> antennaModules = new List<ModuleDataTransmitter>();

        public AntennaSimulatorWindow() : base("RemoteTech Antenna Simulator",
                                                0.8f,
                                                0.5f,
                                                450,
                                                450,
                                                new DialogOptions[] { DialogOptions.HideDismissButton, DialogOptions.AllowBgInputs})
        {
            whichContentOnDisplay = SimContentType.RANGE; // the section a player see for the first time
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> contentComponents = new List<DialogGUIBase>();

            string vesselName;
            if (HighLogic.LoadedSceneIsFlight)
                vesselName = FlightGlobals.ActiveVessel.vesselName;
            else
                vesselName = EditorLogic.fetch.ship.shipName;

            DialogGUILabel descrptionLabel = new DialogGUILabel(string.Format("Based on this vessel '{0}', a number of estimations are computed and displayed below.\n\n", vesselName), true, false);
            contentComponents.Add(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { descrptionLabel }));

            DialogGUIButton rangeButton = new DialogGUIButton("Antenna range", delegate { whichContentOnDisplay = SimContentType.RANGE; refreshContent(); }, false);
            DialogGUIButton scienceButton = new DialogGUIButton("Science data", delegate { whichContentOnDisplay = SimContentType.SCIENCE; refreshContent(); }, false);
            DialogGUIButton powerButton = new DialogGUIButton("Power system", delegate { whichContentOnDisplay = SimContentType.POWER; refreshContent(); }, false);
            DialogGUIButton refreshButton = new DialogGUIButton("Refresh", refreshContent, false);
            contentComponents.Add(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { rangeButton, scienceButton, powerButton, new DialogGUIFlexibleSpace(), refreshButton }));

            DialogGUIBase[] rows = new DialogGUIBase[] { new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) };
            contentRows = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, rows);
            contentComponents.Add(new DialogGUIScrollList(Vector2.one, false, true, contentRows));

            return contentComponents;
        }

        protected override void OnAwake(object[] args)
        {
            refreshContent();
        }

        private void registerContentComponents()
        {
            if (contentRows.children.Count <= 1)
                return;

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(contentRows.uiItem.gameObject.transform);
            for (int i = 1; i < contentRows.children.Count; i++)
            {
                contentRows.children[i].Create(ref stack, HighLogic.UISkin);
            }
        }

        private void deleteContentComponents()
        {
            if (contentRows.children.Count <= 1)
                return;

            int size = contentRows.children.Count;
            for (int i = size - 1; i >= 1; i--)
            {
                DialogGUIBase thisChild = contentRows.children[i];
                if (!(thisChild is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                {
                    contentRows.children.RemoveAt(i);
                    thisChild.uiItem.gameObject.DestroyGameObjectImmediate();
                }
            }
        }

        private void refreshContent()
        {
            List<Part> parts;
            if (HighLogic.LoadedSceneIsFlight)
                parts = FlightGlobals.ActiveVessel.Parts;
            else
                parts = EditorLogic.fetch.ship.Parts;

            ecReport = new ElectricChargeReport(parts);

            deleteContentComponents();
            switch (whichContentOnDisplay)
            {
                case SimContentType.RANGE:
                    displayRangeInfo(parts);
                    break;
                case SimContentType.POWER:
                    displayPowerInfo(parts);
                    break;
                case SimContentType.SCIENCE:
                    displayScienceInfo(parts);
                    break;
            }
            registerContentComponents();

            return;

            string activeVesselText = string.Format("Power storage: {0:0.0}/{1:0.0}\n", ecReport.currentCapacity, ecReport.maxCapacity - ecReport.lockedCapacity);

            /*
                int ecID = PartResourceLibrary.Instance.resourceDefinitions[ElectricChargeReport.ECName].id;
                FlightGlobals.ActiveVessel.GetConnectedResourceTotals(ecID, out currECAmt, out maxECAmt);

                if(currECAmt == maxECAmt)
                {
                    overallECRate = 0.0;
                }
                if ((prevCurrECAmt - currECAmt) != 0.00)
                {
                    overallECRate = (prevCurrECAmt - currECAmt) * -1.0 * ((1.0f/Time.deltaTime)>60? 60: (1.0f / Time.deltaTime));// Framerate is capped to 60 fps internally
                    prevCurrECAmt = currECAmt;
                }
            */

            double sourceComPower = 0, targetComPower = 0;
            double partialControlRangeMultipler = 0.2; //TODO: get from RT's CustomGameParams

            targetComPower = sourceComPower; // temp
            //leftVesselLabel.SetOptionText(activeVesselText);

            string targetVesselText = "<b>Target node</b>\n";
            targetVesselText += "Type: Com node or Target or DSN\n";
            targetVesselText += "Target: VESSEL12 (Distance: 1234 m) or Not applicable\n";
            targetVesselText += "Custom com power: 1234 or blank\n";

            //more options like presets of next DSN level?

            //rightVesselLabel.SetOptionText(targetVesselText);

            string resultText = "\n<b>Range simulation</b>\n";
            double maxPartialControlRange = RemoteTechCommNetScenario.RangeModel.GetMaximumRange(sourceComPower, targetComPower);
            double maxFullControlRange = maxPartialControlRange * (1.0 - partialControlRangeMultipler);

            resultText += string.Format("Full probe control range: {0} m\nPartial probe control range: {1} m\n", maxFullControlRange, maxPartialControlRange);
            resultText += "~ Graph ~\n";
            resultText += "--------------------------------------\n";
            resultText += "| YOU------------FC-------PC      TAR |\n";
            resultText += "--------------------------------------\n";
            resultText += "Color legend     GREEN--------------RED\n";
            resultText += "<color=red>Warning: Out of range</color>\n";

            //resultLabel.SetOptionText(resultText);            
        }

        //------------------------------------
        // Range section
        //------------------------------------
        private double totalComPower = 0.0;
        private double totalDrainPower = 0.0;

        private void displayRangeInfo(List<Part> parts)
        {
            DialogGUILabel message = new DialogGUILabel("<b>List of antennas detected:</b>", true, false);
            contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { message }));

            antennaModules.Clear();
            for (int i = 0; i < parts.Count; i++)
            {
                Part thisPart = parts[i];
                ModuleDataTransmitter antennaModule;
                if ((antennaModule = thisPart.FindModuleImplementing<ModuleDataTransmitter>()) != null)
                {
                    DialogGUIToggle toggleBtn = new DialogGUIToggle(false, thisPart.partInfo.title, delegate(bool b) { antennaSelected(b, antennaModules.Count); }, 130, 24);
                    DialogGUILabel comPowerLabel = new DialogGUILabel("Com power: "+ UiUtils.RoundToNearestMetricFactor(antennaModule.CommPower));
                    DialogGUILabel powerConsLabel = new DialogGUILabel(string.Format("Drain: {0:0.00} charge/s", antennaModule.DataResourceCost));
                    contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleBtn, comPowerLabel, powerConsLabel }));

                    antennaModules.Add(antennaModule);
                }
            }

            DialogGUILabel combinablePower = new DialogGUILabel(getCombinablePowerMessage, true, false);
            DialogGUILabel powerWarning = new DialogGUILabel(getWarningPowerMessage, true, false);
            contentRows.AddChild(new DialogGUIVerticalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { combinablePower, powerWarning }));
        }

        private void antennaSelected(bool toggleState, int indexAntenna) // problem: indexantenna is locked to 4
        {
            if (toggleState)
            {
                totalComPower += antennaModules[indexAntenna].CommPower;
                totalDrainPower += antennaModules[indexAntenna].DataResourceCost;
            }
            else
            {
                totalComPower -= antennaModules[indexAntenna].CommPower;
                totalDrainPower -= antennaModules[indexAntenna].DataResourceCost;
            }
        }

        private string getCombinablePowerMessage()
        {
            return "Combinable com power: "+UiUtils.RoundToNearestMetricFactor(totalComPower);
        }

        private string getWarningPowerMessage()
        {
            if (true)
                return string.Format("<color=red>Warning:</color> Estimated to run out of usable power in {0:0.0} seconds", (ecReport.currentCapacity-ecReport.lockedCapacity)/totalDrainPower);
            else
                return "Sustainable";
        }

        //------------------------------------
        // Science section
        //------------------------------------

        private void displayScienceInfo(List<Part> parts)
        {
            if (ResearchAndDevelopment.Instance == null)
            {
                DialogGUILabel message = new DialogGUILabel("The research and development facility is closed.", true, false);
                contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { message }));
            }
            else
            {
                DialogGUILabel message = new DialogGUILabel("<b>List of science experiments available:</b>", true, false);
                contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { message }));

                List<string> experimentIDs = ResearchAndDevelopment.GetExperimentIDs();
                for (int j = 0; j < experimentIDs.Count; j++)
                {
                    ScienceExperiment thisExp = ResearchAndDevelopment.GetExperiment(experimentIDs[j]);

                    DialogGUIToggle toggleBtn = new DialogGUIToggle(false, string.Format("{0} - {1} Mits", thisExp.experimentTitle, thisExp.dataScale * thisExp.baseValue) , null);
                    contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleBtn }));
                }

                DialogGUIToggle customToggleBtn = new DialogGUIToggle(false, "Custom science data (Mits)", null, 120, 24);
                DialogGUITextInput sizeInput = new DialogGUITextInput("", false, 5, null);
                contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { customToggleBtn, sizeInput, new DialogGUIFlexibleSpace() }));

                //scienceText += string.Format("Science bonus from the signal strength: {0:0.0}%\n", 12.2);
                //scienceText += string.Format("Total power required: {0:0.00} charges for {1:0.00} seconds\n", 1234, 1234);
                //scienceText += "<color=red>Warning: Insufficient power to transmit the largest science data (123 Mits)</color>\n";
            }
        }

        //------------------------------------
        // Power section
        //------------------------------------

        private void displayPowerInfo(List<Part> parts)
        {
            DialogGUIButton btn = new DialogGUIButton("POWER!!!", null, false);
            contentRows.AddChild(btn);
        }
    }
}
