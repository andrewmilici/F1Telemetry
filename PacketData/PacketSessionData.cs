using F1Telemetry.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F1Telemetry
{
    public class MarshalZone
    {
        public float m_zoneStart { get; set; }   // Fraction (0..1) of way through the lap the marshal zone starts
        public sbyte m_zoneFlag { get; set; }    // -1 = invalid/unknown, 0 = none, 1 = green, 2 = blue, 3 = yellow, 4 = red
    }

    public class PacketSessionData
    {
        public byte[] RawData { get; set; }
        public PacketHeader m_header { get; set; }

        public byte m_weather {get;set;}                // Weather - 0 = clear, 1 = light cloud, 2 = overcast
                                        // 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte m_trackTemperature {get;set;}        // Track temp. in degrees celsius
        public sbyte m_airTemperature {get;set;}          // Air temp. in degrees celsius
        public byte m_totalLaps {get;set;}              // Total number of laps in this race
        public ushort m_trackLength {get;set;}               // Track length in metres
        public byte m_sessionType {get;set;}            // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P
                                        // 5 = Q1, 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ
                                        // 10 = R, 11 = R2, 12 = Time Trial
        public sbyte m_trackId {get;set;}                 // -1 for unknown, 0-21 for tracks, see appendix
        public byte m_formula {get;set;}                    // Formula, 0 = F1 Modern, 1 = F1 Classic, 2 = F2,
                                            // 3 = F1 Generic
        public ushort m_sessionTimeLeft {get;set;}       // Time left in session in seconds
        public ushort m_sessionDuration {get;set;}       // Session duration in seconds
        public byte m_pitSpeedLimit {get;set;}              // Pit speed limit in kilometres per hour
        public byte m_gamePaused {get;set;}                // Whether the game is paused
        public byte m_isSpectating {get;set;}               // Whether the player is spectating
        public byte m_spectatorCarIndex {get;set;}          // Index of the car being spectated
        public byte m_sliProNativeSupport {get;set;}        // SLI Pro support, 0 = inactive, 1 = active
        public byte m_numMarshalZones {get;set;}            // Number of marshal zones to follow
        MarshalZone[] m_marshalZones {get;set;}             // List of marshal zones – max 21
        public byte m_safetyCarStatus {get;set;}            // 0 = no safety car, 1 = full safety car
                                                            // 2 = virtual safety car
        public byte m_networkGame {get;set;}               // 0 = offline, 1 = online



        public PacketSessionData()
        {
        }

        public PacketSessionData(byte[] rawData)
        {
            RawData = rawData;
            LoadData();
        }

        public void LoadData()
        {
            m_header = new PacketHeader();
            m_header.RawData = RawData;
            var currentPos = m_header.LoadData();

            using (var br = new BinaryReader(new MemoryStream(RawData)))
            {
                br.BaseStream.Position = currentPos;

                m_weather = br.ReadByte();
                m_trackTemperature = br.ReadSByte();
                m_airTemperature = br.ReadSByte();
                m_totalLaps = br.ReadByte();
                m_trackLength = br.ReadUInt16();
                m_sessionType = br.ReadByte();
                m_trackId = br.ReadSByte();
                m_formula = br.ReadByte();
                m_sessionTimeLeft = br.ReadUInt16();
                m_sessionDuration = br.ReadUInt16();
                m_pitSpeedLimit = br.ReadByte();
                m_gamePaused = br.ReadByte();
                m_isSpectating = br.ReadByte();
                m_spectatorCarIndex = br.ReadByte();
                m_sliProNativeSupport = br.ReadByte();
                m_numMarshalZones = br.ReadByte();

                m_marshalZones = new MarshalZone[m_numMarshalZones];

                for (int i = 0; i < m_numMarshalZones; i++)
                {
                    var newMZ = new MarshalZone();
                    newMZ.m_zoneStart = br.ReadSingle();
                    newMZ.m_zoneFlag = br.ReadSByte();
                    m_marshalZones[i] = newMZ;
                }

                m_safetyCarStatus = br.ReadByte();
                m_networkGame = br.ReadByte();

            }

        }
    }
}
