﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RemoteTech.Common
{
    public class RemoteTechCommonParams : GameParameters.CustomParameterNode
    {
        public enum RangeModel
        {
            [Description("Standard")]
            Standard,
            [Description("Root")]
            Root,
        }

        //[GameParameters.CustomStringParameterUI("", autoPersistance = false, lines = 2)]
        //public string description = "Core functionality";

        [GameParameters.CustomParameterUI("Range Model Mode", toolTip = "This setting controls how the game determines whether two antennas are in range of each other.\nRead more on our online manual about the difference for each rule.")]
        public RangeModel RangeModelType = RangeModel.Standard;

        [GameParameters.CustomParameterUI("Upgradeable Mission Control antennas", toolTip = "ON: Mission Control antenna range is upgraded when the Tracking Center is upgraded.\nOFF: Mission Control antenna range isn't upgradeable.")]
        public bool UpgradeableMissionControlAntennas = true;

        public double OmniRangeClampFactor = 100;
        public double DishRangeClampFactor = 1000;

        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines = 2)]
        public string visualDesc = " \n<b><u>Visual styles</u></b>";

        [GameParameters.CustomParameterUI("Hide Ground Stations completely", toolTip = "ON: Ground Stations will not be shown at all times.\nOFF: Ground Stations are shown conditionally.")]
        public bool HideGroundStationsFully = false;

        [GameParameters.CustomParameterUI("Hide Ground Stations behind planet or moon", toolTip = "ON: Ground Stations are occluded by the planet or moon, and are not visible behind it.\nOFF: Ground Stations are always shown (see range option below).")]
        public bool HideGroundStationsBehindBody = true;

        [GameParameters.CustomParameterUI("Hide Ground Stations at a defined distance", toolTip = "ON: Ground Stations will not be shown past a defined distance to the mapview camera.\nOFF: Ground Stations are shown regardless of distance.")]
        public bool HideGroundStationsOnDistance = true;
        public float DistanceToHideGroundStations = 8000000;

        [GameParameters.CustomParameterUI("Mouseover of Ground Stations", toolTip = "ON: Some useful information is shown when you mouseover a Ground Station on the map view or Tracking Station.\nOFF: Information isn't shown during mouseover.")]
        public bool ShowMouseOverInfoGroundStations = true;

        [GameParameters.CustomStringParameterUI("", autoPersistance = false, lines = 2)]
        public string cheatDesc = " \n<b><u>Cheats</u></b>";

        [GameParameters.CustomParameterUI("Connection required to control antennas", toolTip = "ON: antennas can be activated, deactivated and targeted without a connection.\nOFF: No control without a working connection.")]
        public bool ControlAntennaWithConnection = true;

        [GameParameters.CustomParameterUI("Planets and moons will block a signal", toolTip = "ON: Antennas and dishes will not need line-of-sight to maintain a connection, as long as they have adequate range and power.\nOFF: Antennas and dishes need line-of-sight to maintain a connection.")]
        public bool EnforceLineOfSight = true;

        public Color DishConnectionColor = new Color(1.00f, 0.70f, 0.03f, 1f);
        public Color OmniConnectionColor = new Color(0.55f, 0.51f, 0.40f, 1f);
        public Color ActiveConnectionColor = new Color(0.65f, 1.0f, 0.01f, 1f);
        public Color RemoteStationColorDot = new Color(1.0f, 0.0f, 0.0f, 1f);
        public Color DirectConnectionColor = new Color(0.0f, 0.749f,0.952f, 1f);

        public override string DisplaySection
        {
            get
            {
                return "RemoteTech";
            }
        }

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false;
            }
        }

        public override string Section
        {
            get
            {
                return "RemoteTech";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 0;
            }
        }

        public override string Title
        {
            get
            {
                return "Core";
            }
        }
    }
}
